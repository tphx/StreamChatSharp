// The MIT License (MIT)
//
// Copyright (c) 2014 TPHX
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// A client for connecting and exchanging messages with the Twitch IRC server.
    /// </summary>
    public class ChatClient : IDisposable
    {
        /// <summary>
        /// Triggered whenever a raw message is received.
        /// </summary>
        public event EventHandler<RawMessageEventArgs> RawMessageReceived;

        /// <summary>
        /// Triggered whenever a chat message is received.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> ChatMessageReceived;

        /// <summary>
        /// Triggered whenever the connection is disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Triggered when the connection has successfully registered with the server.
        /// </summary>
        public event EventHandler RegisteredWithServer;

        /// <summary>
        /// Triggered whenever the TWITCHCLIENT connection is disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> TwitchClientDisconnected;

        /// <summary>
        /// Triggered whenever a TWITCHCLIENT chat message is received.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> TwitchClientChatMessageReceived;

        /// <summary>
        /// Triggered when the connection has successfully registered with the server.
        /// </summary>
        public event EventHandler TwitchClientRegisteredWithServer;

        private Connection chatConnection; // Connection for chatting.
        private Connection clientConnection; // TWITCHCLIENT connection.
        private ConcurrentDictionary<string, ChatChannel> channels = new ConcurrentDictionary<string, ChatChannel>();
        private bool connectionTimedOut = false;
        private bool disposed = false;

        /// <summary>
        /// Disposes of everything and disconnects from the chat server.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Connects to the Twitch IRC server using the connection data.
        /// </summary>
        /// <param name="connectionData">Data to connect with.</param>
        public void ConnectToChat(ConnectionData connectionData)
        {
            this.chatConnection = new Connection(connectionData);
            this.chatConnection.RawMessageReceived += OnRawMessageReceived;
            this.chatConnection.ChatMessageReceived += OnChatMessageReceived;
            this.chatConnection.Disconnected += OnDisconnected;
            this.chatConnection.RegisteredWithServer += OnRegisteredWithServer;
        }

        /// <summary>
        /// Connects to the Twitch IRC server using the connection data with the option to create a second connection
        /// for receiving TWITCHCLIENT messages.
        /// </summary>
        /// <param name="connectionData">Data to connect with.</param>
        /// <param name="useTwitchClient">Whether or not to register a second connection for processiong TWITCHCLIENT 
        /// messages. Current version: IrcV3 (TWITCHCLIENT 3)</param>
        public void ConnectToChat(ConnectionData connectionData, bool useTwitchClient)
        {
            ConnectToChat(connectionData);

            if (useTwitchClient)
            {
                this.clientConnection = new Connection(connectionData);
                this.clientConnection.RawMessageReceived += OnClientRawMessageReceived;
                this.clientConnection.ChatMessageReceived += OnClientChatMessageReceived;
                this.clientConnection.Disconnected += OnClientDisconnected;
                this.clientConnection.RegisteredWithServer += OnClientRegisteredWithServer;
            }
        }

        /// <summary>
        /// Disconnects from the Twitch IRC server.
        /// </summary>
        public void Disconnect()
        {
            if (this.chatConnection != null && this.chatConnection.Connected)
            {
                this.chatConnection.RawMessageReceived -= OnRawMessageReceived;
                this.chatConnection.ChatMessageReceived -= OnChatMessageReceived;
                this.chatConnection.Disconnected -= OnDisconnected;
                this.chatConnection.RegisteredWithServer -= OnRegisteredWithServer;
                this.chatConnection.Disconnect();
                this.chatConnection.Dispose();

                this.channels.Clear();

                if (this.Disconnected != null)
                {
                    this.Disconnected(this,
                        new DisconnectedEventArgs(DisconnectedEventArgs.DisconnectReason.ClientDisconnected, false));
                }
            }

            if (this.clientConnection != null && this.clientConnection.Connected)
            {
                this.clientConnection.RawMessageReceived -= OnClientRawMessageReceived;
                this.clientConnection.ChatMessageReceived -= OnClientChatMessageReceived;
                this.clientConnection.Disconnected -= OnClientDisconnected;
                this.clientConnection.RegisteredWithServer -= OnClientRegisteredWithServer;
                this.clientConnection.Disconnect();
                this.clientConnection.Dispose();

                if (this.TwitchClientDisconnected != null)
                {
                    this.TwitchClientDisconnected(this,
                        new DisconnectedEventArgs(DisconnectedEventArgs.DisconnectReason.ClientDisconnected, false));
                }
            }
        }

        /// <summary>
        /// Sends a ChatMessage to the chat server.
        /// </summary>
        /// <param name="message">The chat message to send.</param>
        /// <param name="highPriorityMessage">Whether or not the message is a high priority message.</param>
        public void SendChatMessage(ChatMessage message, bool highPriorityMessage)
        {
            this.chatConnection.SendChatMessage(message, highPriorityMessage);
        }

        /// <summary>
        /// Sends a private message (PRIVMSG) to the chat server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="channelName">The channel to send the message to.</param>
        /// <param name="highPriorityMessage">Whether or not the message is a high priority message.</param>
        public void SendPrivateMessage(string message, string channelName, bool highPriorityMessage)
        {
            SendChatMessage(new ChatMessage("PRIVMSG", message, channelName), highPriorityMessage);
        }

        /// <summary>
        /// Sends a raw message to the chat server.
        /// </summary>
        /// <param name="rawMessage">The raw message to send to the chat server.</param>
        /// <param name="highPriorityMessage">Whether or not the message is a high priority message.</param>
        public void SendRawMessage(string rawMessage, bool highPriorityMessage)
        {
            SendChatMessage(new ChatMessage("RAW", rawMessage), highPriorityMessage);
        }

        /// <summary>
        /// Joins a chat chanel.
        /// </summary>
        /// <param name="channelName">The name of the channel to join.</param>
        public void JoinChannel(string channelName)
        {
            if (!string.IsNullOrWhiteSpace(channelName) && !IsInChatChannel(channelName))
            {
                SendChatMessage(new ChatMessage("JOIN", channelName), true);
                this.clientConnection.SendChatMessage(new ChatMessage("JOIN", channelName), true);
                this.channels.AddOrUpdate(channelName, new ChatChannel(channelName), ((key, oldValue) => oldValue));
            }
        }

        /// <summary>
        /// Joins multiple chat channels.
        /// </summary>
        /// <param name="channelNames">Collection of channel names to join.</param>
        public void JoinChannel(IList<string> channelNames)
        {
            foreach (string currentChannelName in channelNames)
            {
                JoinChannel(currentChannelName);
            }
        }

        /// <summary>
        /// Leaves a chat channel.
        /// </summary>
        /// <param name="channelName">The name of the channel to leave.</param>
        public void LeaveChannel(string channelName)
        {
            if (!string.IsNullOrWhiteSpace(channelName))
            {
                SendChatMessage(new ChatMessage("PART", channelName), true);
                this.clientConnection.SendChatMessage(new ChatMessage("PART", channelName), true);
                ChatChannel channelToRemove;
                this.channels.TryRemove(channelName, out channelToRemove);
            }
        }

        /// <summary>
        /// Gets a collection of the chat channels the client is currently in.
        /// </summary>
        /// <returns>Collection of current chat channels.</returns>
        public ReadOnlyCollection<ChatChannel> Channels
        {
            get
            {
                return new ReadOnlyCollection<ChatChannel>(this.channels.Values.ToList());
            }
        }

        /// <summary>
        /// Whether or not the client is connected to the IRC server.
        /// </summary>
        public bool Connected
        {
            get
            {
                return this.chatConnection.Connected;
            }
        }

        /// <summary>
        /// Whether or not the TWITCHCLIENT is connected to the IRC server.
        /// </summary>
        public bool TwitchClientConnected
        {
            get
            {
                return this.clientConnection.Connected;
            }
        }

        /// <summary>
        /// The amount of time (in milliseconds) to wait between sent messages. By default this is set to 1.6 seconds 
        /// between messages (20 messages in 32 seconds). Set it to 0 to disable it.
        /// </summary>
        public double MessageSendInterval
        {
            get
            {
                return this.chatConnection.MessageSendInterval;
            }
            set
            {
                this.chatConnection.MessageSendInterval = value;
            }
        }

        /// <summary>
        /// Returns whether or not the client is in a chat channel.
        /// </summary>
        /// <param name="channelName">Name of the channel to check.</param>
        /// <returns>Whether or not the client is in a chat channel.</returns>
        public bool IsInChatChannel(string channelName)
        {
            return this.channels.ContainsKey(channelName);
        }

        /// <summary>
        /// Server connection data.
        /// </summary>
        public ConnectionData ConnectionData
        {
            get
            {
                return chatConnection.ConnectionData;
            }
        }

        /// <summary>
        /// Whether or not the connection has been registered with the server.
        /// </summary>
        public bool ConnectionRegistered
        {
            get
            {
                return this.chatConnection.ConnectionRegistered;
            }
        }

        /// <summary>
        /// Whether or not the TWITCHCLIENT connection has been registered.
        /// </summary>
        public bool TwitchClientConnectionRegistered
        {
            get
            {
                return this.clientConnection.ConnectionRegistered; ;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }

                this.disposed = true;
            }
        }

        private void OnRawMessageReceived(object sender, RawMessageEventArgs e)
        {
            if (this.RawMessageReceived != null)
            {
                this.RawMessageReceived(sender, e);
            }
        }

        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            ProcessChatMessage(e.ChatMessage);

            if (this.ChatMessageReceived != null)
            {
                this.ChatMessageReceived(sender, e);
            }
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            if (e.Reason == DisconnectedEventArgs.DisconnectReason.TimedOut)
            {
                this.connectionTimedOut = true;
            }

            if (this.Disconnected != null)
            {
                Disconnected(sender, e);
            }
        }

        private void OnRegisteredWithServer(object sender, EventArgs e)
        {
            // If we are connecting after a timeout we need to rejoin all of the channels we are supposed to be in.
            if (connectionTimedOut)
            {
                connectionTimedOut = false;

                foreach (KeyValuePair<string, ChatChannel> channel in this.channels)
                {
                    SendChatMessage(new ChatMessage("JOIN", channel.Key), true);
                }
            }

            if (this.RegisteredWithServer != null)
            {
                this.RegisteredWithServer(sender, e);
            }
        }

        private void ProcessChatMessage(ChatMessage chatMessage)
        {
            // If the user joined a channel any way other than the Join method the channel may not have been added to
            // the list. The channel needs to be in the list before the message can be proccessed if the message is
            // for a channel. Channels start with a #.
            if (!string.IsNullOrWhiteSpace(chatMessage.ChannelName) && chatMessage.ChannelName.StartsWith("#") &&
                !IsInChatChannel(chatMessage.ChannelName) && chatMessage.Command != "PART")
            {
                this.clientConnection.SendChatMessage(new ChatMessage("JOIN", chatMessage.ChannelName), true);
                this.channels.AddOrUpdate(chatMessage.ChannelName, new ChatChannel(chatMessage.ChannelName), 
                    ((key, oldValue) => oldValue));
            }

            if (IsInChatChannel(chatMessage.ChannelName))
            {
                switch (chatMessage.Command)
                {
                    case "JOIN":
                        JoinReceived(chatMessage);
                        break;
                    case "PART":
                        PartReceived(chatMessage);
                        break;
                    case "353":
                        NamesListReceived(chatMessage);
                        break;
                    case "MODE":
                        ModeReceived(chatMessage);
                        break;
                }
            }
        }

        private void JoinReceived(ChatMessage chatMessage)
        {
            this.channels[chatMessage.ChannelName].AddChatUser(chatMessage.Source);
        }

        private void NamesListReceived(ChatMessage chatMessage)
        {
            string[] userNames = chatMessage.Message.Split(' ');

            // Names list is basically a compact list of JOINs for people who are already in the channel when we join.
            for (int a = 0; a < userNames.Length; a++)
            {
                JoinReceived(
                    new ChatMessage()
                    {
                        Command = "JOIN",
                        ChannelName = chatMessage.ChannelName,
                        Source = userNames[a]
                    });
            }
        }

        private void PartReceived(ChatMessage chatMessage)
        {
            this.channels[chatMessage.ChannelName].RemoveChatUser(chatMessage.Source);
        }

        private void ModeReceived(ChatMessage chatMessage)
        {
            if (chatMessage.Message == "+o")
            {
                this.channels[chatMessage.ChannelName].ToggleSpecialUserType(chatMessage.Target,
                    ChatUser.SpecialUserType.Moderator, true);
            }
            else if (chatMessage.Message == "-o")
            {
                this.channels[chatMessage.ChannelName].ToggleSpecialUserType(chatMessage.Target,
                    ChatUser.SpecialUserType.Moderator, false);
            }
        }

        private void OnClientRawMessageReceived(object sender, RawMessageEventArgs e)
        {
        }

        private void OnClientChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if (this.IsInChatChannel(e.ChatMessage.ChannelName))
            {
                if(e.ChatMessage.Source.Equals("jtv", StringComparison.OrdinalIgnoreCase))
                {
                    if(e.ChatMessage.Message.StartsWith("SPECIALUSER", StringComparison.OrdinalIgnoreCase))
                    {
                        // Message should be in the following format:
                        // SPECIALUSER [username] [level]
                        string[] messageParts = e.ChatMessage.Message.Split(new char[] { ' ' }, 
                            StringSplitOptions.RemoveEmptyEntries);

                        switch(messageParts[2].ToLower())
                        {
                            case "subscriber":
                                this.channels[e.ChatMessage.ChannelName].ToggleSpecialUserType(messageParts[1], 
                                    ChatUser.SpecialUserType.Subscriber, true);
                                break;
                            case "turbo":
                                this.channels[e.ChatMessage.ChannelName].ToggleSpecialUserType(messageParts[1],
                                    ChatUser.SpecialUserType.Turbo, true);
                                break;
                            case "global_moderator":
                                this.channels[e.ChatMessage.ChannelName].ToggleSpecialUserType(messageParts[1],
                                    ChatUser.SpecialUserType.GlobalModerator, true);
                                break;
                            case "staff":
                                this.channels[e.ChatMessage.ChannelName].ToggleSpecialUserType(messageParts[1],
                                    ChatUser.SpecialUserType.Staff, true);
                                break;
                        }
                    }
                }
                if(TwitchClientChatMessageReceived != null)
                {
                    this.TwitchClientChatMessageReceived(sender, e);
                }
            }
        }

        private void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
        }

        private void OnClientRegisteredWithServer(object sender, EventArgs e)
        {
            this.clientConnection.SendChatMessage(new ChatMessage("RAW", "TWITCHCLIENT 3"), true);

            if(this.TwitchClientRegisteredWithServer != null)
            {
                this.TwitchClientRegisteredWithServer(this, new EventArgs());
            }
        }
    }
}