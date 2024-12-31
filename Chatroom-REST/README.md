# Chatroom-REST

This folder contains the implementation of the chatroom application using **REST APIs**.

## Features
- Implements RESTful services for managing chatrooms and messages.
- HTTP-based communication using JSON data formats.
- Fully Dockerized for deployment on multiple machines.

### Key Concepts
- **RESTful Architecture**: Uses HTTP methods (GET, POST, DELETE) for chatroom operations.
- **JSON Serialization**: Message data is serialized into JSON format.

## How to Run
1. Build the Docker image:
   ```bash
   docker-compose build
2. Start the Server and Client containers
   ```bash
   docker-compose up
3. Attach to one or more client container
   ```bash
   docker attach cs-client
   docker attach cs-client-2
