using System;
using System.IO;
using System.Threading;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Threaded message sender used to send messages on a stream.
    /// </summary>
    class ChatMessageSender : IDisposable
    {
        /// <summary>
        /// Triggered whenever the sender stops being able to send messages. The sender will have to be restarted via
        /// the Start method.
        /// </summary>
        public event EventHandler ConnectionLost;

        private OutgoingMessageQueue outgoingMessageQueue = new OutgoingMessageQueue();
        private StreamWriter writer;
        private Thread thread;

        private bool disposed = false;
        private bool running;
        private bool connected;

        public ChatMessageSender()
        {
            this.outgoingMessageQueue.MessageReady += OnOutgoingMessageReady;
        }

        /// <summary>
        /// Disposes of everything.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Writes a raw message to the stream.
        /// </summary>
        /// <param name="chatMessage">The message to write to the stream.</param>
        /// /// <param name="highPriorityMessage">Whether or not the message is a high priority message.</param>
        public void SendMessage(ChatMessage chatMessage, bool highPriorityMessage)
        {
            if (!string.IsNullOrWhiteSpace(chatMessage.Command))
            {
                this.outgoingMessageQueue.AddMessage(chatMessage, highPriorityMessage);
            }
        }

        /// <summary>
        /// Starts the message sender to be able to send messages to a stream.
        /// </summary>
        /// <param name="networkStream">The stream to send messages to.</param>
        public void Start(Stream networkStream)
        {
            this.writer = new StreamWriter(networkStream)
            {
                AutoFlush = true
            };

            this.thread = new Thread(Run);
            this.thread.Start();

            this.running = true;
        }

        /// <summary>
        /// Stops the message sender from sending messsages to the stream.
        /// </summary>
        public void Stop()
        {
            this.writer.Dispose();
            this.running = false;
        }

        /// <summary>
        /// The amount of time (in milliseconds) to wait between sent messages. By default this is set to 1.6 seconds 
        /// between messages (20 messages in 32 seconds).
        /// </summary>
        public double MessageSendInterval
        {
            get
            {
                return this.outgoingMessageQueue.MessageSendInterval;
            }
            set
            {
                this.outgoingMessageQueue.MessageSendInterval = value;
            }
        }

        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    Stop();
                    this.outgoingMessageQueue.Dispose();
                }

                this.disposed = true;
            }
        }

        private void Run()
        {
            // The thread "runs" here but the messages are sent via the SendMessage method above.
        }

        private void OnOutgoingMessageReady(object sender, ChatMessageEventArgs e)
        {
            string rawMessage = RawMessageParser.ChatMessageToRawMessage(e.ChatMessage);

            if (!string.IsNullOrWhiteSpace(rawMessage) && this.running)
            {
                try
                {
                    this.writer.WriteLine(rawMessage);
                    this.connected = true;
                }
                catch (ObjectDisposedException)
                {

                    // We probably lost connection to the server. Wait 5 seconds and try again. If the next try fails,
                    // stop trying and alert that the connection has been lost. If the next try succeeds everything 
                    // will continue on as normal.
                    if (this.connected)
                    {
                        this.connected = false;
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        Stop();

                        if (ConnectionLost != null)
                        {
                            ConnectionLost(this, new EventArgs());
                        }
                    }
                }
            }
        }
    }
}
