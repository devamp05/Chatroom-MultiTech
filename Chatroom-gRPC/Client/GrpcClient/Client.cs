using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Core;
namespace Client;

class Client
{
    // static string userId = ""; // Unique identifier for the current client
    static string baseUrl;

    static async Task ClientHandler(string line, ChatRoomServer.ChatRoomServerClient client)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        string[] words = line.Split(' ');
        string command = words[0].ToLower();

        try
        {
            switch (command)
            {
                case "list":
                    await ListChatRooms(client);
                    break;

                case "create":
                    if (words.Length < 2)
                        Console.WriteLine("Usage: create roomName");
                    else
                        CreateChatRoom(client, words[1]);
                    break;

                case "join":
                    if (words.Length < 2)
                        Console.WriteLine("Usage: join roomName");
                    else
                        JoinChatRoom(client, words[1]);
                    break;

                case "leave":
                    if (words.Length < 2)
                        Console.WriteLine("Usage: leave roomName");
                    else
                        await LeaveChatRoom(client, words[1]);
                    break;

                case "send":
                    if (words.Length < 3)
                        Console.WriteLine("Usage: send roomName message");
                    else
                        await SendMessage(client, words[1], string.Join(" ", words.Skip(2)));
                    break;

                default:
                    Console.WriteLine("Unknown command. Please try again.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // Main function to handle the client logic
    static async Task Main(string[] args)
    {
        // get base url for this container from our env variable
        baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
        Console.WriteLine(baseUrl);
        // Establish a channel to the gRPC server
        using var channel = GrpcChannel.ForAddress(baseUrl);
        var client = new ChatRoomServer.ChatRoomServerClient(channel);

        // // Generate a unique userId for this client
        // userId = GenerateUserId();
        // Console.WriteLine($"Client registered with ID: {userId}\n");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Commands supported for client and its arguments:");
            Console.WriteLine("list (lists all available chat rooms on the server)");
            Console.WriteLine("create roomName (to create and join a new chat room with name roomName)");
            Console.WriteLine("join roomName (to join an existing chat room)");
            Console.WriteLine("leave roomName (to leave a chat room)");
            Console.WriteLine("send roomName message (to send a message to a chat room)");
            Console.WriteLine();
            // Console.Write("\nEnter command: ");
            string? line = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            // upon receiving a line from console, let our thread handle it
            Thread handler = new Thread(() => ClientHandler(line, client));
            handler.Start();
        }
    }

    // // Generate a unique user ID
    // static string GenerateUserId()
    // {
    //     var rand = new Random();
    //     return new string('A', 16).Select(_ => (char)(rand.Next(26) + 'A')).ToArray();
    // }

    // List all available chat rooms
    static async Task ListChatRooms(ChatRoomServer.ChatRoomServerClient client)
    {
        var response = await client.ListAllAsync(new ListRequest());
        Console.WriteLine(response.Rooms);
    }

    // Create a new chat room
    static async Task CreateChatRoom(ChatRoomServer.ChatRoomServerClient client, string roomName)
    {
        // var response = await client.CreateRoomAsync(new CreateRoomRequest { RoomName = roomName });
        // Console.WriteLine(response.Chat);

        var request = new CreateRoomRequest { RoomName = roomName };

        var response = await client.CreateRoomAsync(request);
        Console.WriteLine(response.Result);
        // try
        // {
        //     // Console.WriteLine($"Joined chat room: {roomName}. Listening for messages...");

        //     // Read responses from the stream
        //     // while (await responseStream.ResponseStream.MoveNext())
        //     // {
        //     //     var response = responseStream.ResponseStream.Current;
        //     //     Console.WriteLine(response.Chat);
        //     // }
        //     await foreach (var response in responseStream.ResponseStream.ReadAllAsync())
        //     {
        //         // Console.WriteLine("Greeting: " + response.Message);
        //         // "Greeting: Hello World" is written multiple times
        //         var ServerResponse = responseStream.ResponseStream.Current;
        //         Console.WriteLine(ServerResponse.Chat);
        //         // Console.WriteLine("new message came!");
        //     }
        //     // Console.WriteLine("hope its not exiting");
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"Error occurred: {ex}");
        // }
    }

    // Join an existing chat room
    static async Task JoinChatRoom(ChatRoomServer.ChatRoomServerClient client, string roomName)
    {
        // var response = await client.JoinChatRoomAsync(new JoinChatRoomRequest { RoomName = roomName });
        // Console.WriteLine(response.Chat);
        
        var request = new JoinChatRoomRequest { RoomName = roomName };

        // using var responseStream = client.JoinChatRoom(request);
        using var responseStream = client.JoinChatRoom(request);
        try
        {
            // Console.WriteLine($"Joined chat room: {roomName}. Listening for messages...");

            // Read responses from the stream
            // while (await responseStream.ResponseStream.MoveNext(CancellationToken.None))
            // while (await responseStream.ResponseStream.MoveNext())
            // {
            //     var response = responseStream.ResponseStream.Current;
            //     Console.WriteLine(response.Chat);
            // }
            await foreach (var response in responseStream.ResponseStream.ReadAllAsync())
            {
                // Console.WriteLine("Greeting: " + response.Message);
                // "Greeting: Hello World" is written multiple times
                var ServerResponse = responseStream.ResponseStream.Current;
                Console.WriteLine(ServerResponse.Chat);
                // Console.WriteLine("new message came!");
            }
            // Console.WriteLine("hope its not exiting");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex}");
        }

    }

    // Leave a chat room
    static async Task LeaveChatRoom(ChatRoomServer.ChatRoomServerClient client, string roomName)
    {
        var response = await client.LeaveChatRoomAsync(new LeaveChatRoomRequest { RoomName = roomName });
        // Console.WriteLine(response.);
    }

    // Send a message to a chat room
    static async Task SendMessage(ChatRoomServer.ChatRoomServerClient client, string roomName, string message)
    {
        var response = await client.SendAsync(new SendRequest { RoomName = roomName, Message = message });
        if(response.Chat == "Incorrect Room Name!")
        {
            Console.WriteLine(response.Chat);
        }
    }
}