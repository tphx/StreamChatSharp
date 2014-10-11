// The MIT License (MIT)
//
// Copyright (c) 2014 TPHX
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
// the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        private Connection connection;
        private List<string> chatChannels = new List<string>();

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
            this.connection = new Connection(connectionData);
            this.connection.RawMessageReceived += OnRawMessageReceived;
            this.connection.ChatMessageReceived += OnChatMessageReceived;
            this.connection.Disconnected += OnDisconnected;
        }

        /// <summary>
        /// Disconnects from the Twitch IRC server.
        /// </summary>
        public void Disconnect()
        {
            if (this.connection.Connected)
            {
                this.connection.RawMessageReceived -= OnRawMessageReceived;
                this.connection.ChatMessageReceived -= OnChatMessageReceived;
                this.connection.Disconnected -= OnDisconnected;
                this.connection.Disconnect();
                this.connection.Dispose();

                this.chatChannels.Clear();

                if (this.Disconnected != null)
                {
                    this.Disconnected(this,
                        new DisconnectedEventArgs()
                        {
                            Reason = DisconnectedEventArgs.DisconnectReason.ClientDisconnected
                        });
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
            this.connection.SendChatMessage(message, highPriorityMessage);
        }

        /// <summary>
        /// Sends a private message (PRIVMSG) to the chat server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="channelName">The channel to send the message to.</param>
        /// <param name="highPriorityMessage">Whether or not the message is a high priority message.</param>
        public void SendPrivateMessage(string message, string channelName, bool highPriorityMessage)
        {
            SendChatMessage(
                new ChatMessage()
                {
                    Command = "PRIVMSG",
                    Channel = channelName,
                    Message = message
                },
                highPriorityMessage);
        }

        /// <summary>
        /// Sends a raw message to the chat server.
        /// </summary>
        /// <param name="rawMessage">The raw message to send to the chat server.</param>
        /// <param name="highPriorityMessage">Whether or not the message is a high priority message.</param>
        public void SendRawMessage(string rawMessage, bool highPriorityMessage)
        {
            SendChatMessage(
                new ChatMessage()
                {
                    Command = "RAW",
                    Message = rawMessage
                }, 
                highPriorityMessage);
        }

        /// <summary>
        /// Joins a chat chanel.
        /// </summary>
        /// <param name="channelName">The name of the channel to join.</param>
        public void JoinChannel(string channelName)
        {
            if (!string.IsNullOrWhiteSpace(channelName) && !chatChannels.Contains(channelName))
            {
                SendChatMessage(
                    new ChatMessage()
                    {
                        Command = "JOIN",
                        Message = channelName
                    },
                    true);

                chatChannels.Add(channelName);
            }
        }

        /// <summary>
        /// Joins multiple chat channels.
        /// </summary>
        /// <param name="channelNames">Collection of channel names to join.</param>
        public void JoinChannel(IList<string> channelNames)
        {
            foreach(string currentChannelName in channelNames)
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
            if (!string.IsNullOrWhiteSpace(channelName) && this.chatChannels.Contains(channelName))
            {
                SendChatMessage(
                    new ChatMessage()
                    {
                        Command = "PART",
                        Message = channelName
                    },
                    true);

                this.chatChannels.Remove(channelName);
            }
        }

        /// <summary>
        /// Gets a collection of the chat channels the client is currently in.
        /// </summary>
        /// <returns>Collection of current chat channels.</returns>
        public ReadOnlyCollection<string> ChatChannels
        {
            get
            {
                return new ReadOnlyCollection<string>(this.chatChannels);
            }
        }

        /// <summary>
        /// Whether or not the client is connected to the IRC server.
        /// </summary>
        public bool Connected
        {
            get
            {
                return this.connection.Connected;
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
                return this.connection.MessageSendInterval;
            }
            set
            {
                this.connection.MessageSendInterval = value;
            }
        }

        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    Disconnect();
                }

                this.disposed = true;
            }
        }

        private void OnRawMessageReceived(object sender, RawMessageEventArgs e)
        {
            if(this.RawMessageReceived != null)
            {
                this.RawMessageReceived(sender, e);
            }
        }

        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if (this.ChatMessageReceived != null)
            {
                this.ChatMessageReceived(sender, e);
            }
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            if(this.Disconnected != null)
            {
                Disconnected(sender, e);
            }
        }
    }
}