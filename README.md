# STOMP dotnet core PoC

This proof-of-concept runs a number of containers in order to demonstrate a react-based web client as well as a dotnet core client connecting to a rabbitmq instance which acts a message broker. The clients utilize the the STOMP over web sockets protocol to transmit and recieve messages between one another via the broker.

Containers:

- RabbitMQ with rabbitmq_stomp rabbitmq_web_stomp as well as the the management plugin enabled 
- Web Client - A NextJS+Typescript web-client that uses [@stomp/stompjs](https://github.com/stomp-js/stompjs) to send/recieve messages to/from RabbitMQ using STOMP over websockets
- dotnet-core-client - Implements a STOMP over websockets client that sends and responds to messages. There's a TCP client too! (This is PoC level code, not for prod, but feel free to modify!)
- nginx - Reverse Proxy for the web client and RabbitMQ (websocket, management) ports.

# Getting Started

You'll need some things - you've probably got them but let's make sure:

 - Git - Well, yeah
 - Docker - Containers, baby! If you're on windows, ensure you're in docker linux container mode.
 - VSCode - or your editor of choice
 - NodeJS >=20.9 (Node 24 is used in CI and the web Dockerfiles)
 - Yarn - We're using Yarn workspaces to minimize footprint
    Some recommended extensions:
    - ms-azuretools.vscode-docker
    - eamodio.gitlens
    - dbaeumer.vscode-eslint
    - dotnet related extensions

Pre-reqs can be installed using [chocolatey](https://www.chocolatey.org/) on windows
```
cinst -y nodejs yarn docker-desktop git vscode
```

or [homebrew](https://brew.sh/) on macOS
```
brew update
brew tap caskroom/cask
brew install node git yarn
brew cask install docker visual-studio-code
```

or apt-get on Linux (Debian/Ubuntu)
```
sudo apt-get install -y git nodejs yarn
sudo snap install --classic code docker
```


Once we have the prereqs now run:

```
git clone https://github.com/oceanswave/stomp-dotnet-poc
cd stomp-dotnet-poc
yarn install
yarn dev
```

in the folder to build and bring up the stack.

![Yay, we have a running environment!](https://github.com/Oceanswave/stomp-dotnet-poc/blob/master/images/readme-1.png?raw=true)

# PoC

Give it a couple seconds and then open a browser to localhost:8080. a landing page will load, establish a STOMP-over-websockets connection to RabbitMQ. You'll see 'ping/pong' debug messages in the [console-feed](https://github.com/samdenty/console-feed) powered log.

Every so often you'll see a message with a green background - that's the .Net Core Client publishing out a message.

![dotnet is talking to us, wicked](https://github.com/Oceanswave/stomp-dotnet-poc/blob/master/images/readme-2.png?raw=true)

Enter a message into the textbox and it will send a message to the dotnet client which is subscribing to /topics/hello-from-next-js

We can see the messages being received by the dotnet client by running ```yarn logs dotnet-client```

![and we're talking back!](https://github.com/Oceanswave/stomp-dotnet-poc/blob/master/images/readme-3.png?raw=true)

At this point, you can also run ```yarn logs``` to tail the logs from all the running containers.

The dotnet client is just a console app with a custom STOMP client - all the source is available at ./dotnet-client.

# Debugging/Development

The RabbitMQ Management page is available at http://localhost:8080/rabbitmq to see the current connections/messages in the queue and so forth.
guest/guest is the username and password (RabbitMQ defaults)

![the RabbitMQ management page is function over form here, ladies and gents](https://github.com/Oceanswave/stomp-dotnet-poc/blob/master/images/readme-4.png?raw=true)

Structure:
 - dotnet-client - Contains the dotnet core code to create a simple client that talks STOMP 
 - web-client - Contains the code hosted at localhost:8080
 - rabbitmq - Just contains a dockerfile to set rabbitmq up
 - nginx - just contains a dockerfile for nginx, as well as configuration files for reverse proxying.

The client code needs some love, some locking code, documentation but it's a PoC. There's some scripts in the main package.json to build "production" images but... YAGNI.

Looking at the logs is the best way to see messages flowing, however one can debug the dotnet client as well as the web client through normal mechanisms.

To run the dotnet console app locally, change the hostname to be localhost:
Change line 15 of Program.cs to be ```private const string BrokerHost = "localhost";```


To change to using a straight TCP client, change line 15 and 16 of Program.cs to
```
private const string BrokerHost = "localhost";
private const int BrokerPort = 61613;
```

Then comment out line 23, and then uncomment line 24.

# Troubleshooting

If localhost:8080 isn't functioning, and you're on windows, git for windows has a penchant for changing LF to CRLF - ensure that the .sh files within the /web-client folder haven't been flipped over *sigh*

```
git config --global core.autocrlf false
```

looking at the logs helps too.

# Shuting down

To stop the environment, simply run ```yarn down``` and you can go on your merry way.

## CI validation

Pull requests and pushes to master run a frozen Yarn workspace install,
Next route type generation and TypeScript checking, bounded offline
regressions for both Babel runtime and bundled compiler replacement helpers,
an executable native SWC minification probe, and the full optimized Next
production build on Node 24 using the documented `--webpack` mode.
`BUILD_VALIDATION=1` adds only a compilation audit: post-build tests verify
that actual client JavaScript assets were minimized, compare their SHA256
content digests, and verify the emitted polyfill and rendered HTML reference.
Next's native compiler/minimizer remains unchanged. The separate .NET job
restores/builds with SDK 10; the netcoreapp3.1 EOL warning remains visible.
No application, broker or container is started by CI.

The supported Next 16.4.2 Pages Router retains the existing page, STOMP topics
and logging UI on React 18.3.1. Node 20.9+ and TypeScript 5.1+ are required; the
web Dockerfiles use Node 24. Modern framework browser requirements apply
(Chrome/Edge 111+, Firefox 111+, Safari 16.4+). React 18 support in Next 16 is
deprecated and will be removed in Next 17; this change does not move to
React 19 or the App Router. Console-feed 3.8 declares React 18 support, while
its react-inspector 5 transitive peer still lists React 16/17. That upstream
warning remains visible; no invalid dependency override or warning
suppression is used.

Next 11.1.4 retains the affected Babel runtime 7.15.3 and bundled compiler 7.15.0
for [GHSA-968p-4wvh-cqc8](https://github.com/advisories/GHSA-968p-4wvh-cqc8).
Official Next 16.4.2 bundles patched Babel core 7.26.10 and runtime 7.27.0;
remaining external runtime ranges resolve patched 7.29.10 within their
existing compatible constraints. Both actual helper paths have bounded
regression coverage. No vendor patch, forced override, legacy OpenSSL
provider or skipped minification/build is used.

Next 15.5.28 also contains patched helpers and supports this Pages Router
with React 18, but the [support policy](https://nextjs.org/support-policy)
places its normal two-year maintenance boundary at October 21, 2026.
Next 16 is the Active LTS path selected to avoid another immediate migration.
See the documented [Next 16 upgrade](https://nextjs.org/docs/pages/guides/upgrading/version-16)
and [route type generation](https://nextjs.org/docs/pages/api-reference/cli/next#next-typegen-options).

Offline browser validation uses intercepted production assets and a mocked
STOMP WebSocket; it does not establish real broker/container interoperability
or replace human device/visual acceptance.
