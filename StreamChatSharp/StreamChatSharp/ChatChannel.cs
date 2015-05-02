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
        /// Toggles the special user type for a user in the channel.
        /// </summary>
        /// <param name="userName">Name of the user to toggle the special user type for.</param>
        /// <param name="specialUserType">Special user type to toggle.</param>
        /// <param name="enabled">Whether or not the special user type is enabled.</param>
        public void ToggleSpecialUserType(string userName, ChatUser.SpecialUserType specialUserType, bool enabled)
        {
            if(this.users.ContainsKey(userName))
            {
                this.users[userName].ToggleSpecialUserType(specialUserType, enabled);
            }
        }
    }
}
