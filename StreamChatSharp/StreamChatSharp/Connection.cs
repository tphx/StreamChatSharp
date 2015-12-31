// The MIT License (MIT)
//
// Copyright (c) 2014 TPHX
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
// the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
// SOFTWARE.

using System;
using System.Net.Sockets;
using System.Timers;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Manages a conection to an IRC server.
    /// </summary>
    [System.Runtime.InteropServices.GuidAttribute("F23F4306-DABE-41E5-B02E-267908C228B0")]
    public class Connection : IDisposable
    {        
        /// <summary>
        /// Triggered whenever a raw IRC message is received from the server.
        /// </summary>
        public event EventHandler<RawMessageEventArgs> RawMessageReceived;

        /// <summary>
        /// Triggered whenever a chat message is received.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> ChatMessageReceived;

        /// <summary>
        /// Triggered whenever the connection is disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Triggered when the connection has successfully registered with the server.
        /// </summary>
        public event EventHandler RegisteredWithServer;

        /// <summary>
        /// Triggered whenever a chat message is sent.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> ChatMessageSent;

        // Amount of time to wait before the connection is timed out when first connecting to the server.
        private const double newConnectionTimeoutInterval = 60000.00; // 1 minute.

        // Amount of time to wait since the last message has been received from the server before the connection is
        // timed out. 
        private const double noMessageReceivedTimeoutInterval = 300000.00; // 5 minutes.

        // The amount of time to wait before the a ping reply is received before the connection is timed out.
        private const double pingTimeoutInterval = 30000.00; // 30 seconds.

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private ConnectionData connectionData;
        private ChatMessageSender messageSender = new ChatMessageSender();
        private ChatMessageReceiver messageReceiver = new ChatMessageReceiver();
        private System.Timers.Timer timeoutTimer = new System.Timers.Timer();
        private bool connectionRegistered;
        private bool disposed = false;

        /// <summary>
        /// Connects to an IRC server.
        /// </summary>
        public Connection()
        {
            this.messageSender.ConnectionLost += OnConnectionLost;

            this.messageReceiver.ConnectionLost += OnConnectionLost;

            this.messageReceiver.RawMessageReceived += OnRawMessageReceived;
            this.messageReceiver.ChatMessageReceived += OnChatMessageReceived;
            this.timeoutTimer.Elapsed += OnTimeoutTimerElapsed;
        }


        /// <summary>
        /// Disposes of eveything.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disconnects from the IRC server.
        /// </summary>
        public void Disconnect()
        {
            Disconnect(DisconnectedEventArgs.DisconnectReason.ClientDisconnected, false);
        }

        /// <summary>
        /// Whether or not the client is currently connected to the IRC server.
        /// </summary>
        public bool Connected
        {
            get
            {
                return (this.tcpClient != null && this.tcpClient.Connected);
            }
        }

        /// <summary>
        /// Sends a raw IRC message to the server.
        /// </summary>
        /// <param name="chatMessage">The raw message to send to the server.</param>
        /// /// <param name="highPriorityMessage">Whether or not the message is a high priority message.</param>
        public void SendChatMessage(ChatMessage chatMessage, bool highPriorityMessage)
        {
            this.messageSender.SendMessage(chatMessage, highPriorityMessage);

            if(this.ChatMessageSent != null)
            {
                chatMessage.Source = connectionData.Nickname;
                this.ChatMessageSent(this, new ChatMessageEventArgs(chatMessage));
            }
        }

        /// <summary>
        /// The amount of time (in milliseconds) to wait between sent messages. By default this is set to 1.6 seconds 
        /// between messages (20 messages in 32 seconds).
        /// </summary>
        public double MessageSendInterval
        {
            get
            {
                return this.messageSender.MessageSendInterval;
            }
            set
            {
                this.messageSender.MessageSendInterval = value;
            }
        }

        /// <summary>
        /// Server connection data.
        /// </summary>
        public ConnectionData ConnectionData
        {
            get
            {
                return connectionData;
            }
        }

        /// <summary>
        /// Whether or not the connection has been registered with the server.
        /// </summary>
        public bool ConnectionRegistered
        {
            get
            {
                return this.connectionRegistered;
            }
        }

        /// <summary>
        /// Connects to an IRC server.
        /// </summary>
        /// <param name="serverConnectionData">Data to connect with.</param>
        public void ConnectToServer(ConnectionData serverConnectionData)
        {
            if(serverConnectionData == null)
            {
                throw new ArgumentNullException("serverConnectionData");
            }

            if (this.tcpClient == null || !this.tcpClient.Connected)
            {
                this.connectionData = serverConnectionData;

                try
                {
                    this.tcpClient = new TcpClient(this.connectionData.ServerAddress,
                        this.connectionData.Port);
                }
                catch (SocketException)
                {
                    Disconnect(DisconnectedEventArgs.DisconnectReason.HostNotFound, false);
                    return;
                }

                this.networkStream = this.tcpClient.GetStream();
                this.messageSender.Start(networkStream);
                this.messageReceiver.Start(networkStream);
                this.connectionRegistered = false;

                this.timeoutTimer.Interval = newConnectionTimeoutInterval;
                this.timeoutTimer.Start();

                SendChatMessage(new ChatMessage("PASS", this.connectionData.Password), true);
                SendChatMessage(new ChatMessage("NICK", this.connectionData.Nickname), true);
            }
        }
        
        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    this.Disconnect(DisconnectedEventArgs.DisconnectReason.Disposed, false);
                    this.messageSender.Dispose();
                    this.messageReceiver.Dispose();
                    this.timeoutTimer.Dispose();
                }

                this.disposed = true;
            }
        }

        private void OnRawMessageReceived(object sender, RawMessageEventArgs e)
        {
            // We could also set the interval in OnChatMessageReceived, but it is called at the same time this is so 
            // we only need to set it once here.
            this.timeoutTimer.Interval = noMessageReceivedTimeoutInterval;

            if (this.RawMessageReceived != null)
            {
                this.RawMessageReceived(sender, e);
            }
        }

        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if (String.Equals(e.ChatMessage.Command, "PING", StringComparison.OrdinalIgnoreCase))
            {
                SendChatMessage(new ChatMessage("RAW", "PONG"), true);
            }
            // 001 is the first command Twitch sends on a successful connection.
            if (String.Equals(e.ChatMessage.Command, "001", StringComparison.OrdinalIgnoreCase))
            {
                RegistrationMessageReceived();
            }

            if (this.ChatMessageReceived != null)
            {
                this.ChatMessageReceived(sender, e);
            }
        }

        private void OnTimeoutTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // If the timeout interval is set to noMessageReceivedTimeoutInterval we should have a valid connection 
            // that hasn't received a message for some time. It may have been lost or it may be unusually quiet. A 
            // ping should tell us whether or not we're still connected.
            if (this.timeoutTimer.Interval == noMessageReceivedTimeoutInterval)
            {
                Ping();
            }
            else
            {
                ConnectionTimedOut();
            }
        }

        private void Disconnect(DisconnectedEventArgs.DisconnectReason reason, bool attemptingAutoReconnect)
        {
            SendChatMessage(new ChatMessage("RAW", "QUIT"), true);

            this.timeoutTimer.Stop();
            this.messageReceiver.Stop();
            this.messageSender.Stop();
            this.connectionRegistered = false;

            if (tcpClient != null)
            {
                this.tcpClient.Close();
            }

            if (networkStream != null)
            {
                this.networkStream.Dispose();
            }

            if (this.Disconnected != null)
            {
                this.Disconnected(this, new DisconnectedEventArgs(reason, attemptingAutoReconnect));
            }
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            ConnectionTimedOut();
        }

        private void ConnectionTimedOut()
        {
            Disconnect(DisconnectedEventArgs.DisconnectReason.TimedOut, true);
            ConnectToServer(this.connectionData);
        }

        private void Ping()
        {
            SendChatMessage(new ChatMessage("RAW", "PING"), true);

            this.timeoutTimer.Interval = pingTimeoutInterval;
        }

        private void RegistrationMessageReceived()
        {
            this.connectionRegistered = true;

            if(RegisteredWithServer != null)
            {
                RegisteredWithServer(this, new EventArgs());
            }
        }
    }
}
