package chatroom;
// import chatroom.ChatRoom;
import java.rmi.RemoteException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Random;

public class ChatRoomObject implements ChatRoom {

    // Create a hashmap that stores strings or messages of a chatroom with their names
    private HashMap<String, String> chatRooms;

    // Another hashmap to store chatroom name and a list of member ids
    private HashMap<String, ArrayList<String>> members;

    // a hashmap to map clientID with its caller
    private HashMap<String, Caller> clientIdentifier;

    // A list to keep track of all the registered members 
    private ArrayList<String> registeredMembers;

    public ChatRoomObject()
    {
        // in the constructor initialize our hashmaps
        this.chatRooms = new HashMap<>();
        this.members = new HashMap<>();
        this.clientIdentifier = new HashMap<>();
        this.registeredMembers = new ArrayList<>();
    }
    
    // return a string identifier which will act as an id for current client
    @Override
    public synchronized String registerClient(Caller client) throws RemoteException {
        // generate a randomized 10 character Alphabetic String (alphabetic because I think it will be easier to debug if I have to later) 
        int leftLimit = 97; // letter 'a'
        int rightLimit = 122; // letter 'z'
        int targetStringLength = 10;
        Random random = new Random();
    
        // randomly generate random strings which will serve as ids for members
        String generatedString = random.ints(leftLimit, rightLimit + 1)
          .limit(targetStringLength)
          .collect(StringBuilder::new, StringBuilder::appendCodePoint, StringBuilder::append)
          .toString();
        
        // while there is a member with current generated id alreaady registered
        while (registeredMembers.contains(generatedString))
        {
            // keep generating new random ids
            generatedString = random.ints(leftLimit, rightLimit + 1)
            .limit(targetStringLength)
            .collect(StringBuilder::new, StringBuilder::appendCodePoint, StringBuilder::append)
            .toString();
        }
        
        // map the clientID with its object
        clientIdentifier.put(generatedString, client);
        return generatedString;
    }

    @Override
    public synchronized void clientHandler(String[] line, String clientID) throws RemoteException {
        new Thread(() -> {
                switch (line[0])
            {
                case "list":
                {
                    try {
                        clientIdentifier.get(clientID).call(listAll());
                    } catch (RemoteException ex) {
                    }
                    break;
                }
                case "join":
                {
                    if(line.length != 2)
                    {
                        try {
                            clientIdentifier.get(clientID).call("Need to specify RoomName.\nPlease try again!\n");
                        } catch (RemoteException ex) {
                        }
                    }
                    else
                    {
                        String joinResult = "";
                        try {
                            joinResult = joinChatRoom(line[1], clientID);
                        } catch (RemoteException ex) {
                        }
                        if(joinResult.equals(""))
                        {
                            try {
                                clientIdentifier.get(clientID).call("Room does not exists.\nPlease try again!\n");
                            } catch (RemoteException ex) {
                            }
                            
                        }
                        else
                        {
                            try {
                                clientIdentifier.get(clientID).call(joinResult);
                            } catch (RemoteException ex) {
                            }
                        }
                    }
                    break;
                }
                case "send":
                {
                    if(line.length < 3)
                    {
                        try {
                            clientIdentifier.get(clientID).call("Need to specify 3 arguments: send roomName message for sending a message.\nPlease try again!\n");
                        } catch (RemoteException ex) {
                        }
                    }
                    else
                    {
                        try {
                            // call our handler for send
                            // string.Join(" ", words.Skip(2)) is the way for sending a string from second index of the array of strings words till end as a single string
                            String message = String.join(" ", Arrays.stream(line).skip(2).toArray(String[]::new));
                            if(!handleSend(line[1], message, clientID))
                            {
                                clientIdentifier.get(clientID).call("Sending failed because room with given room name doesn't exist or sender is not a member of that room.\nPlease try again!\n");
                            }
                        } catch (RemoteException ex) {
                        }
                    }
                    break;
                }
                case "create":
                {
                    if(line.length != 2)
                    {
                        try {
                            clientIdentifier.get(clientID).call("Need to specify RoomName to create.\nPlease try again!\n");
                        } catch (RemoteException ex) {
                        }
                    }
                    else
                    {
                        try {
                            if(createRoom(line[1], clientID))
                            {
                                clientIdentifier.get(clientID).call("Room Creation succeeded!\n");
                            }
                            else
                            {
                                clientIdentifier.get(clientID).call("Room Creation Failed!\nRoom name already exist\n");
                            }
                        } catch (RemoteException ex) {
                        }
                    }
                    break;
                }
                case "leave":
                {
                    if(line.length != 2)
                    {
                        try {
                            clientIdentifier.get(clientID).call("Need to specify RoomName to leave.\nPlease try again!\n");
                        } catch (RemoteException ex) {
                        }
                    }
                    else
                    {
                        try {
                            // call our handler for leaveroom
                            leaveChatRoom(line[1], clientID);
                        } catch (RemoteException ex) {
                        }
                    }
                    break;
                }
                default:
                {
                        try {
                            clientIdentifier.get(clientID).call("Please try again!\n");
                        } catch (RemoteException ex) {
                        }
                    break;
                }

            }
        }).start();
    }

    private synchronized String listAll() throws RemoteException {
        // create a new string builder
        StringBuilder rooms = new StringBuilder();

        // get roomnames from chatRooms and send them to the client or return them
        chatRooms.forEach((key, value) -> rooms.append(key).append("\n"));
        return rooms.toString();
    }

    private synchronized boolean createRoom(String roomName, String clientID) throws RemoteException {
        if(!chatRooms.containsKey(roomName))
        {
            // then create it and add client as a member
            chatRooms.put(roomName, "Room: " + roomName + "\n");

            ArrayList<String> writterArrayList = new ArrayList<>();

            writterArrayList.add(clientID);

            members.put(roomName, writterArrayList);
            return true;
        }
        else
        {
            return false;
        }
    }

    private synchronized String joinChatRoom(String roomName, String clientID) throws RemoteException {
        // if a ChatRoom with this name exists
        if(chatRooms.containsKey(roomName))
        {
            // then add current client as a member and return previous messages
            members.get(roomName).add(clientID);
            return chatRooms.get(roomName);
        }
        else
        {
            return "";
        }
    }

    private synchronized boolean handleSend(String roomName, String message, String clientID) throws RemoteException {
        if(chatRooms.containsKey(roomName))
        {
            // now check if current writer is member of the room
            if(members.get(roomName).contains(clientID))
            {
                // then add new message to the room
                String prevMessages = chatRooms.get(roomName);
                String newMessage = prevMessages += message += "\n";
                chatRooms.put(roomName, newMessage);

                // notify all the members of the new message in the room
                notifyAllMembers(roomName);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private  synchronized void notifyAllMembers(String roomName) throws RemoteException {
        members.get(roomName).forEach(clientID -> {
            try {
                // for each member of room roomName send new message
                clientIdentifier.get(clientID).call(chatRooms.get(roomName));
            } catch (RemoteException ex) {
                // it won't come here since we know that roomName exists as it is passed by one of our internal methods
            }
        });
    }

    private synchronized void leaveChatRoom(String roomName, String clientID) throws RemoteException {
        // remove the client from roomnames members list if it exists
        if (members.containsKey(roomName))
        {
            members.get(roomName).remove(clientID);
        }        
    }
}