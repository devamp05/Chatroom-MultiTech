namespace asn2
{

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;

    class Server
    {
        //initializing here because since they are static field, compiler was giving warning
        static Dictionary<string, string> ChatRooms = new Dictionary<string, string>();
        static Dictionary<string, List<StreamWriter>> Members = new Dictionary<string, List<StreamWriter>>();

        // a static object to use with monitor
        static Object Something = new();

        // A thread function to handle clients
        static void ClientHandler(TcpClient client)
        {
            // First create our reader writer streams
            StreamReader reader = new StreamReader(client.GetStream());
            StreamWriter writer = new StreamWriter(client.GetStream());

            // then handle client
            string line;
            while(true)
            {
                if((line = reader.ReadLine()) != null)
                {
                    // break string into an array so that words can directly be indexed
                    string[] words = line.Split(' ');

                    // Write switch case statements to see what action does the client wants to perform
                    switch (words[0])
                    {
                        case "list":
                            // call our list handler funciton
                            string ListAllResult = ListAll();
                            writer.Write(ListAllResult);
                            writer.Flush();
                            break;

                        case "join":
                            Console.WriteLine("join");
                            // call our join handler
                            if(words.Length != 2)
                            {
                                writer.Write("Need to specify RoomName.\nPlease try again!\n");
                                writer.Flush();
                            }
                            else
                            {
                                string JoinResult = JoinChatRoom(words[1], writer);
                                if(JoinResult == "")
                                {
                                    writer.Write("Room does not exists.\nPlease try again!\n");
                                    writer.Flush();
                                }
                                else
                                {
                                    writer.Write(JoinResult);
                                    writer.Flush();
                                }
                            }
                            break;
                        
                        case "send":
                            if(words.Length < 3)
                            {
                                writer.Write("Need to specify 3 arguments: send roomName message for sending a message.\nPlease try again!\n");
                                writer.Flush();
                            }
                            else
                            {
                                // call our handler for send
                                // string.Join(" ", words.Skip(2)) is the way for sending a string from second index of the array of strings words till end as a single string
                                if(!HandleSend(words[1], string.Join(" ", words.Skip(2)), writer))
                                {
                                    writer.Write("Sending failed because room with given room name doesn't exist or sender is not a member of that room.\nPlease try again!\n");
                                    writer.Flush();
                                }
                            }
                            break;

                        case "create":
                            if(words.Length != 2)
                            {
                                writer.Write("Need to specify RoomName to create.\nPlease try again!\n");
                                writer.Flush();
                            }
                            else
                            {
                                if(CreateRoom(words[1], writer))
                                {
                                    writer.Write("Room Creation succeeded!\n");
                                    writer.Flush();
                                }
                                else
                                {
                                    writer.Write("Room Creation Failed!\nRoom name already exist\n");
                                    writer.Flush();
                                }
                            }
                            break;
                        
                        case "leave":
                            if(words.Length != 2)
                            {
                                writer.Write("Need to specify RoomName to leave.\nPlease try again!\n");
                                writer.Flush();
                            }
                            else
                            {
                                // call our handler for leaveroom
                                LeaveChatRoom(words[1], writer);
                            }
                            break;

                        default:
                            writer.WriteLine("Please try again!\n");
                            writer.Flush();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Connection terminated.");
                    break;
                }
            }
        }

        // just to be safe I have kept monitor enter even in readers
        // it can be avoided but then Ill have to check if there are any readers and when there are none then writer will write
        // but then can cause starvation to the writer if new read requests keep coming in so to keep it simple Ill just use monitor in all the 
        // thread helper functions both readers and writers

        // A function to handle lists
        static string ListAll()
        {
            Monitor.Enter(Something);
            string Rooms = "";
            foreach(string RoomName in ChatRooms.Keys)
            {
                Rooms += RoomName;
                Rooms += "\n";
            }
            Monitor.Exit(Something);
            return Rooms;
        }

        // A function to handle create rooms
        static bool CreateRoom(string RoomName, StreamWriter Writer)
        {
            Monitor.Enter(Something);
            // if no such ChatRoom already exists
            if(!ChatRooms.ContainsKey(RoomName))
            {
                // then create it and add client as a member
                ChatRooms[RoomName] = "Room: " + RoomName + "\n";
                List<StreamWriter> WritersList = [];
                Members[RoomName] = WritersList;
                WritersList.Add(Writer);
                Monitor.Exit(Something);
                return true;
            }
            else
            {
                Monitor.Exit(Something);
                return false;
            }
        }

        // A function to join an existing chat room
        static string JoinChatRoom(string RoomName, StreamWriter writer)
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
                return "";
            }
        }

        // A function to handle sends
        static bool HandleSend(string RoomName, string message, StreamWriter writer)
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
                    NotifyAllMembers(RoomName);
                    Monitor.Exit(Something);
                    return true;
                }
                else
                {
                    Monitor.Exit(Something);
                    return false;
                }
            }
            else
            {
                Monitor.Exit(Something);
                return false;
            }
        }

        // A funciton to notify all the members to be used when there is a new message in a chat room
        static void NotifyAllMembers(string RoomName)
        {
            // dont need mon enter here because we will already be in the monitor when this will be called
            // here it is guranteed that RoomName exists because this function will be called by an internal method
            foreach(StreamWriter Writer in Members[RoomName])
            {
                // Writer.WriteLine("hello");
                Writer.Write(ChatRooms[RoomName]);
                Writer.Flush();
            }
        }

        // A function to leave a chat room
        static void LeaveChatRoom(string ChatRoom, StreamWriter Writer)
        {
            Monitor.Enter(Something);
            // if current writer is a member of the chat room then remove it
            if(Members[ChatRoom].Contains(Writer))
            {
                Members[ChatRoom].Remove(Writer);
            }
            Monitor.Exit(Something);
        }


        static void Main()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 91);
            server.Start();
            Console.WriteLine("Echo server started. Listening on port 91...");
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                // upon receiving a connection from a client, let our thread handle it
                Thread handler = new Thread(() => ClientHandler(client));
                handler.Start();
            }
        }
    }
}