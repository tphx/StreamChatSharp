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
            messageSender.ConnectionLost += OnConnectionLost;
            messageSender.ChatMessageSent += OnChatMessageSent;

            messageReceiver.ConnectionLost += OnConnectionLost;

            messageReceiver.RawMessageReceived += OnRawMessageReceived;
            messageReceiver.ChatMessageReceived += OnChatMessageReceived;
            timeoutTimer.Elapsed += OnTimeoutTimerElapsed;
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
                return (tcpClient != null && tcpClient.Connected);
            }
        }

        /// <summary>
        /// Sends a raw IRC message to the server.
        /// </summary>
        /// <param name="chatMessage">The raw message to send to the server.</param>
        /// /// <param name="highPriorityMessage">Whether or not the message is a high priority message.</param>
        public void SendChatMessage(ChatMessage chatMessage, bool highPriorityMessage)
        {
            messageSender.SendMessage(chatMessage, highPriorityMessage);
        }

        /// <summary>
        /// The amount of time (in milliseconds) to wait between sent messages. By default this is set to 1.6 seconds 
        /// between messages (20 messages in 32 seconds).
        /// </summary>
        public double MessageSendInterval
        {
            get
            {
                return messageSender.MessageSendInterval;
            }
            set
            {
                messageSender.MessageSendInterval = value;
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
                return connectionRegistered;
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

            if (tcpClient == null || !tcpClient.Connected)
            {
                connectionData = serverConnectionData;

                try
                {
                    tcpClient = new TcpClient(connectionData.ServerAddress,
                        connectionData.Port);
                }
                catch (SocketException)
                {
                    Disconnect(DisconnectedEventArgs.DisconnectReason.HostNotFound, false);
                    return;
                }

                networkStream = tcpClient.GetStream();
                messageSender.Start(networkStream);
                messageReceiver.Start(networkStream);
                connectionRegistered = false;

                timeoutTimer.Interval = newConnectionTimeoutInterval;
                timeoutTimer.Start();

                SendChatMessage(new ChatMessage("PASS", connectionData.Password), true);
                SendChatMessage(new ChatMessage("NICK", connectionData.Nickname), true);
            }
        }
        
        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    Disconnect(DisconnectedEventArgs.DisconnectReason.Disposed, false);
                    messageSender.Dispose();
                    messageReceiver.Dispose();
                    timeoutTimer.Dispose();
                }

                disposed = true;
            }
        }

        private void OnRawMessageReceived(object sender, RawMessageEventArgs e)
        {
            // We could also set the interval in OnChatMessageReceived, but it is called at the same time this is so 
            // we only need to set it once here.
            timeoutTimer.Interval = noMessageReceivedTimeoutInterval;

            if (RawMessageReceived != null)
            {
                RawMessageReceived(sender, e);
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

            if (ChatMessageReceived != null)
            {
                ChatMessageReceived(sender, e);
            }
        }

        private void OnTimeoutTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // If the timeout interval is set to noMessageReceivedTimeoutInterval we should have a valid connection 
            // that hasn't received a message for some time. It may have been lost or it may be unusually quiet. A 
            // ping should tell us whether or not we're still connected.
            if (timeoutTimer.Interval == noMessageReceivedTimeoutInterval)
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

            timeoutTimer.Stop();
            messageReceiver.Stop();
            messageSender.Stop();
            connectionRegistered = false;

            if (tcpClient != null)
            {
                tcpClient.Close();
            }

            if (networkStream != null)
            {
                networkStream.Dispose();
            }

            if (Disconnected != null)
            {
                Disconnected(this, new DisconnectedEventArgs(reason, attemptingAutoReconnect));
            }
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            ConnectionTimedOut();
        }

        private void ConnectionTimedOut()
        {
            Disconnect(DisconnectedEventArgs.DisconnectReason.TimedOut, true);
            ConnectToServer(connectionData);
        }

        private void Ping()
        {
            SendChatMessage(new ChatMessage("RAW", "PING"), true);

            timeoutTimer.Interval = pingTimeoutInterval;
        }

        private void RegistrationMessageReceived()
        {
            connectionRegistered = true;

            if(RegisteredWithServer != null)
            {
                RegisteredWithServer(this, new EventArgs());
            }
        }

        private void OnChatMessageSent(object sender, ChatMessageEventArgs e)
        {
            if (ChatMessageSent != null)
            {
                e.ChatMessage.Source = ConnectionData.Nickname;
                ChatMessageSent(this, e);
            }
        }
    }
}
