using System;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Provides data for raw message events such as when a raw message is received.
    /// </summary>
    class RawMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Raw IRC message.
        /// </summary>
        public string RawMessage { get; set; }
    }
}
