using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Client used to interact with the Twitch chat.
    /// </summary>
    public class ChatClient
    {
        private bool reconnectingToChat = false;
        private bool disposed = false;

        /// <summary>
        /// Constructs a new chat client.
        /// </summary>
        public ChatClient()
        {
            Connection = new Connection();
            Connection.ChatMessageReceived += OnChatMessageReceived;
            Connection.RegisteredWithServer += OnRegisteredWithServer;
            Connection.Disconnected += OnDisconnected;

            Channels = new ConcurrentDictionary<string, ChatChannel>();
        }

        /// <summary>
        /// Connection to the chat server.
        /// </summary>
        public Connection Connection { get; private set; }

        /// <summary>
        /// Channels the client is currently in.
        /// </summary>
        public ConcurrentDictionary<string, ChatChannel> Channels { get; private set; }

        /// <summary>
        /// Disposes of everything.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Joins the specified channels.
        /// </summary>
        /// <param name="channelNames">Names of the channels to join.</param>
        public void JoinChannel(IList<string> channelNames)
        {
            foreach(string channelName in channelNames)
            {
                JoinChannel(channelName);
            }
        }

        /// <summary>
        /// Joins the specified channel.
        /// </summary>
        /// <param name="channelName">Name of the channel to join.</param>
        public void JoinChannel(string channelName)
        { 
            if (!channelName.StartsWith("#"))
            {
                channelName = channelName.Insert(0, "#");
            }

            if (!Channels.ContainsKey(channelName))
            {
                Connection.SendChatMessage(new ChatMessage("JOIN", channelName.ToLower().Trim()), true);
                Channels.AddOrUpdate(channelName, new ChatChannel(channelName), (key, oldValue) => oldValue);
            }
        }
        
        /// <summary>
        /// Leaves the channels specified.
        /// </summary>
        /// <param name="channelNames">Names of the channels to leave.</param>
        public void LeaveChannel(IList<string> channelNames)
        {
            foreach(string channelName in channelNames)
            {
                LeaveChannel(channelName);
            }
        }

        /// <summary>
        /// Leaves the specified channel.
        /// </summary>
        /// <param name="channelName">Name of the channel to leave.</param>
        public void LeaveChannel(string channelName)
        {
            ChatChannel channel;
            Channels.TryRemove(channelName, out channel);
        }

        /// <summary>
        /// Sends a private message to the channel specified.
        /// </summary>
        /// <param name="channelName">Name of the channel to send the message to.</param>
        /// <param name="message">Message to send.</param>
        /// <param name="isHighPriority">Whether or not the message is a high priority message.</param>
        public void SendMessage(string channelName, string message, bool isHighPriority)
        {
            Connection.SendChatMessage(new ChatMessage("PRIVMSG", message, channelName), isHighPriority);
        }

        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    Connection.Dispose();
                }

                disposed = true;
            }
        }

        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if(String.Equals(e.ChatMessage.Source, Connection.ConnectionData.Nickname, StringComparison.OrdinalIgnoreCase) &&
               String.Equals(e.ChatMessage.Command, "PART", StringComparison.OrdinalIgnoreCase))
            {
                LeaveChannel(e.ChatMessage.ChannelName);
            }
            // We only want to create channels for actual channels, not if we get a message to us or anything else.
            else if (!String.IsNullOrWhiteSpace(e.ChatMessage.ChannelName) && e.ChatMessage.ChannelName.StartsWith("#"))
            {
                Channels.AddOrUpdate(e.ChatMessage.ChannelName, new ChatChannel(e.ChatMessage.ChannelName),
                    (key, oldValue) => oldValue);
                Channels[e.ChatMessage.ChannelName].ProcessChatMessage(e.ChatMessage);
            }
        }

        private void OnRegisteredWithServer(object sender, EventArgs e)
        {
            Connection.SendChatMessage(new ChatMessage("CAP REQ :twitch.tv/tags"), true);
            Connection.SendChatMessage(new ChatMessage("CAP REQ :twitch.tv/commands"), true);
            Connection.SendChatMessage(new ChatMessage("CAP REQ :twitch.tv/membership"), true);

            if(reconnectingToChat)
            {
                JoinChannel(Channels.Keys.ToList());
                reconnectingToChat = false;
            }
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            if(e.AttemptingAutoReconnect)
            {
                reconnectingToChat = true;
            }
            else
            {
                Channels.Clear();
            }
        }
    }
}
