using System;
using System.Collections.Generic;
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

        private Queue<ChatMessage> highPriorityMessages = new Queue<ChatMessage>();
        private Queue<ChatMessage> normalPriorityMessages = new Queue<ChatMessage>();

        private Timer sendMessageTimer = new Timer(1600); // 1.6 seconds.

        private bool disposed = false;

        public OutgoingMessageQueue()
        {
            this.sendMessageTimer.Elapsed += OnSendMessageTimerElapsed;
        }

        /// <summary>
        /// Disposes of everything.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Queues an outgoing ChatMessage to be sent to the chat.
        /// </summary>
        /// <param name="message">The chat message to be queued.</param>
        /// <param name="isHighPriorityMessage">Whether or not the message is a high priority message.</param>
        public void AddMessage(ChatMessage message, bool isHighPriorityMessage)
        {
            if (isHighPriorityMessage)
            {
                this.highPriorityMessages.Enqueue(message);
            }
            else
            {
                this.normalPriorityMessages.Enqueue(message);
            }

            if (!this.sendMessageTimer.Enabled)
            {
                this.sendMessageTimer.Start();
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
            if(this.highPriorityMessages.Count > 0)
            {
                SendMessage(this.highPriorityMessages.Dequeue());
            }
            else if(this.normalPriorityMessages.Count > 0)
            {
                SendMessage(this.normalPriorityMessages.Dequeue());
            }
            else
            {
                this.sendMessageTimer.Stop();
            }
        }

        private void SendMessage(ChatMessage message)
        {
            if(this.MessageReady != null)
            {
                this.MessageReady(this,
                    new ChatMessageEventArgs()
                    {
                        ChatMessage = message
                    });
            }
        }
    }
}
