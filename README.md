# Chatroom-MultiTech

This repository showcases a chatroom application implemented using multiple technologies. The goal is to demonstrate proficiency with various communication paradigms and cloud technologies, including:
- **Java RMI**
- **gRPC**
- **REST APIs**
- **WebSockets**
- **Distributed Systems**

Each implementation consists of a server and a client component, with consistent functionality across technologies.

---

## Features

### Common Functionality
- Create, list, join, and leave chatrooms.
- Messages are stored persistently in chatrooms.
- Clients can:
  - View all previous messages upon joining a chatroom.
  - Send and receive messages in real-time (max delay: 1.5 seconds).
- Supports at least 10 concurrent clients.
- Dockerized for seamless deployment across machines.

---

## Implementations
### 1. [Chatroom-RMI](./Chatroom-RMI/README.md)
Built using **Java RMI**, demonstrating the use of remote objects and method invocation. Dockerized to ensure compatibility across systems.

### 2. [Chatroom-gRPC](./Chatroom-gRPC/README.md)
Uses **gRPC** for high-performance, strongly-typed remote procedure calls. Includes `.proto` files and examples of unary and bidirectional streaming.

### 3. [Chatroom-REST](./Chatroom-REST/README.md)
Implements RESTful APIs for chatroom operations, with a focus on HTTP communication and JSON data formats.

### 4. [Chatroom-WebSockets](./Chatroom-WebSockets/README.md)
Leverages WebSocket technology for low-latency, full-duplex communication between clients and the server.

### 5. [Distributed Chatroom](./Distributed-Chatroom/README.md)
Showcases a distributed system design, enabling horizontal scaling and fault-tolerant operations.

---
