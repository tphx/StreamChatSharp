StreamChatSharp
===============

##About

StreamChatSharp is a C# library designed specifically for use with the [Twitch.tv IRC interface](https://github.com/justintv/Twitch-API/blob/master/IRC.md). Although any IRC server can be connected to, functionality is not guaranteed. This is because Twitch IRC works slightly differently to standard IRC servers.

This library was designed with the hope that it would make the creation of bots and other applications that use the [Twitch.tv IRC interface](https://github.com/justintv/Twitch-API/blob/master/IRC.md) much easier.

----------

##Requirements

 * [.NET Framework 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653)

----------

##How to use

Checkout this [simple chat bot](https://github.com/tphx/StreamChatSharp/blob/master/Examples/SimpleChatBot/SimpleChatBot/ChatBot.cs) to see an example of how to use the [ChatClient](https://github.com/tphx/StreamChatSharp/blob/master/StreamChatSharp/StreamChatSharp/ChatClient.cs) class to connect to a chat room.

Alternatively, if you just want to create a bare bones connection without the extra functionality that the [ChatClient](https://github.com/tphx/StreamChatSharp/blob/master/StreamChatSharp/StreamChatSharp/ChatClient.cs) provides, you can create an instance of the [Connection](https://github.com/tphx/StreamChatSharp/blob/master/StreamChatSharp/StreamChatSharp/Connection.cs) class. 

----------

##ChatMessages

Messages are exchanged with the ChatClient class via ChatMessages. Chat messages can contain a variety of information based on the command that they contain. At a minimum, each incoming ChatMessage should contain a command. When sending messages to the chat it is sometimes necessary to send the message in raw format. To send a raw message to the server, either use the SendRawMessage method in the ChatClient class, or create a ChatMessage and set the Command property to "RAW".

#####ChatMessage Layout:
* <b>Command</b> - The command (ex. PRIVMSG) that describes the purpose of the message.
* <b>Message</b> - The actual message portion of the ChatMessage. When the ChatMessage is for the MODE command, this property will contain the mode to set on the target (ex. +o). When used with the NAMES command, this message will contain the list of names delimited by spaces.
* <b>ChannelName</b> - The name of the chat channel the message pertains to.
* <b>Source</b> - The name of message sender.
* <b>Target</b> - The target of the message. This is mostly used for MODE commands where the target is the user who is having their MODE set.
* <b>Tags</b> - IRCv3 capability tags. These are not sent by default and must be requested ([ChatClient](https://github.com/tphx/StreamChatSharp/blob/master/StreamChatSharp/StreamChatSharp/ChatClient.cs) includes the option to register for these when connecting).

#####Common ChatMessage Commands:
* <b>421</b> - An invalid command (identified in the Message property) has been sent to the IRC server.
```
Command = 421,
Message = Invalid Command: BADCOMMANDNAME,
Source = tmi
```
* <b>JOIN</b> - When sent to the server JOINs a channel. When received from the server indicates the user specified in Source joined (requires membership capability).
```
Command = JOIN,
ChannelName = #channeltojoin,
Source = username
```
* <b>PART</b> - When sent to the server PARTs (leaves) the channel. When received from the server indicates the user specified in Source PART'd (requires membership capability).
```
Command = PART,
ChannelName = #channeltojoin,
Source = myusername
```
* <b>PING</b> - A PING to or from the server. A PONG is sent automatically when this command is received from the server.
```
Command = PING
```
* <b>PONG</b> - Received when the IRC server responds to a PING command. Sent automatically when a PING command is received from the server.
```
Command = PONG
```
* <b>PRIVMSG</b> - Indicates the message is a private message. This is how messages are sent to and received from chat channels.
```
Command = PRIVMSG,
ChannelName = #channelname,
Message = This is the private message.
Source = messagesendername,
Tags = color=#FF0000;display-name=MessageSenderName;emote-sets=0;subscriber=0;turbo=0;user-type=
```
* <b>RAW</b> - Indicates the Message property contains a raw IRC message. If a ChatMesage containing this command is received it means the ChatClient could not recognize the command that was issued. If this command is used in an outgoing message, the Message property will be sent as a RAW message.
```
Command = RAW,
Message = PRIVMSG #somechannelname :This is how you send a raw private message to #somechannelname.
```
* <b>MODE</b> - Received from the server to indicated the user specified in Target gained or lost moderator status.
```
Command = MODE,
Message = +o,
ChannelName = #channelname,
Target = username
```
----------
 
*Disclaimer:
This repository is an independent third party library. It is in no way, officially or unofficially, licensed, supported, endorsed, or even acknowledged to exist by [Twitch.tv](http://www.twitch.tv/). It is meant to interact with the publicly available IRC interface that Twitch provides.*
