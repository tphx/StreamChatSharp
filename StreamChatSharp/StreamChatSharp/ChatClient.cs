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
        /// Triggered whenever a chat message is sent.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> ChatMessageSent;

        private Connection chatConnection;
        private ConcurrentDictionary<string, ChatChannel> channels = new ConcurrentDictionary<string, ChatChannel>();
        private bool connectionTimedOut = false;
        private bool disposed = false;
        private bool tagsCapEnabled = false;
        private bool membershipCapEnabled = false;
        private bool commandsCapEnabled = false;

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
            if (!this.Connected)
            {
                this.chatConnection = new Connection();
                this.chatConnection.RawMessageReceived += OnRawMessageReceived;
                this.chatConnection.ChatMessageReceived += OnChatMessageReceived;
                this.chatConnection.Disconnected += OnDisconnected;
                this.chatConnection.RegisteredWithServer += OnRegisteredWithServer;
                this.chatConnection.ChatMessageSent += OnChatMessageSent;
                this.chatConnection.ConnectToServer(connectionData);
            }
        }

        /// <summary>
        /// Connects to the Twitch IRC server using the connection data and enables IRCv3 capabilities.
        /// </summary>
        /// <param name="connectionData">Data to connect with.</param>
        /// <param name="enableTagsCap">Whether or not to enable the tags capability.</param>
        /// <param name="enableMembershipCap">Whether or not to enable the membership capability.</param>
        /// <param name="enableCommandsCap">Whether or not to enable the commands capability.</param>
        public void ConnectToChat(ConnectionData connectionData, bool enableTagsCap, bool enableMembershipCap,
            bool enableCommandsCap)
        {
            ConnectToChat(connectionData);
            this.tagsCapEnabled = enableTagsCap;
            this.membershipCapEnabled = enableMembershipCap;
            this.commandsCapEnabled = enableCommandsCap;
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
                this.chatConnection.ChatMessageSent -= OnChatMessageSent;
                this.chatConnection.Disconnect();
                this.chatConnection.Dispose();

                this.channels.Clear();

                if (this.Disconnected != null)
                {
                    this.Disconnected(this,
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
            if (!String.IsNullOrWhiteSpace(channelName) && !IsInChatChannel(channelName))
            {
                SendChatMessage(new ChatMessage("JOIN", channelName), true);
                this.channels.AddOrUpdate(channelName, new ChatChannel(channelName), ((key, oldValue) => oldValue));
            }
        }

        /// <summary>
        /// Joins multiple chat channels.
        /// </summary>
        /// <param name="channelNames">Collection of channel names to join.</param>
        public void JoinChannel(IList<string> channelNames)
        {
            if (channelNames != null)
            {
                foreach (string currentChannelName in channelNames)
                {
                    JoinChannel(currentChannelName);
                }
            }
        }

        /// <summary>
        /// Leaves a chat channel.
        /// </summary>
        /// <param name="channelName">The name of the channel to leave.</param>
        public void LeaveChannel(string channelName)
        {
            if (!String.IsNullOrWhiteSpace(channelName))
            {
                SendChatMessage(new ChatMessage("PART", channelName), true);
                ChatChannel channelToRemove;
                this.channels.TryRemove(channelName, out channelToRemove);
            }
        }

        /// <summary>
        /// Gets a dictionary of the chat channels the client is currently in.
        /// </summary>
        /// <returns>Collection of current chat channels.</returns>
        public ConcurrentDictionary<string, ChatChannel> Channels
        {
            get
            {
                return this.channels;
            }
        }

        /// <summary>
        /// Whether or not the client is connected to the IRC server.
        /// </summary>
        public bool Connected
        {
            get
            {
                return (this.chatConnection != null && this.chatConnection.Connected);
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
            if(this.tagsCapEnabled)
            {
                SendRawMessage("CAP REQ :twitch.tv/tags", true);
            }

            if(this.membershipCapEnabled)
            {
                SendRawMessage("CAP REQ :twitch.tv/membership", true);
            }

            if(this.commandsCapEnabled)
            {
                SendRawMessage("CAP REQ :twitch.tv/commands", true);
            }

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
            if (!String.IsNullOrWhiteSpace(chatMessage.ChannelName) && chatMessage.ChannelName.StartsWith("#") &&
                !IsInChatChannel(chatMessage.ChannelName) && chatMessage.Command != "PART")
            {
                this.channels.AddOrUpdate(chatMessage.ChannelName, new ChatChannel(chatMessage.ChannelName),
                    ((key, oldValue) => oldValue));
            }

            if (IsInChatChannel(chatMessage.ChannelName))
            {
                switch (chatMessage.Command)
                {
                    case "JOIN":
                    case "PART":
                    case "PRIVMSG":
                        this.channels[chatMessage.ChannelName].SetUserState(chatMessage);
                        break;
                    case "USERSTATE":
                        // The source of USERSTATE is tmi but it describes us so we need to change it.
                        chatMessage.Source = this.ConnectionData.Nickname;
                        this.channels[chatMessage.ChannelName].SetUserState(chatMessage);
                        break;
                    case "353":
                        NamesListReceived(chatMessage);
                        break;
                    case "MODE":
                        ModeReceived(chatMessage);
                        break;
                    case "ROOMSTATE":
                        this.channels[chatMessage.ChannelName].SetRoomState(chatMessage);
                        break;
                }
            }
        }

        private void NamesListReceived(ChatMessage chatMessage)
        {
            string[] userNames = chatMessage.Message.Split(' ');

            // Names list is basically a compact list of JOINs for people who are already in the channel when we join.
            for (int a = 0; a < userNames.Length; a++)
            {
                this.channels[chatMessage.ChannelName].SetUserState(
                    new ChatMessage()
                    {
                        Command = "JOIN",
                        Source = userNames[a]
                    });
            }
        }

        private void ModeReceived(ChatMessage chatMessage)
        {
            if(String.Equals(chatMessage.Message, "+o", StringComparison.OrdinalIgnoreCase))
            {
                this.channels[chatMessage.ChannelName].SetUserState(
                    new ChatMessage()
                    {
                        Command = "MODE",
                        Source = chatMessage.Target,
                        Tags = "user-type=mod"
                    });
            }
            else if(String.Equals(chatMessage.Message, "-o", StringComparison.OrdinalIgnoreCase))
            {
                this.channels[chatMessage.ChannelName].SetUserState(
                    new ChatMessage()
                    {
                        Command = "MODE",
                        Source = chatMessage.Target,
                        Tags = "user-type="
                    });
            }
        }

        private void OnChatMessageSent(object sender, ChatMessageEventArgs e)
        {
            if(this.ChatMessageSent != null)
            {
                this.ChatMessageSent(sender, e);
            }
        }
    }
}