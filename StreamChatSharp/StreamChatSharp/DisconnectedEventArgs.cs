using System;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Data for when the the client disconnects from the server.
    /// </summary>
    public class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Reasons a connection was disconnected.
        /// </summary>
        public enum DisconnectReason
        {
            /// <summary>
            /// The client has voluntarily disonnected.
            /// </summary>
            ClientDisconnected,
            /// <summary>
            /// The connection timed out.
            /// </summary>
            TimedOut,
            /// <summary>
            /// The connection has been disposed.
            /// </summary>
            Disposed
        }
        
        /// <summary>
        /// The reason the disconnect occured.
        /// </summary>
        public DisconnectReason Reason { get; set; }
    }
}
