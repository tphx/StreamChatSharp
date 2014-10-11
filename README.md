StreamChatSharp
===============

##About

StreamChatSharp is a C# library designed specifically for use with the [Twitch.tv IRC chat service](http://help.twitch.tv/customer/portal/articles/1302780-twitch-irc). Although any IRC server can be connected to, functionality is not guaranteed. This is because Twitch IRC works differently to standard IRC servers. It only uses a small portion of standard IRC commands. In addition, it is also used to relay Twitch specific information such as subscriber notifications, special user events (staff joined, etc.), and sending user information such as the color the user has selected their username to display as.

This library was designed with the hope that it would make the creation of chat bots and other applications that use the [Twitch.tv IRC chat service](http://help.twitch.tv/customer/portal/articles/1302780-twitch-irc) much easier. There are many IRC clients available for C# and .NET in general; however, they are designed for use with full featured IRC servers. This library provides a lighter weight alternative to such libraries by removing the uneeded features. 

----------

##Requirements


 * [.NET Framework 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653)

----------

##How to use

Checkout the  [Examples/SimpleChatBot](https://github.com/tphx/StreamChatSharp/tree/master/Examples/SimpleChatBot) directory to see an example of a simple chat bot created with the StreamChatSharp library for an example on how to use it.

----------

##ChatMessages

Messages are exchanged with the ChatClient class via ChatMessages. Chat messages can contain a variety of information based on the command that they contain. At a minimum, each ChatMessage should contain a command. When sending messages to the chat it is sometimes necessary to send the message in raw format. To send a raw message to the server, either use the SendRawMessage method in the ChatClient class, or create a ChatMessage and set the Command property to "RAW".

#####ChatMessage Layout:
* <b>Source</b> - The nickname of message sender. This property is not used with outgoing ChatMessages.
* <b>Target</b> - The target of the message. This is mostly used for mode commands where the target is the user who is having their MODE set. This property is not used with outgoing ChatMessages.
* <b>Channel</b> - The name of the chat channel the message pertains to. This property is optional in outgoing ChatMessages, but is required if the Message property is defined.
* <b>Command</b> - The command (ex. PRIVMSG) that describes the purpose of the message. This property is the only property that is required to be set for all types of ChatMessages.
* <b>Message</b> - The actual message portion of the ChatMessage. When the ChatMessage is for the MODE command, this property will contain the mode to set on the target (ex. +o). When used with the NAMES command, this message will contain the list of names delimited by spaces. This property is optional for outgoing ChatMessages. If this property is defined in an outgoing message, the Channel property should also be defined.

#####Common ChatMessage Commands:
* <b>353</b> - Indicates that the Message property contains a list of names of current chat users delimited by spaces.
```
Channel = #channelname,
Command = 353,
Message = somename anothername thirdname
```
* <b>366</b> - Indicates the list of names for a channel (353 command) has finished listing all users.
```
Channel = #channelname,
Command = 366,
Message = :End of /NAMES list.
```
* <b>421</b> - An invalid command (identified in the Message property) has been sent to the IRC server.
```
Source = tmi,
Command = 421,
Message = Invalid Command: BADCOMMANDNAME
```
* <b>JOIN</b> - JOINs the channel specified in the Channel property.
```
Source = username, <--- Source is not required when sending a JOIN message but is included when receiving it.
Channel = #channeltojoin,
Command = JOIN
```
* <b>PART</b> - PARTs (leaves) the channel specified in the Channel property.
```
Source = myusername, <--- Source is not required when sending a PART message but is included when receiving it.
Channel = #channeltojoin,
Command = PART
```
* <b>PING</b> - Indicates a PING has been received from the server. A PONG is sent automatically when this command is received.
```
Command = PING
```
* <b>PONG</b> - Received when the IRC server receives a successful PING command.
```
Command = PONG
```
* <b>PRIVMSG</b> - Indicates the message is a private message. This is how messages are sent to and received from chat channels.
```
Source = messagesendername,
Channel = #channelname,
Command = PRIVMSG,
Message = This is the private message.
```
* <b>RAW</b> - Indicates the Message property contains a raw IRC message. If a ChatMesage containing this command is received it means the ChatClient could not recognize the command that was issued. If this command is used in an outgoing message, the Message property will be sent as a RAW message.
```
Command = RAW,
Message = PRIVMSG #somechannelname :This is how you send a raw private message to #somechannelname.
```

----------
 
*Disclaimer:
This repository is an independent third party library. It is in no way, officially or unofficially, licensed, supported, endorsed, or even acknowledged to exist by [Twitch.tv](http://www.twitch.tv/). It is meant to interact with the publicly available IRC chat service that Twitch.tv provides.*
