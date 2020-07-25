# STOMP dotnet core PoC

This proof-of-concept runs a number of containers in order to demonstrate a web client and dotnet core connecting to a rabbitmq instance using the STOM over web sockets protocol and sending messages to one another.

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


Clone this project and run

```
git clone https://github.com/oceanswave/stomp-dotnet-poc
yarn install
yarn dev
```

in the folder to build and bring up the stack.

At this point, you can also run ```yarn logs``` to tail the docker logs.

# PoC

Give it a couple seconds and then open a browser to localhost:8080. a landing page will load, establish a STOMP-over-websockets connection to RabbitMQ. You'll see 'ping/pong' debug messages in the [console-feed](https://github.com/samdenty/console-feed) powered log.

Every 2 seconds you'll see a message from the dotnet client in green

Enter a message into the textbox and it will send a message to the dotnet client which is subscribing to /topics/hello-from-next-js

This shows that the RabbitMQ server is operational with STOMP

Every so often you'll see a message with a green background - that's the .Net Core Client publishing out a message.

The dotnet client is just a console app - all the source is available at ./dotnet-client.

# Debugging/Development

The RabbitMQ Management page is available at http://localhost:8080/rabbitmq to see the current connections/messages in the queue and so forth.
guest/guest is the username and password (RabbitMQ defaults)


Structure:
 - dotnet-client - Contains the dotnet core code to create a simple client that talks STOMP 
 - web-client - Contains the code hosted at localhost:8080
 - rabbitmq - Just contains a dockerfile to set rabbitmq up
 - nginx - just contains a dockerfile for nginx, as well as configuration files for reverse proxying.

# Troubleshooting

If localhost:8080 isn't functioning, and you're on windows, git for windows has a penchant for changing LF to CRLF - ensure that the .sh files within the /web-client folder haven't been flipped over *sigh*

# Shuting down

To stop the environment, simply run ```yarn down``` and you can go on your merry way.
