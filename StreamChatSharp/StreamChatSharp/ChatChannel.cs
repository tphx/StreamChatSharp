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
using System.Text;
using System.Threading.Tasks;

namespace Tphx.StreamChatSharp
{
   /// <summary>
   /// Data for a chat channel.
   /// </summary>
    public class ChatChannel
    {
        private ConcurrentDictionary<string, ChatUser> users = new ConcurrentDictionary<string, ChatUser>();
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
        public ReadOnlyCollection<ChatUser> ChatUsers
        {
            get
            {
                return new ReadOnlyCollection<ChatUser>(this.users.Values.ToList());
            }
        }

        /// <summary>
        /// Adds a user to the list of users in the channel.
        /// </summary>
        /// <param name="userName">Name of the user to add.</param>
        public void AddChatUser(string userName)
        {
            this.users.AddOrUpdate(userName, new ChatUser(userName), ((key, oldValue) => oldValue));

            // The '#' needs to be removed from the beginning of the channel name before checking to see if it's the
            // same as the username.
            if(string.Equals(userName, this.ChannelName.Remove(0, 1), StringComparison.OrdinalIgnoreCase))
            {
                this.users[userName].IsChannelOwner = true;
            }
        }

        /// <summary>
        /// Removes a user from the list of users in the channel.
        /// </summary>
        /// <param name="userName">Name of the user to remove.</param>
        public void RemoveChatUser(string userName)
        {
            ChatUser userToRemove;
            this.users.TryRemove(userName, out userToRemove);
        }

        /// <summary>
        /// Sets the special user type for a user in the channel.
        /// </summary>
        /// <param name="userName">Name of the user to toggle the special user type for.</param>
        /// <param name="specialUserType">Special user type to set.</param>
        /// <param name="enabled">Whether or not the special user type is enabled.</param>
        public void SetSpecialUserType(string userName, ChatUser.SpecialUserType specialUserType, bool enabled)
        {
            if(this.users.ContainsKey(userName))
            {
                this.users[userName].SetSpecialUserType(specialUserType, enabled);
            }
        }

        /// <summary>
        /// Whether or not the channel is in subscribers only mode.
        /// </summary>
        public bool SubscribersOnlyModeEnabled { get; private set; }

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
        public int SlowModeInterval { get; private set; }

        /// <summary>
        /// Whether or not R9K mode is enabled in the channel.
        /// </summary>
        public bool R9KModeEnabled { get; private set; }

        /// <summary>
        /// Language spoken by the broadcaster.
        /// </summary>
        public string BroadcasterLanguage
        {
            get
            {
                return this.broadcasterLanguage ?? "English";
            }
        }

        /// <summary>
        /// Sets the room states for the channel.
        /// </summary>
        /// <param name="roomStateMessage"></param>
        internal void SetRoomState(ChatMessage roomStateMessage)
        {
                // The rooms state can contain one state or various states delimited by semicolons.
                // broadcaster-lang=;r9k=0;slow=0;subs-only=0 
                string[] states = roomStateMessage.Tags.Split(';');

                for(int a = 0; a < states.Length; a++)
                {
                    string[] state = states[a].Split('=');

                    switch(state[0])
                    {
                        case "broadcaster-lang":
                            // If it's english, the language will be blank.
                            this.broadcasterLanguage = string.IsNullOrEmpty(state[1]) ? "English" : 
                                state[1];
                            break;
                        case "r9k":
                            this.R9KModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                            break;
                        case "slow":
                            this.SlowModeInterval = Convert.ToInt32(state[1].ToString());
                            break;
                        case "subs-only":
                            this.SubscribersOnlyModeEnabled = Convert.ToBoolean(Convert.ToInt32(state[1]));
                            break;
                    }
                }
            
        }
    }
}
