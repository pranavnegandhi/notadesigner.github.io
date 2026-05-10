---
title: "Socket Talk – Interprocess Communications by Example"
date: 2016-10-03T13:28:22Z
slug: socket-talk-interprocess-communications-by-example
aliases: ["/socket-talk-interprocess-communications-by-example/"]
categories:
  - "Technique"
tags:
  - "actionscript"
  - "c#"
wp_post_id: 908
---

In [the previous article in this series](/a-message-in-a-socket-interprocess-communications-by-example/), I explained the logging framework that the Flex SDK provides, limitations of the Flash Player at providing effective run-time logging, and how they can be circumvented by using the Socket API. I also described a custom message data structure that encodes the information to be sent out of the Flash Player. This part of the article explores the implementation of the socket server that receives the message, and how it decodes it into meaningful instructions and information.

### Introducing...Puppeteer!

Puppeteer is a desktop application that runs on Windows which can be used to manipulate one or more client applications. The application works by opening up a TCP port for client applications to connect to, then exchanging messages over the connection to send or receive instructions back and forth. Messages are binary-encoded and follow a simple format which was described in the previous article. Instructions are 8-bit integers, and therefore, have an upper limit of 255 possible values. The message may contain an additional payload along with the instruction. There is no limit to the length of the message.

Client applications require to integrate the ability to dispatch, receive and parse messages to Puppeteer. It cannot connect to any application on its own without the application explicitly requesting the connection, and having the ability to dispatch or react to messages from the server. Client applications can be written on any platform that supports TCP sockets.

On startup, Puppeteer begins listening for incoming connections from client applications on a well-known port number. It also accommodates certain idiosyncrasies specific to the Flash Player. Applications written in other languages can ignore these aspects, as they are most likely not relevant to their own ability to function correctly.

### "Hello!"

One of the restrictions applied on the Flash Player is that every TCP socket connection must be explicitly authorised by the owner of that server. To quote Peleus Uhley from the Adobe website -

> One of these features is the ability to create TCP sockets in order to exchange data with servers. From a network administrator's point of view, the idea that content from the Internet could make socket connections to internal hosts is scary. This is why Flash Player requires permission from the target host before it will allow content to make the network connection.

The policy file is requested automatically by the Flash Player the first time the content playing in makes the connect() API call. The process followed is described below.

1. The content (.swf file playing the the Flash Player) requests a connection to the server through the Socket API.
2. The Flash Player checks its whitelist if the server has already allowed access to the content.
3. If it does not already have a policy file for the server in memory, it pauses the connection request from the content and instead tries to connect on port number 843. If a connection is established on this port number, it sends a message containing the string "".
4. If the server does not respond on port number 843 unti timeout, the Flash Player attempts to connect again on the same port number that is requested by the content.
5. The server must respond by sending back the contents of the policy file as soon as the connection is accepted. Any other response from the server will be considered invalid and the Flash Player will disconnect from the server.
6. Once the policy file is served, the Flash Player parses it for correctness and checks if it authorises the content to connect to the server. If the policy file allows the connection, the content is notified about it and is now able to communicate freely with the server.

So the first requirement for Puppeteer is to be able to listen for a client request for a socket policy file and serve it up. The application does not listen for incoming policy file requests on port 843. Instead, it responds with the policy file when the client attempts to connect on the public incoming request port number (1337 by default). The response is an XML document encoded as a null-terminated string. The null at the end is necessary. The XML response will be treated invalid without it (many hours were wasted in learning this seemingly insignificant detail).

### A Mirrored Standard for Commands

Puppeteer works in tandem with the command pattern implemented by its corresponding client library. Each message is serialised into a byte array, which contains the instruction number of the command being invoked, and the payload that accompanies the instruction. It must, therefore, be able to parse and interpret the message, which is done by mirroring the ICommand interface and its implementation classes in .NET.

```csharp
public interface ICommand
{
    event ExecutionCompletedHandler ExecutionCompleted;

    DateTime TimeStamp
    {
        get;
    }

    void Execute();
}
```

The ICommand interface declares the Execute() method, same as the one contained within the ActionScript code. It also declares a public property called TimeStamp and an ExecutionComplete event. A CommandBase class implements this interface and provides the basic common set of methods which are required to fulfill this contract.

```csharp
public abstract class CommandBase : ICommand
{
    ...
}
```

Finally, separate classes are written for each command that Puppeteer must be able to interpret and understand.

```csharp
public class Trace : CommandBase
{
    ...
}
```

### Insert Plug Here

The previous article talked about sockets very briefly. So let me explain that topic before proceeding.

Sockets are a software construct that operating systems use to communicate over the network. They are handles to resources over a network interface, same as files are handles to resources on the hard disk. They come in various different types, of which datagram and stream sockets are most commonly heard of. They are often referred to by the protocol they typically use - UDP for datagram sockets, and TCP for stream sockets. Puppeteer is built on stream sockets. The .NET framework ships with a managed implementation of the socket API in the System.Net.Sockets namespace. The Socket class is the primary actor in this namespace.

On startup, the application begins with instantiating the Socket class, binding it to a local end point and setting it to listen for incoming connection requests.

