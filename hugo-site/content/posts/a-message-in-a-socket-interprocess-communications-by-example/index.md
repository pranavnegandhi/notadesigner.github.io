---
title: "A Message in a Socket – Interprocess Communications by Example"
date: 2016-09-26T16:41:00Z
slug: a-message-in-a-socket-interprocess-communications-by-example
aliases: ["/a-message-in-a-socket-interprocess-communications-by-example/"]
categories:
  - "Technique"
tags:
  - "actionscript"
  - "c#"
wp_post_id: 884
---

### Introduction

I couldn't help but feel like a luddite as I sat down to write down this article. Who works on Flash applications in 2016 any more? Aren't we all done with it now?

The Flash Player might have its own set of problems, which is a topic for another discussion. The ActionScript language, however, is fun to work with. It is syntactically similar to Java, has a class-based inheritance model, a choice between static or dynamic typing, or both, within the same code base, namespaces, and a host of easy to use and powerful APIs for 2D and 3D graphics, network operations, animation, audio processing, XML and text processing, and more. Frameworks like the open source Flex SDK make it possible to do even more interesting things on top of this.

But, more importantly, we are heavily invested into the Flash ecosystem for the VMX platform at Vertika. We have 9 years of effort and 40,000 lines of code written in ActionScript and MXML, which would be very expensive to port over to a new platform without a very compelling business reason. So here we are today.

Unfortunately, the Flash Platform also comes with its idiosyncrasies, one of which is addressed in this article.

For those losing interest at the mention of Flash, the good thing about this client architecture is that it can easily be ported over to literally any other platform with little change and still work. And the server-side stuff is written in C#. Collectively, this solution covers a plethora of different concepts such as socket programming, data structure design, a custom-built lightweight ORM framework and design patterns.

### Raison D'être

The Great Catsby is a rather upset feline. He is fond of company and loves nothing more than to kick back with a saucerful of heavy cream milk and a stack of Calvin & Hobbes comics with his human pals. But they just won't let him out of his cage. Catsby, you see, is a 300 kilogram Bengal Tiger at the zoo. And no matter how much he pleads, they are not going to let him out and explore the city.

It's a rather bleak state of affairs for the poor cat.

The guys at Adobe (then Macromedia) had the same ideas when they designed the Flash Player, which was designed primarily as a browser plug-in to play specially authored content served off websites. And as every competent security-minded person knows, it can be a very bad idea to allow strange websites to gain unrestricted access to your computer. Even if every ActionScript programmer pinky-swore not abuse their users' trust, the guys at Macromedia were not going allow them free rein over the computers running the Flash Player. Hence, Flash content runs in a sandbox with several restrictions, one of them being no direct access the file system.

This is a tough situation for many programmers who need any kind of runtime application logging. No file system access means there is no persistent data storage on the client.

It is not that there's no logging at all. The Flash Player Debugger exposes a very limited, plain-text log file access that is disabled by default, and requires a configuration flag in a specific plain-text file in order to be activated. The state of this flag cannot be queried or controlled from within the Flash Player. Extracting the file is a task and a half because it's buried deep within the file system hierarchy. The log file is shared among all Flash Player applications which are executed on that computer. While there is no possibility of data conflicts, filtering and extraction of the data that is relevant to your application becomes a challenge. Finally, since this is a plain-text file, there is no structured information storage facility available here other than what can be achieved with plain text files (such as comma-separated values).

Did I mention that even this facility is only available on a special Debugger edition of the Flash Player that most people don’t use?

In effect, it's impossible to have any kind of reliable, persistent runtime logging on a production computer. A third-party intervention is essential. And this is the challenge that is addressed in this article – a logging framework that allows Flash content to delegate the task of persistent storage to another process, which runs at a higher privilege level and has access to the file system. The persistence process can be running locally on the same computer, or on another computer. The mechanism works identically as long as the two computers are able to establish a connection with each other over standard sockets.

### Hi! It's me, your brother!

Parents are like the operating system of the family. They spawn child processes, manage resources and schedule time for tasks. A well-organised family is like Unix. Every child gets their own space, and nobody gets into each other's way. When they have to communicate, they send messages to pre-designated points. These could be a cell phone, a sticky note on the fridge or a whiteboard in the hallway. They could also speak across the dinner table, but nobody does that in 2016. So that's out.

