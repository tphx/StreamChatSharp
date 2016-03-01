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

        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(e.ChatMessage.ChannelName))
            {
                this.Channels.AddOrUpdate(e.ChatMessage.ChannelName, new ChatChannel(e.ChatMessage.ChannelName),
                    (key, oldValue) => oldValue);
                this.Channels[e.ChatMessage.ChannelName].ProcessChatMessage(e.ChatMessage);
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
    }
}
