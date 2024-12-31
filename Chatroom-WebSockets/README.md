# Chatroom-WebSockets

This folder contains the implementation of the chatroom application using **WebSockets**.

## Features
- Real-time, full-duplex communication between clients and server.
- Low-latency message delivery.
- Fully Dockerized for deployment on multiple machines.

### Key Concepts
- **WebSocket Protocol**: Enables bidirectional communication over a single TCP connection.
- **Message Broadcasting**: Messages are broadcast to all connected clients in a chatroom.

## How to Run
1. Build the Docker image:
   ```bash
   docker-compose build
2. Start the Server and Client containers
   ```bash
   docker-compose up
3. Attach to one or more client container
   ```bash
   docker attach java-Client
   docker attach java-Client2