Speaking over the cell phone is more versatile because it works whether the person is at home or away at work, out running an errand, or at a party. A message on a sticky note is only delivered when the person returns back home and checks the fridge.

Operating systems also offer similar means to communicate between processes. You could send a message to the recipient over a socket, or write to a file that the recipient checks periodically. Since we have already discussed earlier that writing to files is out, that leaves us with socket communications.

Fortunately, the Flash Player has an API to communicate over TCP sockets. It works identically when communicating with a different device or another process on the same computer where the Flash application is being run. The Flash Player Socket API can only make client-side calls. It cannot listen for connections from another process. Therefore, the external application must play the role of a server and begin listening for connection requests from the Flash Player on a known port number. Once the connection is established, the two processes can send messages back and forth.

This mechanism can be utilised to create an external logging tool that listens for messages from a Flash Player client, and logs them to a persistent storage such as hard disk. Since the socket API works locally as well as remotely, the logging server can be situated at an offsite location, and as long as the two computers can communicate with each other, log messages will be successfully delivered and logged.

### Flex Logging API

A lot of work that is needed to establish a robust logging API has already been implemented as part of the Flex SDK in form of the Logging API, which is loosely modelled on its Java framework equivalent. This API provides a convenient way to create structured log messages with severity levels and named categories. A log message is triggered by simple API calls, and is printed to a destination by a target object. Targets can be programmed to listen for messages at a certain severity level (and above), as well as certain categories only. For example, low-latency logging such as an in-memory temporary store can be used for diagnostic messages which are triggered more often, while infrequent but critical errors can be dispatched to remote persistent storage.

The SDK ships with two in-built logging targets – LineFormattedTarget and TraceTarget – which provide basic logging facilities to the application. TraceTarget prints the message to the Flash Builder or fdb console and extends LineFormattedTarget.

The Flex Logging API is documented elsewhere in sufficient detail. However, I'll cover it briefly in this article to provide context to the reader for how it is used.

The API is centred around four types.

1. mx.logging.Log
2. mx.logging.ILogger
3. mx.logging.LogEvent
4. mx.logging.ILoggingTarget

The SDK ships with a mx.logging.AbstractTarget class that implements the ILoggingTarget interface. Developers can extend this class to build their own custom logging target implementations.

#### How the Flex Logging API Works

The ILogger interface provides methods to send messages to one or more targets. The developer doesn't create the logger instance directly. Rather, they call upon the getLogger static method of the Log class to retrieve an ILogger implementation instance. The getLogger method takes a string parameter called category, which can be used to filter log messages down to a particular sub-system within an application.

The category is conventionally set to the fully-qualified name of the class that calls the ILogger API.

The ILogger interface also exposes methods to perform log operations at five different severity levels, and a generic log method that takes the severity level as a parameter.

The severity levels in increasing order are DEBUG, INFO, WARN, ERROR and FATAL.

```actionscript
var logger:ILogger = Log.getLogger("com.notadesigner.ExpletiveGenerator");
logger.info("Sonofagun!");
logger.log(LogEventLevel.FATAL, "Crumbs!");
```

A logging target receives the log message and prints it to the destination medium. The logging target has a property called level, which determines the severity of messages that it will receive.

```actionscript
var target:ILoggingTarget = new TraceTarget();
target.level = LogEventLevel.INFO;
```

The target receives all messages of its assigned severity level and below. For example, the target instance in the example above would be able to receive DEBUG and INFO messages.

A logging target instance also has a filters property of type array that contains the categories which this instance should listen for.

```actionscript
target.filters = [ "com.notadesigner.ExpletiveGenerator" ]; // Category name must match
```

This snippet enables the target to receive all log messages whose category is set to "com.notadesigner.Example". The filters array can contain more than one category.

```actionscript
target.filters.push("com.notadesigner.GreetingsGenerator");
```

The target is actually of greatest interest in this article, because it is this class that performs the message printing. In order to send the message to an external process, we need to build a custom logging target that is able to dispatch messages over a socket.

#### com.notadesigner.sockPuppet.SocketTarget

