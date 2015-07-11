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

namespace Tphx.StreamChatSharp
{
   /// <summary>
   /// Data for a chat channel.
   /// </summary>
    public class ChatChannel
    {
        private ConcurrentDictionary<string, ChatUser> chatUsers = new ConcurrentDictionary<string, ChatUser>();
        private string broadcasterLanguage;

        /// <summary>
        /// Creates a new chat channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        public ChatChannel(string channelName)
        {
            this.ChannelName = channelName;
        }

        /// <summary>
        /// The name of the channel.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Users that are currently in the channel.
        /// </summary>
        public ConcurrentDictionary<string, ChatUser> ChatUsers
        {
            get
            {
                return this.chatUsers;
            }
        }

        /// <summary>
        /// Adds a user to the list of users in the channel.
        /// </summary>
        /// <param name="userName">Name of the user to add.</param>
        public void AddChatUser(string userName)
        {
            this.chatUsers.AddOrUpdate(userName, new ChatUser(userName), ((key, oldValue) => oldValue));

            // The '#' needs to be removed from the beginning of the channel name before checking to see if it's the
            // same as the username.
            if(String.Equals(userName, this.ChannelName.Remove(0, 1), StringComparison.OrdinalIgnoreCase))
            {
                this.chatUsers[userName].IsChannelOwner = true;
                this.chatUsers[userName].IsModerator = true;
            }
        }

        /// <summary>
        /// Removes a user from the list of users in the channel.
        /// </summary>
        /// <param name="userName">Name of the user to remove.</param>
        public void RemoveChatUser(string userName)
        {
            ChatUser userToRemove;
            this.chatUsers.TryRemove(userName, out userToRemove);
        }

        /// <summary>
        /// Language spoken by the broadcaster.
        /// </summary>
        public string BroadcasterLanguage
        {
            get
            {
                return this.broadcasterLanguage ?? "English";
            }

            set
            {
                this.broadcasterLanguage = value;
            }
        }

        /// <summary>
        /// Whether or not R9K mode is enabled in the channel.
        /// </summary>
        public bool R9KModeEnabled { get; set; }

        /// <summary>
        /// Whether or not slow mode is enabled on the channel.
        /// </summary>
        public bool SlowModeEnabled
        {
            get
            {
                return (this.SlowModeInterval > 0);
            }
        }

        /// <summary>
        /// Interval (seconds) a user must wait beteen sending messages.
        /// </summary>
        public int SlowModeInterval { get; set; }

        /// <summary>
        /// Whether or not the channel is in subscribers only mode.
        /// </summary>
        public bool SubscribersOnlyModeEnabled { get; set; }

        /// <summary>
        /// Sets the user state based on the chat message.
        /// </summary>
        /// <param name="userStateMessage">Message containing the user states. </param>
        internal void SetUserState(ChatMessage userStateMessage)
        {
            if (String.Equals(userStateMessage.Command, "PART", StringComparison.OrdinalIgnoreCase))
            {
                RemoveChatUser(userStateMessage.Source);
                return;
            }
            // MODE is sent after the user has already parted so we need to check to see if the user is still in the
            // channel before changing their user state.
            else if (String.Equals(userStateMessage.Command, "MODE", StringComparison.OrdinalIgnoreCase) && 
                this.chatUsers.ContainsKey(userStateMessage.Source))
            {
                this.chatUsers[userStateMessage.Source].SetUserState(userStateMessage);
                return;
            }
            else if (!this.chatUsers.ContainsKey(userStateMessage.Source))
            {
                AddChatUser(userStateMessage.Source);
            }

            this.chatUsers[userStateMessage.Source].SetUserState(userStateMessage);
        }

        /// <summary>
        /// Sets the room states for the channel.
        /// </summary>
        /// <param name="roomStateMessage">Message containing the room states.</param>
        internal void SetRoomState(ChatMessage roomStateMessage)
        {
            // The room state can contain one state or various states delimited by semicolons.
            // broadcaster-lang=;r9k=0;slow=0;subs-only=0 
            string[] states = roomStateMessage.Tags.Split(';');

            for(int a = 0; a < states.Length; a++)
            {
                string[] state = states[a].Split('=');

                switch(state[0])
                {
                    case "broadcaster-lang":
                        // If it's english, the language will be blank.
                        this.BroadcasterLanguage = String.IsNullOrEmpty(state[1]) ? "English" : state[1];
                        break;
                    case "r9k":
                        this.R9KModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "slow":
                        this.SlowModeInterval = Convert.ToInt32(state[1]);
                        break;
                    case "subs-only":
                        this.SubscribersOnlyModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                }
            }
        }
    }
}