```csharp
listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
endPoint = new IPEndPoint(0x0100007F, 1337);
listener.Bind(endPoint);
listener.Listen(0);
listener.BeginAccept(BUFFER_SIZE, new AsyncCallback(AcceptConnection), listener);
```

In this example, a stream socket is initialised to use the IPv4 addressing scheme and TCP protocol. In order to begin listening, the socket must be bound to an end point, which is a combination of an IP address and a port number. Every TCP connection can be uniquely identified by its two end points - one on the host, and the other on the client. The IPEndPoint class represents an end point on one end of the connection. The IP address of the end point is represented as a hexadecimal-encoded, big-endian 64-bit integer.

Note: The IP address of the computer is hard-coded to 127.0.0.1 in this example. There are more sophisticated techniques to identify the IP address of the computer that the application runs on at run-time which are excluded here for brevity.

Listen is a non-blocking API call. The thread continues to run while the socket is listening for incoming connections. For single-threaded applications, the programmer must put the application into a loop while calling BeginAccept() periodically, in order to prevent it from exiting. Since Puppeteer is a Windows Forms application, this is not necessary.

The buffer size parameter of the BeginAccept API specifies the number of bytes that the server has to read from the message sent by the client. The AsyncCallback is a reference to a callback that is fired when an incoming connection request is received. The last parameter is a state object which can be used to pass around the state of the connection. In this case, the Socket instance itself is used as the state object.

The callback contains code to handover the connection from the listener to another Socket instance dedicated to communicating with the client. This is required so that the listener can be freed to continue to listen for incoming requests on from other clients. The EndAccept() API automatically creates a new Socket instance to communicate with the client.

```csharp
private void AcceptConnection(IAsyncResult result)
{
    Socket listener = (Socket)result.AsyncState;
    Socket clientSocket = listener.EndAccept(result);

    byte[] response = Encoding.ASCII.GetBytes(SocketPolicy);
    clientSocket.BeginSend(response, 0, response.Length, SocketFlags.None, new AsyncCallback(SendData), clientSocket);
    listener.BeginAccept(new AsyncCallback(AcceptConnection), listener);
}
```

Once the connection has been established on the new socket, the server must first publish the string containing the socket policy file required by the Flash Player. It does this with the BeginSend method on the new socket. Finally, the original socket instance which is bound to a well-known port number is set back to accept incoming connection requests.

The .NET framework triggers an AsyncCallback when the data has been sent over the new socket instance. This callback signals the socket to end the sending operation, clears the incoming buffer and sets the socket to begin receiving data when the client sends it.

```csharp
private void SendData(IAsyncResult result)
{
    Socket clientSocket = (Socket)result.AsyncState;
    clientSocket.EndSend(result);
    Array.Clear(buffer, 0, BUFFER_SIZE);
    clientSocket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
}
```

The last stage in this cycle is to receive and process the data, and either respond to the data if required, or set the client socket back into the receiving state as shown here. This is where the meat of the operation occurs. The basic structure of the ReceiveData() method is shown below.

```csharp
private void ReceiveData(IAsyncResult result)
{
    Socket clientSocket = (Socket)result.AsyncState;

    if (clientSocket.Connected)
    {
        Array.Clear(buffer, 0, BUFFER_SIZE);
        clientSocket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
    }
}
```

Some basic processing is also performed in this method. The application is aware of the message structure that was described in the previous article in this series. The ReceiveData method receives the entire message, but only processes the first four bytes which contain the total length of the message. The rest of the bytes from the message are read from the buffer into a byte array and passed on to a deserialisation utility class, which converts it into an ICommand instance.

```csharp
private void ReceiveData(IAsyncResult result)
{
    Socket clientSocket = (Socket)result.AsyncState;

    // Extract the length of the message from the first 4 bytes
    int messageLength = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];

    // Extract the bytes containing the message and deserialize it into an ICommand object
    byte[] message = new byte[messageLength];
    Buffer.BlockCopy(buffer, 4, message, 0, messageLength);

    ICommand command = Util.Deserialize(message);

    if (clientSocket.Connected)
    {
        Array.Clear(buffer, 0, BUFFER_SIZE);
        clientSocket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
    }
}
```

Finally, the method triggers a CommandReceived event which other classes in the application subscribe to in order to act upon the command.

```csharp
private void ReceiveData(IAsyncResult result)
{
    Socket clientSocket = (Socket)result.AsyncState;

    // Extract the length of the message from the first 4 bytes
    int messageLength = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];

    // Extract the bytes containing the message and deserialize it into an ICommand object
    byte[] message = new byte[messageLength];
    Buffer.BlockCopy(buffer, 4, message, 0, messageLength);

    ICommand command = Util.Deserialize(message);

    if (null == CommandReceived)
    {
        return;
    }

    CommandReceivedEventArgs e = new CommandReceivedEventArgs(command);
    CommandReceived(this, e);

    if (clientSocket.Connected)
    {
        Array.Clear(buffer, 0, BUFFER_SIZE);
        clientSocket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
    }
}
```

In the next part in this series, I will explain how the command is deserialised and executed by the Puppeteer.