The SocketTarget class extends AbstractTarget rather than LineFormattedTarget, because the latter merges all the message fields into a single string, which we don't want. By keeping them as separate fields of their native types, we get maximum control over the structure of the message, as well as the least amount of memory usage.

We begin by instantiating the class.

```actionscript
var target:SocketTarget = new SocketTarget("localhost", 1337);
```

The constructor takes two parameters for the host name and the port number on which the socket server is already ready and running. We will gloss over the details of performing the connection and maintaining it.

When a message is received, the logging framework triggers the logEvent method of the SocketTarget instance. This is defined as an empty method in the AbstractTarget class that takes a parameter of type LogEvent. By overriding this method, the SocketTarget instance can then dispatch the message to the server.

### Making It Real

Before Daryl got into the programming business, he used to wait at the local deli. His job was to take orders from diners and pass them on to the kitchen, and serve the completed order after it was prepared by the kitchen staff. Daryl’s task was simple. He would jot down a list of orders from the table and pass it on to the chef. The chef would then prepare whatever items came up in the queue, and ring up the waiting staff once it was ready. The staff picked the completed order and served it fresh and hot to their patrons.

You want a burger and fries? Sure! Footlong with everything? On its way, sir. Need coffee with extra cream? Here it comes.

The food was good and footfalls were high. The entire team loved their job and did their best to stick to the process because it was so efficient. It abstracted away the underlying complexity of preparing a meal with all its customisations into a simple, mechanical task, while still retaining all necessary details that the kitchen staff would need to build an order.

In the years that followed, Daryl was on his way to become an accomplished software developer of some fame. It was during this period that he learned about design patterns in software engineering and came across the command pattern. In his mind, he could relate immediately.

> A command is a *reified method call*. – Bob Nystrom, Game Programming Patterns

I find Bob‘s explanation to be much more relatable than the longer definition defined in the GOF book. There’s a beauty in its terseness. To understand it, of course you need to understand what reified means. To borrow another quote from Bob‘s book

> “Reify” comes from the Latin “res”, for “thing”, with the English suffix “–fy”. So it basically means “thingify”, which, honestly, would be a more fun word to use.

So there we are. The command pattern converts an abstract item like a deli order into a real, fresh, piping hot meal. I‘m sure even reading about this makes you hungry. Maybe you would like to repeat the experience right about now, and issue a command to your local delivery joint for a large pizza with Coke. Go ahead. I‘ll wait.

Are you back? Let‘s continue.

Returning to the task at hand, we are now at a stage in the logging operation where the message is to be dispatched to the socket server process. The Flex framework already provides all the necessary structured information in an event handler – the logEvent method – that is overridden by the SocketTarget class. It is a matter of sending this information to the external process, so that it can be logged to disk. Reify the log, if you please. We need some infrastructure in order to implement this mechanism.

#### A Standard for Commands

The invention of the assembly line provided a massive boost to human progress during the industrial revolution. It was a break from the previous practice of having few master-craftsmen assembling a product from start to finish. By delegating each part of the assembly to a single individual, the human skill required was greatly diminished and productivity soared. Rather than having one exceptionally good worker build a single sword in a day, factories were able to assemble dozens by assigning several average-skilled workers, each handling only one aspect of the assembly process. All workers just followed one instruction – assemble the product. But what each did in response to this instruction was different. Somewhat akin to what we do with the ICommand interface.

This interface unifies all commands so that they can be guaranteed to have a single method for execution. The ActionScript implementation looks like this.

```actionscript
public interface ICommand
{
    function execute():ByteArray;
}
```

The obvious follow up is why have a command interface at all when the only operation we need is to trace messages. However, it’s a small leap from where we are now (sending plain-text messages for printing) into communicating more elaborate commands such as enabling and disabling logging, force-flushing the log to disk, clearing the log or sending other structured diagnostic information such as internal data structures. It becomes easier to swap out one command for another, much like the assembly line, if they all must adhere to a common interface. All these commands can be triggered from the Flash application by instantiating an appropriate command class instance, serialising it, and dispatching the result over the connection.

The details of the execution of the command are specific to the command. The common operation is serialisation of the message from a Flash object to something that the server can read. For this, we use a byte-array because it is compact and efficient to send over a socket.

