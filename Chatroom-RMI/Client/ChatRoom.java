package chatroom;

import java.rmi.Remote;
import java.rmi.RemoteException;

// Define the public interface of our ChatRoom (or Server earlier) object
public interface ChatRoom extends Remote {
    // A method that will allow clients to register themselves and get an unique id which allows the server to 
    // know a message is coming from which client.
    String registerClient(Caller client) throws RemoteException;

    // A method that will handle all our client requests it should get a list of string which is the line typed by the user
    void clientHandler(String[] line, String clientID) throws RemoteException;
}