using System;
using System.Collections.Concurrent;

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
            Users = new ConcurrentDictionary<string, ChatUser>();
            ChannelName = channelName;
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
        /// Whether or not emote only mode is enabled.
        /// </summary>
        public bool EmoteOnlyModeEnabled { get; set; }

        /// <summary>
        /// Processes a chat message.
        /// </summary>
        /// <param name="chatMessage">Message to process.</param>
        internal void ProcessChatMessage(ChatMessage chatMessage)
        {
            switch(chatMessage.Source)
            {
                case "tmi":
                    ProcessTmiMessage(chatMessage);
                    break;
                case "jtv":
                    SetUserMode(chatMessage.Target, chatMessage.Message);
                    break;
                default:
                    Users.AddOrUpdate(chatMessage.Source, new ChatUser(chatMessage.Source, chatMessage.ChannelName),
                        (key, oldValue) => oldValue);
                    Users[chatMessage.Source].ProcessChatMessage(chatMessage);
                    break;
            }
        }

        private void ProcessTmiMessage(ChatMessage chatMessage)
        {
            switch (chatMessage.Command)
            {
                case "ROOMSTATE":
                    SetRoomState(chatMessage.Tags);
                    break;
                case "CLEARCHAT":
                    ClearChatReceived(chatMessage.Tags, chatMessage.Message);
                    break;
                case "NOTICE":
                    break;
            }
        }

        private void ClearChatReceived(string tags, string message)
        {
            if (!String.IsNullOrEmpty(message) && Users.ContainsKey(message))
            {
                int banDuration = -1;
                string banReason = null;

                // Ban message looks like the following:
                // ban-duration=600;ban-reason=
                // ban-duration will not be present when the user is permanently banned.
                string[] splitTags = tags.Split(';');

                for(int a = 0; a < splitTags.Length; a++)
                {
                    string[] tagParts = splitTags[a].Split('=');

                    switch(tagParts[0])
                    {
                        case "ban-duration":
                            banDuration = Convert.ToInt32(tagParts[1]);
                            break;
                        case "ban-reason":
                            banReason = String.IsNullOrEmpty(tagParts[1]) ? "" : tagParts[1];
                            break;
                    }
                }

                Users[message].BanUser(banDuration, banReason);
            }
        }

        private void SetUserMode(string user, string mode)
        {
            if (Users.ContainsKey(user))
            {
                Users[user].IsModerator = String.Equals(mode, "+o", StringComparison.OrdinalIgnoreCase);
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
                        BroadcasterLanguage = state[1];
                        break;
                    case "r9k":
                        R9KModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "slow":
                        SlowModeInterval = Convert.ToInt32(state[1]);
                        SlowModeEnabled = (SlowModeInterval > 0);
                        break;
                    case "subs-only":
                        SubscribersOnlyModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "emote-only":
                        EmoteOnlyModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                }
            }
        }
    }
}
