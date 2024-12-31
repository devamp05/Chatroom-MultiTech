package chatroom;
import java.rmi.Remote;
import java.rmi.RemoteException;

public interface Caller extends Remote
{
    public void call(String message) throws RemoteException;
}