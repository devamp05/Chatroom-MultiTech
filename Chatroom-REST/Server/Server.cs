class Server
{
    //initializing here because since they are static field, compiler was giving warning
    static Dictionary<string, string> ChatRooms = new Dictionary<string, string>();
    static Dictionary<string, List<string>> Members = new Dictionary<string, List<string>>();

    // a list containing all the users in the system
    static List<string> AllUsers = new List<string>();

    // a static object to use with monitor
    static Object Something = new();

    // just to be safe I have kept monitor enter even in readers
    // it can be avoided but then Ill have to check if there are any readers and when there are none then writer will write
    // but then can cause starvation to the writer if new read requests keep coming in so to keep it simple Ill just use monitor in all the 
    // thread helper functions both readers and writers

    // A function to handle lists
    static string ListAll()
    {
        // Monitor.Enter(Something);
        // string Rooms = "";
        // foreach(string RoomName in ChatRooms.Keys)
        // {
        //     Rooms += RoomName;
        //     Rooms += "\n";
        // }
        // Monitor.Exit(Something);
        // return Rooms;
        string result = string.Join("\n", ChatRooms.Keys);
        return result + "\n";
    }

    // A function to handle create rooms
    static string CreateRoom(string RoomName, string Writer)
    {
        Monitor.Enter(Something);
        // if no such ChatRoom already exists
        if(!ChatRooms.ContainsKey(RoomName))
        {
            // then create it and add client as a member
            ChatRooms[RoomName] = "Room: " + RoomName + "\n";
            List<string> WritersList = [];
            Members[RoomName] = WritersList;
            WritersList.Add(Writer);
            Monitor.Exit(Something);
            return ChatRooms[RoomName];
        }
        else
        {
            // if a room with given roomname already exists make current user a member of that room
            Members[RoomName].Add(Writer);
            Monitor.Exit(Something);
            return ChatRooms[RoomName];
        }
    }

    // A function to join an existing chat room
    static string JoinChatRoom(string RoomName, string writer)
    {
        Monitor.Enter(Something);
        // if a ChatRoom with this name exists
        if(ChatRooms.ContainsKey(RoomName))
        {
            // then add current writer as a member and return previous messages
            Members[RoomName].Add(writer);
            Monitor.Exit(Something);
            return ChatRooms[RoomName];
        }
        else
        {
            Monitor.Exit(Something);
            return "No such room exists!\n";
        }
    }

    // A function to handle sends
    static string HandleSend(string RoomName, string message, string writer)
    {
        Monitor.Enter(Something);
        // if a ChatRoom with this name exists
        if(ChatRooms.ContainsKey(RoomName))
        {
            // now check if current writer is member of the room
            if(Members[RoomName].Contains(writer))
            {
                // then add new message to the room
                ChatRooms[RoomName] += message;
                ChatRooms[RoomName] += "\n";

                // notify all the members of the new message in the room
                // NotifyAllMembers(RoomName);
                Monitor.Exit(Something);
                return ChatRooms[RoomName];
            }
            else
            {
                Monitor.Exit(Something);
                return "Not a member!\n";
            }
        }
        else
        {
            Monitor.Exit(Something);
            return "Incorrect Roomname!\n";
        }
    }

    // A function to leave a chat room
    static void LeaveChatRoom(string ChatRoom, string Writer)
    {
        Monitor.Enter(Something);
        // if current writer is a member of the chat room then remove it
        if(Members[ChatRoom].Contains(Writer))
        {
            Members[ChatRoom].Remove(Writer);
        }
        Monitor.Exit(Something);
    }

    // a function to get messages of a user from all the rooms that it is a member of
    static Dictionary<string, string> GetUserMessages(string Writer)
    {
        // Create a dictionary to store room names and their messages
        var result = new Dictionary<string, string>();

        // Loop through each room in Members
        foreach (var room in Members)
        {
            // Check if the Writer is part of the room
            if (room.Value.Contains(Writer))
            {
                // Add the room and its corresponding messages to the dictionary
                result[room.Key] = ChatRooms[room.Key];
            }
        }

        // Return the dictionary of room names and messages
        return result;
    }


    static void Main(string[] args)
    {
        // initial setup generated with the project template
        var builder = WebApplication.CreateBuilder(args);

        // Set the application URL to listen on http://0.0.0.0:8080
        builder.WebHost.UseUrls("http://0.0.0.0:8080");

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpsRedirection();

        // RESTful services
        app.MapGet("/", () => "Hello World!");

        // A service to register a client into the system (to make sure no 2 clients end up having the same id)
        app.MapPost("/users/register", (RegisterRequest request) => {
            if (AllUsers.Contains(request.Username))
            {
                return Results.BadRequest("Username already exists, try again!\n");
            }
            AllUsers.Add(request.Username);
            return Results.Ok("Registration successful!\n");
        });

        // A service to get a list of all the room names in the server
        app.MapGet("/rooms", () => ListAll());

        // A service to create a room
        app.MapPost("/rooms", (RoomRequest request) => CreateRoom(request.RoomName, request.Writer));

        // A service to join a room
        app.MapPost("/rooms/join", (RoomRequest request) => JoinChatRoom(request.RoomName, request.Writer));

        // A service to send a message to a chat room
        app.MapPost("/rooms/{room}/messages", (string room, MessageRequest request) => HandleSend(room, request.Message, request.Writer));

        // A service for a user to leave a chatroom
        app.MapDelete("/rooms/{room}/users/{user}", (string room, string user) => {
            // delete if a room with roomname room exists
            if(Members.ContainsKey(room) && Members[room].Contains(user))
            {
                LeaveChatRoom(room, user);
                return Results.Accepted();
            }
            else
            {
                return Results.NotFound();
            }
        });

        // A service to get messages from all of the rooms that a user is registered in
        app.MapGet("/users/{user}/messages", (string user) => GetUserMessages(user));

        // used this for debugging
        // app.Use(async (context, next) =>
        // {
        //     Console.WriteLine($"Request received: {context.Request.Method} {context.Request.Path}");
        //     await next();
        // });


        app.Run();
    }
}

// Define the RoomRequest class
public class RoomRequest
{
    public string RoomName { get; set; }
    public string Writer { get; set; }
}

// Define the MessageRequest class
public class MessageRequest
{
    public string Message { get; set; }
    public string Writer { get; set; }
}


// Define the RegisterRequest class
public class RegisterRequest
{
    public string Username { get; set; }
}