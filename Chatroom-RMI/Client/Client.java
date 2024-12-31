package chatroom;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.rmi.RemoteException;
import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;
import java.rmi.server.UnicastRemoteObject;


public class Client extends UnicastRemoteObject implements Caller
{
    private ChatRoom room;

    private static String myId;

    public Client(ChatRoom Room) throws RemoteException
    {
        super();
        room = Room;
    }


    public class CommandReader extends Thread
    {
        @Override
        public void run()
        {
            // in client sender we wait for users input and once we get it we send it to the server
            BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));

            while (true) 
            {
                try 
                {
                    // read a line from the terminal
                    // System.out.print("Enter your command: ");
                    // System.out.flush();

                    // System.out.println("reached here\n");
                    String line = reader.readLine();

                    // after reading client call on our handler on the server and let it handle the command
                    room.clientHandler(line.split(" "), myId);


                } catch (IOException e) {
                    System.out.println("error while creating printStream or reading input" + e);
                }
            }
        }
    }

    @Override
    public void call(String message) throws RemoteException {
        new Thread(() -> {
            System.out.println();
            System.out.print(message);
            System.out.println();
            System.out.print("Commands supported for client and its arguments:\n" +
                            "list (lists all available chat rooms on the server)\n" +
                            "create roomName (to create and join a new chat room with name roomName)\n" +
                            "join roomName (to join an existing room named chat room)\n" +
                            "leave roomName (to leave a roomName named chat room)\n" +
                            "send roomName message (to send message to chat room named roomName)\n");
        }).start();
    }

    private void register() {
        try {
            myId = room.registerClient(this);
        } catch (RemoteException ex) {
            System.out.print(ex);
        }
    }

    private CommandReader startReading() {
        Client.CommandReader commandReader = new CommandReader();
        commandReader.start();
        return commandReader;
    }

    // public static void callme()
    // {
    //     // try {
    //     //     Thread.sleep(60000);
    //     //     System.out.println("finished sleeping\n");
    //     // } catch (InterruptedException ex) {
    //     // }
    //     new Thread(() -> {
    //         try {
    //             Thread.sleep(60000);
    //             System.out.println("finished sleeping\n");
    //         } catch (InterruptedException ex) {
    //         }
    //     }).start();
    // }


    public static void main(String[] Args) {

        String host = (Args.length < 1) ? null : Args[0];
        try {
        Registry registry = LocateRegistry.getRegistry(host);
        Client client = new Client(((ChatRoom) registry.lookup("ChatRoomServer")) );
        
        client.register();

        System.out.print("Commands supported for client and its arguments:\n" +
                            "list (lists all available chat rooms on the server)\n" +
                            "create roomName (to create and join a new chat room with name roomName)\n" +
                            "join roomName (to join an existing room named chat room)\n" +
                            "leave roomName (to leave a roomName named chat room)\n" +
                            "send roomName message (to send message to chat room named roomName)\n");

        client.startReading().join();

        } catch (Exception e) {
        System.err.println("Client exception: " + e.toString());
        e.printStackTrace();
        }
    }
}