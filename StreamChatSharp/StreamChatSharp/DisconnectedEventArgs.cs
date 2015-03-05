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
