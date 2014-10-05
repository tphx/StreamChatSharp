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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tphx.StreamChatSharp;

namespace SimpleChatBot
{
    /// <summary>
    /// A simple chatbot to demonstrate how to use the StreamChatSharp library.
    /// </summary>
    class ChatBot : IDisposable
    {
        // This is our chat client. It will be used to connect to and communciate with the chat.
        private ChatClient chatClient = new ChatClient();

        string chatChannel; 

        private bool disposed = false;

        public ChatBot()
        {
            // In order to receive messages and know when the client disconnects we need to subscribe to the client's
            // events.
            this.chatClient.ChatMessageReceived += OnChatMessageReceived;
            this.chatClient.Disconnected += OnDisconnected;
        }

        /// <summary>
        /// Disposes of everything.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Connects the chatbot to the chat server and joins a chat channel.
        /// </summary>
        public void ConnectToChat()
        {
            this.chatClient.ConnectToChat(GetConnectionData());
            this.chatClient.JoinChannel(this.chatChannel);
        }

        /// <summary>
        /// Sends a private message to the chat channel the client is currently conected to.
        /// </summary>
        public void SendMessageToChat(string message)
        {
            // We should only send messages if we're connected to avoid problems.
            if(this.chatClient.Connected)
            {
                // Send the private message to the server. 
                this.chatClient.SendPrivateMessage(message, chatChannel, true);

                Console.WriteLine("> {0} ", message);
            }
            else
            {
                Console.WriteLine("The chat client cannot send messages because it is not currently connected.");
            }
        }

        /// <summary>
        /// Disconnects the chat client.
        /// </summary>
        public void Disconnect()
        {
            this.chatClient.Disconnect();
        }

        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    chatClient.Dispose();
                }

                this.disposed = true;
            }
        }

        private ConnectionData GetConnectionData()
        {
            // In order to connect to the chat we need to define the connection data to connect with. This defines
            // the server and port to connect to as well as the credentials (nickname and OAuth) needed to connect to
            // the chat.
            ConnectionData connectionData = new ConnectionData()
            {
                HostName = "irc.twitch.tv",
                Port = 6667
            };

            Console.WriteLine("Enter the Nickname to connect with:");
            connectionData.Nick = Console.ReadLine();
            Console.WriteLine("Enter the OAuth to connect with:");
            connectionData.OAuth = Console.ReadLine();

            // Now we can join a chat channel. We could join as many channels as we want to but we only need one for
            // this example.
            Console.WriteLine("Enter the name of the channel to join (ex. #channelname):");
            this.chatChannel = Console.ReadLine();

            return connectionData;
        }

        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            // We can handle messages based on their command.
            switch(e.ChatMessage.Command)
            {
                case "RAW":
                    Console.WriteLine("RAW - {0}", e.ChatMessage.Message);
                    break;
                case "PRIVMSG":
                    Console.WriteLine("{0}: {1}", e.ChatMessage.Source, e.ChatMessage.Message);
                    CheckMessageForBotCommand(e.ChatMessage);
                    break;
                case "JOIN":
                    Console.WriteLine("{0} joined.", e.ChatMessage.Source);
                    break;
                case "PART":
                    Console.WriteLine("{0} left.", e.ChatMessage.Source);
                    break;
                case "353":
                    Console.WriteLine("Names list: {0}.", string.Join(", ", e.ChatMessage.Message.Split(' ')));
                    break;
                case "366":
                    Console.WriteLine("End of names for {0}.", e.ChatMessage.Channel);
                    break;
                case "MODE":
                    Console.WriteLine("Set mode {0} for {1}", e.ChatMessage.Message, e.ChatMessage.Target);
                    break;
                case "PING":
                    // The pong reply is automatically sent by the client.
                    Console.WriteLine("Ping? Pong!");
                    break;
                case "PONG":
                    Console.WriteLine("Pong!");
                    break;
                case "421":
                    Console.WriteLine(e.ChatMessage.Message);
                    break;
                default:
                    Console.WriteLine("{0} --- {1}", e.ChatMessage.Command, e.ChatMessage.Message);
                    break;
            }
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            // When we disconnected the reason will tell us what caused the disconnect to occur. The reasons can be
            // the client disconnecting on its own (calling the disconnect method), timing out, or the connection
            // getting disposed somehow. If the connection times out, it will automatically keep trying to reconnect.
            Console.WriteLine("Disconnected: {0}", e.Reason.ToString());
        }

        private void CheckMessageForBotCommand(ChatMessage chatMessage)
        {
            string reply;

            chatMessage.Message = chatMessage.Message.ToLower().Trim();

            // If the message contains a bot command, send a reply.
            if(chatMessage.Message.StartsWith("!hi"))
            {
                reply = string.Format("Hello {0}.", chatMessage.Source);

                // In addition to using the ChatClient.SendPrivateMessage method we can manually make the chat message 
                // ourselves and send it. This has the same effect as the ChatClient.SendPrivateMessage method.
                ChatMessage replyMessage = new ChatMessage()
                {
                    Command = "PRIVMSG",
                    Channel = chatChannel,
                    Message = reply
                };
                this.chatClient.SendChatMessage(replyMessage, false);
                
                Console.WriteLine("> {0}", reply);
            }
            else if(chatMessage.Message.StartsWith("!time"))
            {
                reply = string.Format("Current time: {0}.", System.DateTime.Now.TimeOfDay);
                
                // We can also send raw messages to the server. We can do this by creating a ChatMessage, setting 
                // the command property to "RAW" and then sending the Message property to the raw message. Likewise, 
                // we can use the ChatClient.SendRawMessage method as seen below. The raw private message will have 
                // the same effect as the ChatClient.SendPrivateMessage method.
                this.chatClient.SendRawMessage(string.Format("PRIVMSG {0} :{1}", chatChannel, reply), false);
                
                Console.WriteLine("> {0}", reply);
            }
        }
    }
}
