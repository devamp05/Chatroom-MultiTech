# Chatroom-RMI

This folder contains the implementation of the chatroom application using **Java RMI**.

## Features
- Implements chatroom functionality using Java Remote Method Invocation (RMI).
- Demonstrates the use of remote objects for inter-process communication.
- Fully Dockerized for seamless deployment on multiple machines.
- Supports at least 10 concurrent clients.

### Key Concepts
- **RMI Registry**: Allows clients to locate remote server objects.
- **Remote Methods**: Enables invocation of methods on objects located on a remote JVM.

## How to Run
1. Build the Docker image:
   ```bash
   docker-compose build
2. Start the Server and Client containers
   ```bash
   docker-compose up
3. Attach to the client container
   ```bash
   docker attach java-client
4. Run client
   ```bash
   java chatroom.Client java-server
