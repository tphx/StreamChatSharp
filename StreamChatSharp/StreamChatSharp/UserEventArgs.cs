using System;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Provides data for user events such as when a user leaves or joins a channel.
    /// </summary>
    public class UserEventArgs : EventArgs
    {
        /// <summary>
        /// Defines the arguments for a user event.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public UserEventArgs(string userName)
        {
            UserName = userName;
        }
        
        /// <summary>
        /// Name of a chat user.
        /// </summary>
        public string UserName { get; set; }
    }
}
