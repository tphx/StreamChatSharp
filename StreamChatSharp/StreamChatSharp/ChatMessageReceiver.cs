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
using System.IO;
using System.Threading;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Threaded message receiver used to receive messages on a stream.
    /// </summary>
    class ChatMessageReceiver : IDisposable
    {
        /// <summary>
        /// Triggered whenever a raw message is received from the server.
        /// </summary>
        public event EventHandler<RawMessageEventArgs> RawMessageReceived;

        /// <summary>
        /// Triggered whenever messages stop being received from the server. The connection will need to be restarted 
        /// via the Start method.
        /// </summary>
        public event EventHandler ConnectionLost;
        
        private StreamReader reader;
        private Thread thread;

        private bool disposed = false;

        private bool running;
        private bool connected;

        /// <summary>
        /// Disposes of everything.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Starts receiving messages from a stream.
        /// </summary>
        /// <param name="networkStream">The stream to receive messages from.</param>
        public void Start(Stream networkStream)
        {
            this.reader = new StreamReader(networkStream);

            this.thread = new Thread(Run);
            this.thread.Start();
            this.running = true;
        }
        
        /// <summary>
        /// Stops receiving messages from the stream.
        /// </summary>
        public void Stop()
        {
            this.reader.Dispose();
            this.running = false;
        }

        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    Stop();
                }

                this.disposed = true;
            }
        }

        private void Run()
        {
            string rawMessage;

            while(this.running)
            {
                try
                {
                    rawMessage = this.reader.ReadLine();
                }
                catch (IOException)
                {
                    // The stream has probably been disposed, make the message null and let it timeout.
                    rawMessage = null;
                }

                if(!string.IsNullOrWhiteSpace(rawMessage))
                {
                    if (this.RawMessageReceived != null)
                    {
                        this.RawMessageReceived(this, 
                            new RawMessageEventArgs()
                            {
                                RawMessage = rawMessage
                            });
                    }

                    this.connected = true;
                }
                else if (rawMessage == null)
                {
                    // If the message is actually null we probably lost connection. Set the connected flag to false 
                    // and wait 5 seconds before trying to read again just to be sure. If the next read fails, notify 
                    // of the lost connection and stop reading from the stream. If the next read succeeds everything 
                    // will go continue as normal normal.
                    if(this.connected)
                    {
                        this.connected = false;
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        Stop();

                        if (this.ConnectionLost != null)
                        {
                            this.ConnectionLost(this, new EventArgs());
                        }
                    }
                }
                else
                {
                    // We read a blank line. The connection still exists but there is nothing going on.
                    this.connected = true;
                }
            }
        }
    }
}
