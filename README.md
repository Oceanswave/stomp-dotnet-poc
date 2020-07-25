# STOMP dotnet core PoC

This proof-of-concept runs a number of containers to demonstrate dotnet core connecting to a rabbitmq instance
that has the STOMP plugin enabled.

Containers:

- RabbitMQ with rabbitmq_stomp rabbitmq_web_stomp as well as the the management plugin enabled 
- Web Client - A NextJS+Typescript web-client that uses [@stomp/stompjs](https://github.com/stomp-js/stompjs) to send/recieve messages to/from RabbitMQ using STOMP over websockets
- dotnet-core-client - Implements a STOMP over websockets client that sends and responds to messages.
- nginx - Reverse Proxy for the web client and RabbitMQ (websocket, management) ports.

# Getting Started

You'll need some things - you've probably got them but let's make sure:

 - Git - Well, yeah
 - Docker - Containers, baby! If you're on windows, ensure you're in docker linux container mode.
 - VSCode - or your editor of choice
 - NodeJS > v12 - Ubiquitous
 - Yarn - We're using Yarn workspaces to minimize footprint
    Some recommended extensions:
    - ms-azuretools.vscode-docker
    - eamodio.gitlens
    - dbaeumer.vscode-eslint
    - dotnet related extensions


Clone this project and run ```yarn dev``` in the folder to build and bring up the stack.

At this point, you can also run ```yarn logs``` to tail the docker logs.

# PoC

Open a browser to localhost:8080. the page will load, establish a STOMP connection to RabbitMQ. You'll see 'ping/pong' messages in the console-feed powered log.

Enter a message into the textbox and it will send a message to all subscribers of the 'all' topic.
In this way you can use pub/sub with clients subscribed to topics and queues.

But I digress, what we're interested in (I think) is listening and sending STOMP messages from dotnet.

aaaand... here.

