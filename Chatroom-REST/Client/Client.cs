using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;

class Client
{
    // to store current clients userId or userName
    static string userId = "";

    // client side chatrooms
    static Dictionary<string, string> ChatRooms = new Dictionary<string, string>();

    // client 
    static HttpClient client = new HttpClient();
        
    static string baseUrl = "";

    // A thread function to handle clients
    static async Task ClientHandler(string line)
    {
        // break string into an array so that words can directly be indexed
        string[] words = line.Split(' ');

        Console.Write("\n");
        // Write switch case statements to see what action does the client wants to perform
        switch (words[0])
        {
            case "list":
                try
                {
                    var response = await client.GetAsync($"{baseUrl}/rooms");

                    // Ensure the response indicates success
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a single string
                    var result = await response.Content.ReadAsStringAsync();

                    Console.Write(result);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP request error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
                break;

            case "join":
                if(words.Length != 2)
                {
                    Console.Write("Need to specify RoomName.\nPlease try again!\n");
                }
                else
                {
                    var data = new { RoomName = words[1], Writer = userId };
                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    string result;
                    try
                    {
                        var response = await client.PostAsync($"{baseUrl}/rooms/join", content);
                        result = await response.Content.ReadAsStringAsync();
                        Console.Write(result);
                        if(result != "No such room exists!\n")
                        {
                            ChatRooms.Add(words[1], result);
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Request failed: {ex.Message}");
                        result = "Error";
                    }
                }
                break;
            
            case "send":
                if(words.Length < 3)
                {
                    Console.Write("Need to specify 3 arguments: send roomName message for sending a message.\nPlease try again!\n");
                }
                else
                {
                    var data = new { Message = string.Join(" ", words.Skip(2)), Writer = userId };
                    string room = words[1];
                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    string result;
                    try
                    {
                        var response = await client.PostAsync($"{baseUrl}/rooms/{room}/messages", content);
                        result = await response.Content.ReadAsStringAsync();
                        Console.Write(result);
                        if(result != "Not a member!\n" && result != "Incorrect Roomname!\n")
                        {
                            // Console.WriteLine("coming here");
                            // add the result to our copy of chatrooms
                            ChatRooms[room] = result;
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Request failed: {ex.Message}");
                        result = "Error";
                    }
                }
                break;

            case "create":
                if(words.Length != 2)
                {
                    Console.Write("Need to specify RoomName to create.\nPlease try again!\n");
                }
                else
                {
                    var data = new { RoomName = words[1], Writer = userId };
                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    string result;
                    try
                    {
                        var response = await client.PostAsync($"{baseUrl}/rooms", content);
                        result = await response.Content.ReadAsStringAsync();
                        ChatRooms.Add(words[1], result);
                        Console.Write(result);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Request failed: {ex.Message}");
                        result = "Error";
                    }
                }
                break;
            
            case "leave":
                if(words.Length != 2)
                {
                    Console.Write("Need to specify RoomName to leave.\nPlease try again!\n");
                }
                else
                {
                    string room = words[1];
                    string user = userId;

                    var response = await client.DeleteAsync($"{baseUrl}/rooms/{room}/users/{user}");

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("User removed successfully.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("Room not found or Current user is not a member of given room.");
                    }
                    else
                    {
                        Console.WriteLine($"Unexpected error: {response.StatusCode}");
                    }
                }
                break;

            default:
                Console.WriteLine("Please try again!\n");
                break;
        }
        Console.Write("\n\n\n");
        Console.Write("Commands supported for client and its arguments:\n" +
                            "list (lists all available chat rooms on the server)\n" +
                            "create roomName (to create and join a new chat room with name roomName)\n" +
                            "join roomName (to join an existing room named chat room)\n" +
                            "leave roomName (to leave a roomName named chat room)\n" +
                            "send roomName message (to send message to chat room named roomName)\n");
    }

    static async Task CheckUpdates()
    {
        int pollIntervalInSeconds = 5;
        while (true)
        {
            // Poll the server to get the messages for the user
            var response = await client.GetAsync($"{baseUrl}/users/{userId}/messages");

            if (response.IsSuccessStatusCode)
            {
                // // Read and display the response from the server
                // var messages = await response.Content.ReadAsStringAsync();
                // Console.WriteLine("Received messages:");
                // Console.Write(messages);

                // Read and parse the response from the server
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the response into a dictionary
                var updatedChatRooms = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse);

                if (updatedChatRooms != null)
                {
                    // Update the local ChatRooms dictionary
                    foreach (var room in updatedChatRooms)
                    {
                        if (ChatRooms.ContainsKey(room.Key))
                        {
                            if(ChatRooms[room.Key].Trim() != room.Value.Trim())
                            {
                                // it means a new message has ben sent to this room
                                // Console.Write("chatroom value:" + ChatRooms[room.Key] + " \n got:" + room.Value);

                                // so notify our user
                                Console.Write(room.Value);

                                // update locally
                                ChatRooms[room.Key] = room.Value;
                            }
                        }
                        else
                        {
                            // I don't think it will come here as we already add room to our local chatroom in send
                            // when we create it but just in case
                            // Console.Write(room.Value);
                            // Console.Write("hi\n");

                            // Console.Write(ChatRooms[room.Key]);
                            // Console.Write("ello\n");

                            // Console.Write(ChatRooms[room.Key].Trim() != room.Value.Trim());

                            Console.Write(room.Value);

                            ChatRooms.Add(room.Key, room.Value);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }

            // Wait for the specified interval before sending the next request
            await Task.Delay(pollIntervalInSeconds * 1000); // Delay is in milliseconds
        }
    }

    static async Task Main(string[] args)
    {

        baseUrl = "http://cs-server:8080";

        // first generate a random string for current user and register it in the system

        string result = new string("");

        // Creating object of random class 
        Random rand = new Random(); 

        // id will be of fixed length: 16 characters
        int stringlen = 16;
        string str = "";
        int randValue;
        while (true)
        {
            str = "";
            char letter; 
            for (int i = 0; i < stringlen; i++) 
            { 

                // Generating a random number. 
                randValue = rand.Next(0, 26); 

                // Generating random character by converting 
                // the random number into character. 
                letter = Convert.ToChar(randValue + 65); 

                // Appending the letter to string. 
                str = str + letter; 
            }

            var data = new { Username = str };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
    

            try
            {
                var response = await client.PostAsync($"{baseUrl}/users/register", content);
                result = await response.Content.ReadAsStringAsync();
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
                result = "Error";
            }
        }
        // assign the new username to our static variable
        userId = str;

        // start our polling thread
        Thread updates = new Thread(() => CheckUpdates());
        updates.Start();
        // after this the user registration was successful
        Console.Write("Commands supported for client and its arguments:\n" +
                            "list (lists all available chat rooms on the server)\n" +
                            "create roomName (to create and join a new chat room with name roomName)\n" +
                            "join roomName (to join an existing room named chat room)\n" +
                            "leave roomName (to leave a roomName named chat room)\n" +
                            "send roomName message (to send message to chat room named roomName)\n");
        while(true)
        {
            string line = Console.ReadLine();
            // upon receiving a line from console, let our thread handle it
            Thread handler = new Thread(() => ClientHandler(line));
            handler.Start();
        }

    }
}