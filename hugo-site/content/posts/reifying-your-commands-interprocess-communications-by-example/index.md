---
title: "Reifying Your Commands – Interprocess Communications by Example"
date: 2016-10-17T16:35:19Z
slug: reifying-your-commands-interprocess-communications-by-example
aliases: ["/reifying-your-commands-interprocess-communications-by-example/"]
categories:
  - "Technique"
tags:
  - "actionscript"
  - "c#"
wp_post_id: 915
---

In the first part of this series, I introduced readers to the work reify, which means to make something real. So far, we have seen how the ActionScript application converts a logging request into a command object, serialises it into a byte array, and sends it over a TCP socket connection into the waiting arms of a server. The server for its part must deserialise the byte array back into a command object and execute it.

This last part in the series explains how this is done.

The Story So Far

We have seen how the Puppeteer receives a message. In the receive callback method is a call to deserialise the message into an object.

```csharp
byte[] message = new byte[messageLength];
Buffer.BlockCopy(buffer, 4, message, 0, messageLength);

ICommand command = Util.Deserialize(message);
```

The Deserialize utility method receives only the portion of the message that constitutes the actual data. The first 32 bits are discarded as they are not relevant to the deserialisation process. The Deserialize method is extremely simple.

```csharp
public static ICommand Deserialize(byte[] message)
{
    Dictionary<byte, type=""> instructionClassMap = new Dictionary<byte, type="">() { { 0x02, typeof(Trace) } };

    Type commandType = null;
    ICommand command = null;
    if (instructionClassMap.TryGetValue(message[0], out commandType))
    {
        command = (ICommand)Activator.CreateInstance(commandType, new object[] { message });
    }

    return command;
}
```

It reads the first byte from the message which contains the instruction to be executed. The instruction is then used as key to look for the type in the instruction map. The Activator.CreateInstance() API is used to instantiate the type into a variable. The instance is then returned from the function.

The receive callback then dispatches a CommandReceived event. The application implements the plumbing from that point onward to handle the event notification and act upon it.

At this point, we need to take a step back and observe the command object instantiation in detail. Each command type has its own implementation detail which interprets and utilises the message. The Trace class, for example, reads the level, category and message values from the message. Its constructor is listed below.

```csharp
public Trace(byte[] message)
{
    int unixTimeStamp = message[1] << 24 | message[2] << 16 | message[3] << 8 | message[4];
    TimeStamp = Util.UnixTimeStampToDateTime((double)unixTimeStamp);
    int paramCount = message[5] << 24 | message[6] << 16 | message[7] << 8 | message[8];
    parameters = new string[paramCount];
    int index = 9;
    
    for (int i = 0; i << paramCount; i++)
    {
        int length = message[index] << 8 | message[index + 1];
        parameters[i] = Encoding.UTF8.GetString(message, index + 2, length);

        index += (2 + length);
    }

    Level = parameters[0];
    Category = parameters[1];
    Parameters = parameters;
}
```

The first byte contains the instruction. This is ignored since we already know that the instruction is Trace (0x02).

The next four bytes contain the timestamp of the message as a 32-bit integer. The value is converted into DateTime object through a utility method.

The next four bytes contain the number of parameters that are passed into the Trace command. The command uses this number to determine the number of string objects to retrieve from the message. Remember that each string object is prefixed by a 16-bit integer that contains the number of characters that make up the string. That's where the index + 2 comes from, which offsets the current position in the array by another 2 bytes. Once the parameters are loaded into an array, they are assigned to public accessors of the Trace class.

The application uses the public members to display the Trace command on screen and store them into a database for persistence.
