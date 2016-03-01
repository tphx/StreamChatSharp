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
        private bool disposed = false;

        /// <summary>
        /// Constructs a new chat client.
        /// </summary>
        public ChatClient()
        {
            this.Connection = new Connection();
            this.Connection.ChatMessageReceived += OnChatMessageReceived;
            this.Connection.RegisteredWithServer += OnRegisteredWithServer;

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
        /// <param name="channelNames">Channels to join.</param>
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
            // We only want to create channels for actual channels, not if we get a message to us or anything else.
            if (!String.IsNullOrWhiteSpace(e.ChatMessage.ChannelName) && e.ChatMessage.ChannelName.StartsWith("#"))
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
        }
    }
}
