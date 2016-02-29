using System;
using System.Collections.Concurrent;
using System.Timers;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Queues messages that are destined for the chat and provides flood control.
    /// </summary>
    class OutgoingMessageQueue : IDisposable
    {
        /// <summary>
        /// Triggered whenever a message is ready to be sent to the chat.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> MessageReady;

        private ConcurrentQueue<ChatMessage> highPriorityMessages = new ConcurrentQueue<ChatMessage>();
        private ConcurrentQueue<ChatMessage> normalPriorityMessages = new ConcurrentQueue<ChatMessage>();
        private Timer sendMessageTimer = new Timer(1600); // 1.6 seconds.
        private bool stoppedManually = false;
        private bool messageSentLastCycle = false;
        private bool disposed = false;

        public OutgoingMessageQueue()
        {
            this.sendMessageTimer.Elapsed += OnSendMessageTimerElapsed;
            this.sendMessageTimer.Start();
        }

        /// <summary>
        /// Disposes of everything.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Queues an outgoing ChatMessage to be sent to the chat and starts the send timer unless the queue has been
        /// stopped manually via the Stop method.
        /// </summary>
        /// <param name="message">The chat message to be queued.</param>
        /// <param name="isHighPriorityMessage">Whether or not the message is a high priority message.</param>
        public void AddMessage(ChatMessage message, bool isHighPriorityMessage)
        {
            // If the send message timer isn't running (no message has been sent in since the last interval) we can 
            // send the message instantly.
            if (!this.messageSentLastCycle)
            {
                SendMessage(message);
                this.messageSentLastCycle = true;
            }
            else
            {
                if (isHighPriorityMessage)
                {
                    this.highPriorityMessages.Enqueue(message);
                }
                else
                {
                    this.normalPriorityMessages.Enqueue(message);
                }
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
                return this.sendMessageTimer.Interval;
            }
            set
            {
                this.sendMessageTimer.Interval = value;
            }
        }

        /// <summary>
        /// Manually starts the outgoing message queue.
        /// </summary>
        public void Start()
        {
            this.sendMessageTimer.Start();
            this.stoppedManually = false;
        }

        /// <summary>
        /// Manually stops the outgoing message queue. Once the queue has been stopped manually, it can only be 
        /// restarted manually.
        /// </summary>
        public void Stop()
        {
            this.sendMessageTimer.Stop();
            this.stoppedManually = true;
        }

        /// <summary>
        /// Whether or not the outgoing message queue was stopped manually.
        /// </summary>
        public bool StoppedManually
        {
            get
            {
                return this.stoppedManually;
            }
        }

        /// <summary>
        /// Clears all of the messages in queue.
        /// </summary>
        public void ClearMessages()
        {
            this.highPriorityMessages = new ConcurrentQueue<ChatMessage>();
            this.normalPriorityMessages = new ConcurrentQueue<ChatMessage>();
        }

        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    this.sendMessageTimer.Dispose();
                }

                this.disposed = true;
            }
        }

        private void OnSendMessageTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ChatMessage message;

            if(this.highPriorityMessages.TryDequeue(out message))
            {
                SendMessage(message);
                this.messageSentLastCycle = true;
            }
            else if(this.normalPriorityMessages.TryDequeue(out message))
            {
                SendMessage(message);
                this.messageSentLastCycle = true;
            }
            else
            {
                this.messageSentLastCycle = false;
            }
        }

        private void SendMessage(ChatMessage message)
        {
            if(this.MessageReady != null)
            {
                this.MessageReady(this, new ChatMessageEventArgs(message));
            }
        }
    }
}
