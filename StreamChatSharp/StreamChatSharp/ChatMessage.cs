namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Contains the information for a chat message.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// The sender of the message.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The target of a command.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The channel the message is being sent to or received from.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// The IRC command being issued.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The message that was sent.
        /// </summary>
        public string Message { get; set; }
    }
}
