# Chatroom-gRPC

This folder contains the implementation of the chatroom application using **gRPC**.

## Features
- High-performance communication using gRPC.
- Supports unary and bidirectional streaming RPCs for real-time chat functionality.
- Dockerized for deployment across machines.

### Key Concepts
- **Protocol Buffers**: Used to define the service and message structures in `.proto` files.
- **Server side Streaming**: Real-time communication between clients and server.

## How to Run
1. Build the Docker image:
   ```bash
   docker-compose build
2. Start the Server and Client containers
   ```bash
   docker-compose up
3. Attach to the client container
   ```bash
   docker attach grpc-client
