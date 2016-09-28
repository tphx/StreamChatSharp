using System;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Provides data for raw message events such as when a raw message is received.
    /// </summary>
    public class RawMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Defines the arguments for a raw message event.
        /// </summary>
        /// <param name="rawMessage">Raw message.</param>
        public RawMessageEventArgs(string rawMessage)
        {
            RawMessage = rawMessage;
        }

        /// <summary>
        /// Raw IRC message.
        /// </summary>
        public string RawMessage { get; set; }
    }
}