Trace generates a certain kind of layout in the byte array, which contains the string to be traced in the message log. Another possible message might be to toggle the state of the logging operation itself. In this case, the byte array could only require a Boolean value to indicate the toggle state. A more complex diagnostic message might send across complete object instances after serialisation, along with information on how they are to be deserialised again.

While message specifics are different, they all share a common structure called the message header. The header in the current version of this API requires the following elements.

1. Message length (32-bit integer) – The total length of the message in bytes (including the header and all parameters).
2. Instruction (byte) – An 8-bit integer value that contains the instruction code. Each command has a unique instruction number associated with it. This makes the message more compact than sending the human-readable command name as a string.
3. Message timestamp in UTC (32-bit integer) – The date and time that the message was created in the Flash Player, encoded as seconds since the UNIX epoch. The time is calculated relative to UTC and must be converted into local time as an additional step by the receiver of the message, if needed.
4. Number of parameters (32-bit integer) – A count of the number of parameters attached to the message.

The instruction for the Trace command is assigned the value 0├ù02. The byte array needed to trace the string “Hello” at severity level INFO is shown in the table below. Each cell represents 1 byte.

| Message Length (4 bytes) |
| --- |
| 0×00 | 0×00 | 0×00 | 0×20 |  |
| Instruction (1 byte) |
| 0×02 |  |
| Timestamp (4 bytes) |
| 0×57 | 0×CD | 0×AC | 0×31 |  |
| Parameter Count (4 bytes) |
| 0×00 | 0×00 | 0×00 | 0×03 |  |
| String Length (2 bytes) |
| 0×00 | 0×04 |  |
| Severity Level ("INFO", variable-length string) |
| 0×49 | 0×4e | 0×46 | 0×4f |  |
| String Length (2 bytes) |
| 0×00 | 0×04 |  |
| Category ("Main", variable-length string) |
| 0×4d | 0×61 | 0×69 | 0×6e |  |
| String Length (2 bytes) |
| 0×00 | 0×05 |  |
| Message ("Hello", variable-length string) |
| 0×48 | 0×65 | 0×6c | 0×6c | 0×6f |

The first 4 bytes contain the message length. The next 1 byte contains the instruction code (0├ù02). Four bytes are allocated to sending the timestamp in seconds (1,473,096,753 seconds since 1 January 1970). The next 4 bytes contain the number of parameters attached to this message (3). The contents of the rest of the message are specific to the Trace command. It is made up of 3 data fragments that encode a string prefixed by its length as a 16-bit integer. A string is an array of characters. C-style strings mark the end of the array by placing a null character in the last position of the array. When a piece of code needs to perform any string operation, it walks the length of the array until it arrives at the null terminating character. Alternative to this are length-prefixed strings, or Pascal strings, which prefix the length of the character array at the beginning. Pascal strings do not require a null terminating character as the length of the string is already known beforehand.

This kind of data structure has an advantage of speed over null-terminated strings. Any operation that requires the length of the string can peek into the beginning of the sequence, making it a constant time operation. Finding the length of null terminated strings, on the other hand, is a linear operation because it requires walking the entire byte array to locate the null terminator. Longer strings take more time, shorter strings take less.

The writeUTF method of the ActionScript ByteArray class uses Pascal strings. Programmers do not have a choice in this matter. Therefore, all commands, present as well as new ones in the future, use this same structure to encode strings.

### A Pregnant Pause

Once the message has been serialised into a byte array, the send method of the Socket class is used to dispatch the message over the network.

At this point, the message is out of the realm of the Flash Player and is crossing boundaries into the .NET runtime. If the server at the other end receives the message in full (which is more or less guaranteed due to the TCP stack), it can be parsed, deserialised and acted upon by the server. If the message fails for any reason, the Socket class raises an error event within the Flash Player for the programmer to handle. For brevity, we ignore that aspect and proceed with the assumption that the message has been received successfully and in full. Transparent to the ActionScript programmer, the server dispatches an ACK response to acknowledge receipt of the message. There is nothing more that the Flash Player has to after this point and it can return back to its steady state.

Keep watching this space for part two of this article that explores receiving the message on the server, interpreting it into its component parts, then acting upon the instruction contained in it.
