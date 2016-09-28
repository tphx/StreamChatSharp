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
        /// Triggered whenenver a chat message is received from the server.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> ChatMessageReceived;

        /// <summary>
        /// Triggered whenever messages stop being received from the server. The connection will need to be restarted 
        /// via the Start method.
        /// </summary>
        public event EventHandler ConnectionLost;
        
        private StreamReader reader;
        private Thread thread;
        private bool running;
        private bool connected;
        private bool disposed = false;

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
            reader = new StreamReader(networkStream);

            thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();

            running = true;
        }
        
        /// <summary>
        /// Stops receiving messages from the stream.
        /// </summary>
        public void Stop()
        {
            if (reader != null)
            {
                reader.Dispose();
            }

            running = false;
        }

        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    Stop();
                }

                disposed = true;
            }
        }

        private void Run()
        {
            string rawMessage;

            while(running)
            {
                rawMessage = ReadRawMessage();

                if(!String.IsNullOrWhiteSpace(rawMessage))
                {
                    MessageReceived(rawMessage);
                }
                else if (rawMessage == null)
                {
                    LostConection();
                }
                else
                {
                    // We read a blank line. The connection still exists but there is nothing going on.
                    connected = true;
                }
            }
        }

        private string ReadRawMessage()
        {
            try
            {
                return reader.ReadLine();
            }
            catch (IOException)
            {
                // The stream has probably been disposed, make the message null and let it timeout.
                return null;
            }
        }

        private void MessageReceived(string rawMessage)
        {
            if (RawMessageReceived != null)
            {
                RawMessageReceived(this, new RawMessageEventArgs(rawMessage));
            }

            if (ChatMessageReceived != null)
            {
                ChatMessageReceived(this,
                    new ChatMessageEventArgs(RawMessageParser.ReceivedRawMessageToChatMessage(rawMessage)));
            }

            connected = true;
        }

        private void LostConection()
        {
            // Set the connected flag to false and wait 5 seconds before trying to read again just to be sure the
            // conection is actually lost. If the next read fails, the connection is actually lost, otherwise everything
            // should continue on as normal.
            if (connected)
            {
                connected = false;
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
