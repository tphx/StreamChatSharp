using System;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Provides data for user events such as when a user leaves or joins a channel.
    /// </summary>
    public class UserEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the name of a chat user.
        /// </summary>
        public string UserName { get; set; }
    }
}
