import React, { useState, useEffect } from 'react';
import dynamic from 'next/dynamic'
import { Client, Message, StompSubscription } from '@stomp/stompjs';

function useInput({ type, initialState }) {
  const [value, setValue] = useState(initialState || '');
  const input = <input value={value} onChange={e => setValue(e.target.value)} type={type} />;
  return [value, input];
}

const Logs = dynamic(
  () => import('../components/LogsContainer'),
  { ssr: false }
)

export default function Home() {
  const [message, messageInput] = useInput({ type: "text", initialState: "Test Message" });
  let client: Client;

  useEffect(() => {
    client = new Client({
      brokerURL: "ws://localhost:8080/stomp",
      connectHeaders: {
        login: "guest",
        passcode: "guest"
      },
      debug: function (str) {
        console.log(str);
      },
      reconnectDelay: 5000,
      heartbeatIncoming: 4000,
      heartbeatOutgoing: 4000
    });
    
    let subscription: StompSubscription;
    client.onConnect = (frame) => {
      console.log('%c connected!!', 'background: green; color: white; display: block;');
      subscription = client.subscribe("/topic/hello-from-dot-net", (message) => {
        console.log(`%c Dotnet says: ${message.body}`, 'background: green; color: white; display: block;');
      });
    };
    
    client.onStompError = (frame) => {
      console.log('Broker reported error: ' + frame.headers['message']);
      console.log('Additional details: ' + frame.body);
    };
    
    client.activate();

    return () => {
      subscription?.unsubscribe();
      client?.deactivate();
    }
  });

  const sendMessage = (body) => {
    client?.publish({ destination: '/topic/hello-from-next-js', body: body });
  }

  return (
    <>
      <div>
        Send a message with STOMP: <br />
        {messageInput} <button onClick={() => sendMessage(message)}>Send</button> <br />
      </div>
      <div style={{marginTop: '15px', backgroundColor: '#242424', maxHeight: '500px', maxWidth: '600px', display: 'flex', flexDirection: 'column-reverse', overflow: 'scroll'}}>
        <Logs />
      </div>
    </>
  )
}
