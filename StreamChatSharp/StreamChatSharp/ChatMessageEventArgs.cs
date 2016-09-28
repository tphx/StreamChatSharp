using System;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Provides data for chat message events such as when a chat message is received.
    /// </summary>
    public class ChatMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Defines the arguments for a chat message event.
        /// </summary>
        /// <param name="chatMessage">ChatMessage.</param>
        public ChatMessageEventArgs(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }

        /// <summary>
        /// Chat message.
        /// </summary>
        public ChatMessage ChatMessage { get; set; }
    }
}
