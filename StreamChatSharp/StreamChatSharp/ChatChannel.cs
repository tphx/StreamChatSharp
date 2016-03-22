using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Data for a chat channel.
    /// </summary>
    public class ChatChannel
    {
        /// <summary>
        /// Constructs a new chat channel.
        /// </summary>
        /// <param name="channelName">Name of the channel</param>
        public ChatChannel(string channelName)
        { 
            this.Users = new ConcurrentDictionary<string, ChatUser>();
            this.ChannelName = channelName;
        }

        /// <summary>
        /// Name of the channel.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Users that are currently in the channel.
        /// </summary>
        public ConcurrentDictionary<string, ChatUser> Users { get; private set; }

        /// <summary>
        /// Lanaguage that is selected when broadcaster mode is enabled. English by default.
        /// </summary>
        public string BroadcasterLanguage { get; set; }

        /// <summary>
        /// Whether or not R9K mode is enabled.
        /// </summary>
        public bool R9KModeEnabled { get; set;  }

        /// <summary>
        /// Whether or not slow mode is enabled.
        /// </summary>
        public bool SlowModeEnabled { get; set; }

        /// <summary>
        /// The amount of seconds users must wait between meessages when slowmode is enabled.
        /// </summary>
        public int SlowModeInterval { get; set; }

        /// <summary>
        /// Whether or not subscribers only mode is enabled.
        /// </summary>
        public bool SubscribersOnlyModeEnabled { get; set; }

        /// <summary>
        /// Processes a chat message.
        /// </summary>
        /// <param name="chatMessage">Message to process.</param>
        internal void ProcessChatMessage(ChatMessage chatMessage)
        {
            switch(chatMessage.Source)
            {
                case "tmi":
                    SetRoomState(chatMessage.Tags);
                    break;
                case "jtv":
                    SetUserMode(chatMessage.Target, chatMessage.Message);
                    break;
                default:
                    this.Users.AddOrUpdate(chatMessage.Source, new ChatUser(chatMessage.Source, chatMessage.ChannelName),
                        (key, oldValue) => oldValue);
                    this.Users[chatMessage.Source].ProcessChatMessage(chatMessage);
                    break;
            }
        }

        private void SetUserMode(string user, string mode)
        {
            if (!String.IsNullOrWhiteSpace(user))
            {
                this.Users.AddOrUpdate(user, new ChatUser(user, this.ChannelName), (key, oldValue) => oldValue);
                this.Users[user].IsModerator = String.Equals(mode, "+o", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void SetRoomState(string roomStates)
        {
            // The room state can contain one state or various states delimited by semicolons.
            // broadcaster-lang=;r9k=0;slow=0;subs-only=0 
            string[] states = roomStates.Split(';');

            for (int a = 0; a < states.Length; a++)
            {
                string[] state = states[a].Split('=');

                switch (state[0])
                {
                    case "broadcaster-lang":
                        this.BroadcasterLanguage = state[1];
                        break;
                    case "r9k":
                        this.R9KModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "slow":
                        this.SlowModeInterval = Convert.ToInt32(state[1]);
                        this.SlowModeEnabled = (SlowModeInterval > 0);
                        break;
                    case "subs-only":
                        this.SubscribersOnlyModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                }
            }
        }
    }
}
