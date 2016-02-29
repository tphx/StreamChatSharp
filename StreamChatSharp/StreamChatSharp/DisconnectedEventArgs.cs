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
            Disposed,
            /// <summary>
            /// The host being connected to could not be found.
            /// </summary>
            HostNotFound
        }

        /// <summary>
        /// Defines the arguments for a disconnected event.
        /// </summary>
        /// <param name="disconnectReason">Reason for the disconnect.</param>
        public DisconnectedEventArgs(DisconnectReason disconnectReason)
        {
            this.Reason = disconnectReason;
        }

        /// <summary>
        /// Defines the arguments for a disconnected event.
        /// </summary>
        /// <param name="disconnectReason">Reason for the disconnect</param>
        /// <param name="attemptingAutoReconnect">Whether or not the connection is going to try to automatically
        /// reconnect.</param>
        public DisconnectedEventArgs(DisconnectReason disconnectReason, bool attemptingAutoReconnect)
            : this(disconnectReason)
        {
            this.AttemptingAutoReconnect = attemptingAutoReconnect;
        }
        
        /// <summary>
        /// The reason the disconnect occured.
        /// </summary>
        public DisconnectReason Reason { get; set; }

        /// <summary>
        /// Whether or not the connection is going to try to automatically reconnect to the server.
        /// </summary>
        public bool AttemptingAutoReconnect { get; set; }
    }
}
