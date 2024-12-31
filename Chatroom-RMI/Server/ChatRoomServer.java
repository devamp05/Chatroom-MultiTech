package chatroom;

import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;
import java.rmi.server.UnicastRemoteObject;

public class ChatRoomServer {
    public static void main(String args[])
    {
        // try {
        //     ChatRoomObject obj = new ChatRoomObject();
        //     ChatRoom stub = (ChatRoom) UnicastRemoteObject.exportObject(obj, 0);
        //     // Bind the remote object's stub in the registry
        //     Registry registry = LocateRegistry.getRegistry();
        //     registry.rebind("Hello", stub);
        //     System.out.println("Server ready");
        // } catch (Exception e) {
        //     System.err.println("Server exception: " + e.toString());
        //     e.printStackTrace();
        // }

        try {
            ChatRoomObject obj = new ChatRoomObject();
            ChatRoom stub = (ChatRoom) UnicastRemoteObject.exportObject(obj, 0);
        
            // Try to create the registry on port 1099
            try {
                LocateRegistry.createRegistry(1099);
                System.out.println("RMI registry created on port 1099");
            } catch (Exception e) {
                System.out.println("RMI registry already exists.");
            }
        
            // Bind the remote object's stub in the registry
            Registry registry = LocateRegistry.getRegistry("localhost", 1099);
            registry.rebind("ChatRoomServer", stub);
            System.out.println("Server ready");
        } catch (Exception e) {
            System.err.println("Server exception: " + e.toString());
            e.printStackTrace();
        }
        
    }
}