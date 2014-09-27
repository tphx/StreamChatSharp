StreamChatSharp
--------------

####About

A C# library designed specifically for use with the [Twitch.tv IRC chat service](http://help.twitch.tv/customer/portal/articles/1302780-twitch-irc). Although any IRC server can be connected to, functionality is not guaranteed. This is because Twitch IRC works differently to standard IRC servers. It only uses a small portion of standard IRC commands. In addition, it is also used to relay Twitch specific information such as subscriber notifications, special user events (staff joined, etc.), and sending user information such as the color the user has selected their username to display as.

This library was designed with the hope that it would make the creation of chat bots and other applications that use the [Twitch.tv IRC chat service](http://help.twitch.tv/customer/portal/articles/1302780-twitch-irc) much easier. There are many IRC clients available for C# and .NET in general; however, they are designed for use with full featured IRC servers. This library provides a lighter weight alternative to such libraries by removing the uneeded features.

--------------
*Disclaimer:
This repository is an independent third party library. It is in no way, officially or unofficially, licensed, supported, endorsed, or even acknowledged by <a href="http://www.twitch.tv/">Twitch.tv</a>. It is meant to interact with the publicly available IRC chat service that Twitch.tv provides.*
