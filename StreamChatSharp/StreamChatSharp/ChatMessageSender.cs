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

        /// <summary>
        /// Triggered whenever a chat message is sent.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> ChatMessageSent;

        private OutgoingMessageQueue outgoingMessageQueue = new OutgoingMessageQueue();
        private StreamWriter writer;
        private Thread thread;
        private bool running;
        private bool connected;
        private bool disposed = false;

        /// <summary>
        /// Constructs a new chat message sender.
        /// </summary>
        public ChatMessageSender()
        {
            outgoingMessageQueue.MessageReady += OnOutgoingMessageReady;
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
            if (running && !String.IsNullOrWhiteSpace(chatMessage.Command))
            {
                outgoingMessageQueue.AddMessage(chatMessage, highPriorityMessage);
            }
        }

        /// <summary>
        /// Starts the message sender to be able to send messages to a stream.
        /// </summary>
        /// <param name="networkStream">The stream to send messages to.</param>
        public void Start(Stream networkStream)
        {
            writer = new StreamWriter(networkStream)
            {
                AutoFlush = true
            };

            thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();

            running = true;
        }

        /// <summary>
        /// Stops the message sender from sending messsages to the stream.
        /// </summary>
        public void Stop()
        {
            outgoingMessageQueue.ClearMessages();

            if (writer != null)
            {
                writer.Dispose();
            }

            running = false;
        }

        /// <summary>
        /// The amount of time (in milliseconds) to wait between sent messages. By default this is set to 1.6 seconds 
        /// between messages (20 messages in 32 seconds).
        /// </summary>
        public double MessageSendInterval
        {
            get
            {
                return outgoingMessageQueue.MessageSendInterval;
            }
            set
            {
                outgoingMessageQueue.MessageSendInterval = value;
            }
        }

        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    Stop();
                    outgoingMessageQueue.Dispose();
                }

                disposed = true;
            }
        }

        private void Run()
        {
            // The thread "runs" here but the messages are sent via the SendMessage method above.
        }

        private void OnOutgoingMessageReady(object sender, ChatMessageEventArgs e)
        {
            string rawMessage = RawMessageParser.ChatMessageToRawMessage(e.ChatMessage);

            if (!String.IsNullOrWhiteSpace(rawMessage) && running)
            {
                if (ChatMessageSent != null)
                {
                    ChatMessageSent(this, new ChatMessageEventArgs(e.ChatMessage));
                }

                try
                {
                    writer.WriteLine(rawMessage);
                    connected = true;

                    // If the connection was lost at some point we need to restart it now that we're working again.
                    if(outgoingMessageQueue.StoppedManually)
                    {
                        outgoingMessageQueue.Start();
                    }
                }
                catch (IOException)
                {
                    // We probably lost connection to the server. Wait 5 seconds and try again. If the next try fails,
                    // stop trying and alert that the connection has been lost. If the next try succeeds everything 
                    // will continue on as normal.
                    if (connected)
                    {
                        outgoingMessageQueue.Stop(); // We don't want any other messages firing while we wait.
                        connected = false;
                        Thread.Sleep(5000);
                        OnOutgoingMessageReady(sender, e);
                    }
                    else
                    {
                        AlertConnectionLost();
                    }
                }
                catch(ObjectDisposedException)
                {
                    AlertConnectionLost();
                }
            }
        }

        private void AlertConnectionLost()
        {
            Stop();

            if (ConnectionLost != null)
            {
                ConnectionLost(this, new EventArgs());
            }
        }
    }
}
