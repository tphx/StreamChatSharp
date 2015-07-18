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
        /// Queues an outgoing ChatMessage to be sent to the chat and starts the send timer unless the queue has been
        /// stopped manually via the Stop method.
        /// </summary>
        /// <param name="message">The chat message to be queued.</param>
        /// <param name="isHighPriorityMessage">Whether or not the message is a high priority message.</param>
        public void AddMessage(ChatMessage message, bool isHighPriorityMessage)
        {
            // If the send message timer isn't running (no message has been sent in since the last interval) we can 
            // send the message instantly.
            if (!this.sendMessageTimer.Enabled)
            {
                SendMessage(message);
                this.sendMessageTimer.Start();
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
            if (this.highPriorityMessages.Count > 0 || this.normalPriorityMessages.Count > 0)
            {
                this.sendMessageTimer.Start();
            }

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
            this.sendMessageTimer.Stop();
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
            if (this.highPriorityMessages.Count > 0)
            {
                ChatMessage message;
                this.highPriorityMessages.TryDequeue(out message);
                SendMessage(message);
            }
            else if (this.normalPriorityMessages.Count > 0)
            {
                ChatMessage message;
                this.normalPriorityMessages.TryDequeue(out message);
                SendMessage(message);
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
                this.MessageReady(this, new ChatMessageEventArgs(message));
            }
        }
    }
}
