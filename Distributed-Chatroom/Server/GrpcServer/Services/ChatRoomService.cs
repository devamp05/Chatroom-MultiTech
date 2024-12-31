using Grpc.Core;
using Grpc.Net.Client;
namespace GrpcServer.Services;

public class ChatRoomService : ChatRoomServer.ChatRoomServerBase
{
    private static readonly Dictionary<string, string> ChatRooms = new();
    private static readonly Dictionary<string, List<IServerStreamWriter<SendResponse>>> Members = new();
    private static readonly object LockObject = new();
    private static string CoordinatorAddress = "http://grpc-server-coordinator:5063"; // set coordinator address manually for now

    // to check if current server instance is the coordinator 
    private static bool IsCoordinator;

    private static readonly List<string> OtherServers = new List<string> { "http://grpc-server-two:5063", "http://grpc-server-three:5063" };

    private readonly ILogger<ChatRoomService> _logger;

    public ChatRoomService(ILogger<ChatRoomService> logger)
    {
        _logger = logger;
        CheckCoordinator();
    }


    private static void CheckCoordinator()
    {
        IsCoordinator = Environment.GetEnvironmentVariable("SERVER_ADDRESS") == CoordinatorAddress;
        // Console.WriteLine(IsCoordinator);
    }

    public override Task<ListResponse> ListAll(ListRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ListResponse
        {
            Rooms = string.Join("\n", ChatRooms.Keys) + "\n"
        });
    }

    
    public override Task<CreateRoomResponse> CreateRoom(CreateRoomRequest request, ServerCallContext context)
    {
        string roomName = request.RoomName;
        bool Success = false;

        lock (LockObject)
        {
            if (!ChatRooms.ContainsKey(roomName))
            {
                ChatRooms[roomName] = $"Room: {roomName}\n";
                // Members[roomName] = new List<IServerStreamWriter<SendResponse>>();
                Success = true;
            }

            // Members[roomName].Add(responseStream);
        }
        if(Success)
        {
            if(IsCoordinator)
            {
                PropagateCreateRoom(request);
            }
            else
            {
                ForwardCreateRoomToCoordinator(request);
            }
            return Task.FromResult(new CreateRoomResponse
            {
                Result = "Success!",
            });
        }


        return Task.FromResult(new CreateRoomResponse
        {
            Result = "Room already exists!",
        });

        // // send initial response
        // await responseStream.WriteAsync(new SendResponse
        // {
        //     Chat = ChatRooms[roomName]
        // });

        // try
        // {
        //     // Keep the stream open
        //     while (!context.CancellationToken.IsCancellationRequested)
        //     {
        //         await Task.Delay(1000, context.CancellationToken); // Optional delay to avoid busy loop
        //     }
        // }
        // catch (OperationCanceledException)
        // {
        //     // Handle client disconnection
        //     lock (LockObject)
        //     {
        //         Members[roomName].Remove(responseStream);
        //     }
        // }
    }

    public override async Task JoinChatRoom(JoinChatRoomRequest request, IServerStreamWriter<SendResponse> responseStream, ServerCallContext context)
    {
        string roomName = request.RoomName;

        string result;
        lock (LockObject)
        {
            if (ChatRooms.ContainsKey(roomName))
            {
                result = ChatRooms[roomName];
                Members[roomName].Add(responseStream);
            }
            else
            {
                result = "No such room exists!\n";
            }
        }

        await responseStream.WriteAsync(new SendResponse
        {
            Chat = result
        });

        try
        {
            // Keep the stream open
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, context.CancellationToken); // Optional delay to avoid busy loop
            }
        }
        catch (OperationCanceledException)
        {
            // Handle client disconnection
            lock (LockObject)
            {
                Members[roomName].Remove(responseStream);
            }
        }
    }

    public override Task<SendResponse> Send(SendRequest request, ServerCallContext context)
    {
        string roomName = request.RoomName;
        string message = request.Message;

        string result;

        lock (LockObject)
        {
            if (ChatRooms.ContainsKey(roomName))
            {
                ChatRooms[roomName] += $"{message}\n";
                result = ChatRooms[roomName];
                Thread handler = new Thread(() => NotifySubscribers(roomName));
                handler.Start();
            }
            else
            {
                result = "Incorrect Room Name!";
                // subscribers = new List<IServerStreamWriter<SendResponse>>();
            }
        }

        if(result != "Incorrect Room Name!")
        {
            if(IsCoordinator)
            {
                PropagateSend(request);
            }
            else
            {
                ForwardSendToCoordinator(request);
            }
        }

        return Task.FromResult(new SendResponse
        {
            Chat = result
        });
    }

    private async void NotifySubscribers(string RoomName)
    {
        List<IServerStreamWriter<SendResponse>> subscribers;

        subscribers = new List<IServerStreamWriter<SendResponse>>(Members[RoomName]);

        string result;

        result = ChatRooms[RoomName];

        foreach (var subscriber in subscribers)
        {
            try
            {
                await subscriber.WriteAsync(new SendResponse
                {
                    Chat = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send message to a subscriber: {ex.Message}");
            }
        }
    }

    public override Task<LeaveChatRoomResponse> LeaveChatRoom(LeaveChatRoomRequest request, ServerCallContext context)
    {
        string roomName = request.RoomName;
        lock (LockObject)
        {
            if (Members.TryGetValue(roomName, out var members))
            {
                members.RemoveAll(writer => writer.Equals(context.GetHttpContext()));
            }
        }

        return Task.FromResult(new LeaveChatRoomResponse
        {

        });
    }

    // a method to propogate send request to the coordinator server which will then forward it to other servers
    private async Task ForwardSendToCoordinator(SendRequest request)
    {
        using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(CoordinatorAddress);
        var client = new GrpcServer.ChatRoomServer.ChatRoomServerClient(channel);
        await client.SendAsync(request);
    }

    // a method to propogate Create room request to other servers by the coordinator
    private async Task ForwardCreateRoomToCoordinator(CreateRoomRequest request)
    {
        using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(CoordinatorAddress);
        var client = new GrpcServer.ChatRoomServer.ChatRoomServerClient(channel);
        await client.CreateRoomAsync(request);
        // foreach (var server in OtherServers)
        // {
        //     // Establish a channel to other gRPC servers
        //     // using var channel = GrpcChannel.ForAddress(server);
        //     // var client = new ChatRoomServer.ChatRoomServerClient(channel);
        //     using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(CoordinatorAddress);
        //     var client = new GrpcServer.ChatRoomServer.ChatRoomServerClient(channel);
        //     await client.CreateRoomAsync(request);
        // }
    }

    // a method to propogate send request to other servers by the coordinator
    private async Task PropagateSend(SendRequest request)
    {
        foreach (var server in OtherServers)
        {
            // Establish a channel to other gRPC servers
            using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(server);
            var client = new GrpcServer.ChatRoomServer.ChatRoomServerClient(channel);
            await client.SendAsync(request);
        }
    }

    // a method to propogate Create room request to other servers by the coordinator
    private async Task PropagateCreateRoom(CreateRoomRequest request)
    {
        foreach (var server in OtherServers)
        {
            // Establish a channel to other gRPC servers
            // using var channel = GrpcChannel.ForAddress(server);
            // var client = new ChatRoomServer.ChatRoomServerClient(channel);
            using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(server);
            var client = new GrpcServer.ChatRoomServer.ChatRoomServerClient(channel);
            await client.CreateRoomAsync(request);
            Console.WriteLine("propogating createroom requests");
        }
    }
}
