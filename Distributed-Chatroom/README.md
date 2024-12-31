# Distributed Chatroom Application

This folder contains the implementation of a distributed chatroom system using gRPC.

## Features
- Horizontally scalable design with multiple servers(3 in the implementation but can be increased).
- Fault-tolerant operations with distributed message storage.
- Fully Dockerized for deployment across machines.

### Key Concepts
- **Distributed Systems**: Messages and chatrooms are distributed across multiple servers following **coordinator approach**.
- **Load Balancing**: Efficient distribution of clients among servers.
- **Coordinator approach**: There is one coordinator Server and rest of the servers send all kinds of write requests to coordinator server which then propogates the message to rest of the servers in the system.

## How to Run
1. Build the Docker images:
   ```bash
   docker-compose build
2. Start the Server and Client containers
   ```bash
   docker-compose up
3. Above command creates 3 Servers and 3 clients all are connected to different servers
4. Attach to one or more client container
   ```bash
   docker attach grpc-client-one
   docker attach grpc-client-two
   docker attach grpc-client-three
5. Once attached sending message through any of the client will propogate message to all the other clients even though they are connected to different servers, thus forming a distributed system.
