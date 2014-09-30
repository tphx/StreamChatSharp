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

            if (!this.stoppedManually && !this.sendMessageTimer.Enabled)
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
