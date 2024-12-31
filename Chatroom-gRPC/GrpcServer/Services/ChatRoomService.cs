using Grpc.Core;
namespace GrpcServer.Services;

public class ChatRoomService : ChatRoomServer.ChatRoomServerBase
{
    private static readonly Dictionary<string, string> ChatRooms = new();
    private static readonly Dictionary<string, List<IServerStreamWriter<SendResponse>>> Members = new();
    private static readonly object LockObject = new();


    private readonly ILogger<ChatRoomService> _logger;

    public ChatRoomService(ILogger<ChatRoomService> logger)
    {
        _logger = logger;
        // ChatRooms = new Dictionary<string, string>();
        // Members = new Dictionary<string, List<IServerStreamWriter<SendResponse>>>();
    }

    public override Task<ListResponse> ListAll(ListRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ListResponse
        {
            Rooms = string.Join("\n", ChatRooms.Keys) + "\n"
        });
    }

    public override async Task CreateRoom(CreateRoomRequest request, IServerStreamWriter<SendResponse> responseStream, ServerCallContext context)
    {
        string roomName = request.RoomName;

        lock (LockObject)
        {
            if (!ChatRooms.ContainsKey(roomName))
            {
                ChatRooms[roomName] = $"Room: {roomName}\n";
                Members[roomName] = new List<IServerStreamWriter<SendResponse>>();
            }

            Members[roomName].Add(responseStream);
        }

        // send initial response
        await responseStream.WriteAsync(new SendResponse
        {
            Chat = ChatRooms[roomName]
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
}
