using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.Connection = new Connection();
            this.Connection.ChatMessageReceived += OnChatMessageReceived;
            this.Connection.RegisteredWithServer += OnRegisteredWithServer;
            this.Connection.Disconnected += OnDisconnected;

            this.Channels = new ConcurrentDictionary<string, ChatChannel>();
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
                this.Connection.SendChatMessage(new ChatMessage("JOIN", channelName.ToLower().Trim()), true);
                this.Channels.AddOrUpdate(channelName, new ChatChannel(channelName), (key, oldValue) => oldValue);
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
            this.Channels.TryRemove(channelName, out channel);
        }

        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    this.Connection.Dispose();
                }

                this.disposed = true;
            }
        }

        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if(String.Equals(e.ChatMessage.Source, this.Connection.ConnectionData.Nickname, StringComparison.OrdinalIgnoreCase) &&
               String.Equals(e.ChatMessage.Command, "PART", StringComparison.OrdinalIgnoreCase))
            {
                LeaveChannel(e.ChatMessage.ChannelName);
            }
            // We only want to create channels for actual channels, not if we get a message to us or anything else.
            else if (!String.IsNullOrWhiteSpace(e.ChatMessage.ChannelName) && e.ChatMessage.ChannelName.StartsWith("#"))
            {
                this.Channels.AddOrUpdate(e.ChatMessage.ChannelName, new ChatChannel(e.ChatMessage.ChannelName),
                    (key, oldValue) => oldValue);
                this.Channels[e.ChatMessage.ChannelName].ProcessChatMessage(e.ChatMessage);
            }
        }

        private void OnRegisteredWithServer(object sender, EventArgs e)
        {
            this.Connection.SendChatMessage(new ChatMessage("CAP REQ :twitch.tv/tags"), true);
            this.Connection.SendChatMessage(new ChatMessage("CAP REQ :twitch.tv/commands"), true);
            this.Connection.SendChatMessage(new ChatMessage("CAP REQ :twitch.tv/membership"), true);

            if(reconnectingToChat)
            {
                JoinChannel(this.Channels.Keys.ToList());
                this.reconnectingToChat = false;
            }
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            if(e.AttemptingAutoReconnect)
            {
                this.reconnectingToChat = true;
            }
            else
            {
                this.Channels.Clear();
            }
        }
    }
}
