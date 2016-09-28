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
            sendMessageTimer.Elapsed += OnSendMessageTimerElapsed;
            sendMessageTimer.Start();
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
            if (!messageSentLastCycle)
            {
                SendMessage(message);
                messageSentLastCycle = true;
            }
            else
            {
                if (isHighPriorityMessage)
                {
                    highPriorityMessages.Enqueue(message);
                }
                else
                {
                    normalPriorityMessages.Enqueue(message);
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
                return sendMessageTimer.Interval;
            }
            set
            {
                sendMessageTimer.Interval = value;
            }
        }

        /// <summary>
        /// Manually starts the outgoing message queue.
        /// </summary>
        public void Start()
        {
            sendMessageTimer.Start();
            stoppedManually = false;
        }

        /// <summary>
        /// Manually stops the outgoing message queue. Once the queue has been stopped manually, it can only be 
        /// restarted manually.
        /// </summary>
        public void Stop()
        {
            sendMessageTimer.Stop();
            stoppedManually = true;
        }

        /// <summary>
        /// Whether or not the outgoing message queue was stopped manually.
        /// </summary>
        public bool StoppedManually
        {
            get
            {
                return stoppedManually;
            }
        }

        /// <summary>
        /// Clears all of the messages in queue.
        /// </summary>
        public void ClearMessages()
        {
            highPriorityMessages = new ConcurrentQueue<ChatMessage>();
            normalPriorityMessages = new ConcurrentQueue<ChatMessage>();
        }

        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    sendMessageTimer.Dispose();
                }

                disposed = true;
            }
        }

        private void OnSendMessageTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ChatMessage message;

            if(highPriorityMessages.TryDequeue(out message))
            {
                SendMessage(message);
                messageSentLastCycle = true;
            }
            else if(normalPriorityMessages.TryDequeue(out message))
            {
                SendMessage(message);
                messageSentLastCycle = true;
            }
            else
            {
                messageSentLastCycle = false;
            }
        }

        private void SendMessage(ChatMessage message)
        {
            if(MessageReady != null)
            {
                MessageReady(this, new ChatMessageEventArgs(message));
            }
        }
    }
}
