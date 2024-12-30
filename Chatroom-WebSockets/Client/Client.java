import java.io.*;
import java.net.*;


public class Client {
    // declare 2 static inner classes to use as thread functions for sender and receiver
    public static class ClientReceiver extends Thread
    {
        public Socket socket;

        public ClientReceiver(Socket socket) 
        {
            this.socket = socket;
        }
        @Override
        public void run()
        {      
            // receiver receives stream from the server and prints it on the screen

            // first get our write stream
            BufferedWriter writer = new BufferedWriter(new OutputStreamWriter(System.out));

            InputStreamReader receiver;

            while (true)
            {
                try
                {
                    // first get a receiver stream from the socket connection
                    receiver = new InputStreamReader(socket.getInputStream());

                    // then wait to receive any input from the server
                    char[] data = new char[100000];
                    receiver.read(data);


                    // then write the received data to the output stream
                    writer.write("\n");
                    writer.write(data);
                    writer.write("\n");
                    //now write supported commands for users convinence
                    writer.write("Commands supported for client and its arguments:\n" +
                            "list (lists all available chat rooms on the server)\n" +
                            "create roomName (to create and join a new chat room with name roomName)\n" +
                            "join roomName (to join an existing room named chat room)\n" +
                            "leave roomName (to leave a roomName named chat room)\n" +
                            "send roomName message (to send message to chat room named roomName)\n");
                    writer.flush();
                }
                catch(IOException e)
                {
                    System.out.println("error while writing to output stream " + e);
                }
            }
        }
    }
    
    public static class ClientSender extends Thread
    {
        public Socket socket;
        public ClientSender(Socket socket)
        {
            this.socket = socket;
        }
        @Override
        public void run()
        {
            // in client sender we wait for users input and once we get it we send it to the server
            BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));

            // get our output stream for sending the line we read
            PrintStream sendStream;
            while (true) 
            {
                try 
                {
                    // first get a senderstream from our socket connection
                    sendStream = new PrintStream(socket.getOutputStream());

                    // then read a line from the terminal
                    // System.out.print("Enter your command: ");
                    // System.out.flush();
                    String line = reader.readLine();

                    // then send the line just read to the server and repeat
                    sendStream.println(line);   // will have to use println here because it doesn't recognize the end without newline character
                    // because we are using ReadLine on the server side.

                } catch (IOException e) {
                    System.out.println("error while creating printStream or reading input" + e);
                }
            }
        }
    }

    public static void main(String[] Args) {
        try {
            System.out.print("Commands supported for client and its arguments:\n" +
                            "list (lists all available chat rooms on the server)\n" +
                            "create roomName (to create and join a new chat room with name roomName)\n" +
                            "join roomName (to join an existing room named chat room)\n" +
                            "leave roomName (to leave a roomName named chat room)\n" +
                            "send roomName message (to send message to chat room named roomName)\n"
                        );

            // create server socket
            // Socket s = new Socket("host.docker.internal",80);
            boolean connected = false;
            Socket s = null;
            while (!connected)
            {
                // keep trying to connect while not connected
                try 
                {
                    s = new Socket("cs", 91);
                    connected = true;
                } catch (IOException e) {
                }
            }

            // create the client receiver thread
            ClientReceiver receiver = new ClientReceiver(s);

            // and client sender thread
            ClientSender sender = new ClientSender(s);

            // start our sender and receiver threads
            sender.start();
            receiver.start();

            // our main thread just has to wait for sender and receiver to finish now which wont happen for now as we end our program with control c
            sender.join();
            receiver.join();

        } catch (InterruptedException E) {
            System.out.println("Socket connection error!"+ E);
        }
    }
}