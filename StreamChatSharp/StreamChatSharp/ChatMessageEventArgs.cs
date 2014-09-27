using System;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Provides data for chat message events such as when a chat message is received.
    /// </summary>
    public class ChatMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Chat message.
        /// </summary>
        public ChatMessage ChatMessage { get; set; }
    }
}
