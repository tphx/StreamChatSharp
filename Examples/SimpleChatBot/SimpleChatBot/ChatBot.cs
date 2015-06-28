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
        // This is our chat client. It will be used to connect to and communicate with the chat.
        private ChatClient chatClient;

        private List<string> channelsToJoin;
        private bool showRawMessages = false;
        private bool disposed = false;

        /// <summary>
        /// Run the chat bot.
        /// </summary>
        public void RunChatBot()
        {
            // To get started we need to create a new instance of the chat client and subscribe to it's events so we can
            // be notified when something happens.
            this.chatClient = new ChatClient();

            // This event lets us know when we have successfully connected to the server. We should wait until this 
            // event is fired before we join any channels or send any messages.
            this.chatClient.RegisteredWithServer += OnRegisteredWithServer;

            // This event lets us know that we have received a new chat message. The message can contain various
            // information such as the command, source, and name of the channel the message is meant for.
            this.chatClient.ChatMessageReceived += OnChatMessageReceived;

            // This event lets us know we have received a new raw message. This will show us every message we receive in 
            // its raw form. We mainly use OnChatMessageReceived for receiving messages but this event is useful for
            // debugging and troubleshooting purposes or if messages need to parsed in a certain way.
            this.chatClient.RawMessageReceived += OnRawMessageReceived;

            // This event lets us know the client has been disconnected from the server. It informs us of the reason we
            // were disconnected and whether or not the client is going to try to automatically reconnect.
            this.chatClient.Disconnected += OnDisconnected;

            // To connect to the IRC server all we have to do is use the chat client to connect to chat with our
            // connection data. When connecting we can choose whether or not we want to enable various IRCv3 
            // capabilities.
            this.chatClient.ConnectToChat(GetConnectionData(), true, true, true);

            // Keep the console open while the chatbot is running and check for input.
            while (true)
            {
                switch(Console.ReadLine())
                {
                    case "1":
                        showRawMessages = showRawMessages ? false : true;
                        break;
                    case "2":
                        ShowChannelStats();
                        break;
                    case "3":
                        ShowUserStats();
                        break;
                    case "x": // quit the program.
                        return;
                  
                }
            }
        }

        /// <summary>
        /// Dispose of everything.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    if (this.chatClient != null)
                    {
                        this.chatClient.Dispose();
                    }
                }
            }
        }

        private ConnectionData GetConnectionData()
        {
            Console.WriteLine("Enter the username to connect with:");
            string nickName = Console.ReadLine();
            Console.WriteLine("Enter the oauth to connect with:");
            string oauth = Console.ReadLine();
            Console.WriteLine("Enter the server address to connect to:");
            string serverAddress = Console.ReadLine();

            int port;
            while (true)
            {
                Console.WriteLine("Enter the port to connect to:");

                if (int.TryParse(Console.ReadLine(), out port))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("The port must be numeric.");
                }
            }

            // We shouldn't actually join the channels until our connection has been registered but this is a good place
            // to get them so we will store them until the register event is fired.
            Console.WriteLine("Enter the channels to connect to separated by commas (ex. #channelname1, " +
                "#channelname2):");
            this.channelsToJoin = Console.ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim()).ToList();

            return new ConnectionData(nickName, oauth, serverAddress, port);
        }

        private void OnRegisteredWithServer(object sender, EventArgs e)
        {
            Console.WriteLine("Connection registered.");

            // Once we are registered, we can join the channels we want. After we join the channels we should start
            // receiving messages for those channels.
            this.chatClient.JoinChannel(this.channelsToJoin);
        }

        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            switch(e.ChatMessage.Command)
            {
                case "353":
                    Console.WriteLine("Current chatters: {0}", e.ChatMessage.Message);
                    break;
                case "JOIN":
                    Console.WriteLine(string.Format("{0} joined {1}.", e.ChatMessage.Source, 
                        e.ChatMessage.ChannelName));
                    break;
                case "PART":
                    Console.WriteLine(string.Format("{0} left {1}.", e.ChatMessage.Source, e.ChatMessage.ChannelName));
                    break;
                case "MODE":
                    ModeReceived(e.ChatMessage);
                    break;
                case "PRIVMSG":
                    Console.WriteLine("{0} - {1}: {2}", e.ChatMessage.ChannelName, e.ChatMessage.Source, 
                        e.ChatMessage.Message);
                    CheckForChatCommand(e.ChatMessage);
                    break;
                case "366":
                case "ROOMSTATE":
                case "USERSTATE":
                    // We can ignore these.
                    break;
                default:
                    Console.WriteLine("{0} - {1} - {2} - {3} - {4}", e.ChatMessage.ChannelName, e.ChatMessage.Command,
                        e.ChatMessage.Target, e.ChatMessage.Source, e.ChatMessage.Message);
                    break;
            }
        }

        private void OnRawMessageReceived(object sender, RawMessageEventArgs e)
        {
            if(this.showRawMessages)
            {
                Console.WriteLine(e.RawMessage);
            }
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Disconnected from chat. Reason: {0} {1}", e.Reason, 
                (e.AttemptingAutoReconnect ? "Attempting to reconnect." : "")));
        }

        private void CheckForChatCommand(ChatMessage chatMessage)
        {
            // If the message contains a chat command we can send a reply using the information in the chat message. To
            // send the reply we can either create the private message ourselves using a ChatMessage and send it using 
            // ChatClient.SendChatMessage or we can use the ChatClient.SendPrivateMessage method and the ChatMessage 
            // will be made for us by the chat client. If the reply is important, setting the isHighPriority parameter 
            // to true in either method will make it send before any normal priority messages.
            if (chatMessage.Message.StartsWith("!hello", StringComparison.OrdinalIgnoreCase))
            {
                string reply = string.Format("Hello {0}.", chatMessage.Source);
                this.chatClient.SendChatMessage(new ChatMessage("PRIVMSG", reply, chatMessage.ChannelName), false);
                Console.WriteLine(string.Format("{0} > {1}", chatMessage.ChannelName, reply));
            }
            else if(chatMessage.Message.StartsWith("!time", StringComparison.OrdinalIgnoreCase))
            {
                string reply = string.Format("{0} - The current time is {1}.", chatMessage.Source, 
                    DateTime.Now.ToShortTimeString());
                this.chatClient.SendPrivateMessage(reply, chatMessage.ChannelName, false);
                Console.WriteLine(string.Format("{0} > {1}", chatMessage.ChannelName, reply));
            }
        }

        private void ModeReceived(ChatMessage chatMessage)
        {
            if (String.Equals(chatMessage.Message, "+o"))
            {
                Console.WriteLine(string.Format("Added moderator to {0} on {1}.", chatMessage.Target,
                    chatMessage.ChannelName));
            }
            else if (String.Equals(chatMessage.Message, "-o"))
            {
                Console.WriteLine(string.Format("Removed moderator from {0} on {1}.", chatMessage.Target,
                    chatMessage.ChannelName));
            }
        }

        private void ShowUserStats()
        {
            int totalUsers = 0;
            Console.WriteLine("----------------------------------------------");
            foreach (KeyValuePair<string, ChatChannel> channel in this.chatClient.Channels)
            {
                Console.WriteLine(channel.Value.ChannelName);
                Console.WriteLine("Total users: " + channel.Value.ChatUsers.Count);
                Console.WriteLine("Moderators: " + channel.Value.ChatUsers.Where(u => u.IsModerator).ToList().Count);
                Console.WriteLine("Global moderators: " + channel.Value.ChatUsers.Where(u => u.IsGlobalModerator)
                    .ToList().Count);
                Console.WriteLine("Staff: " + channel.Value.ChatUsers.Where(u => u.IsStaff).ToList().Count);
                Console.WriteLine("Admins: " + channel.Value.ChatUsers.Where(u => u.IsAdmin).ToList().Count);
                Console.WriteLine("Subscribers: " + channel.Value.ChatUsers.Where(u => u.IsSubscriber).ToList().Count);
                Console.WriteLine("Turbo users: " + channel.Value.ChatUsers.Where(u => u.IsTurbo).ToList().Count);
                Console.WriteLine("Channel owners: " + channel.Value.ChatUsers.Where(u => u.IsChannelOwner).ToList()
                    .Count);
                Console.WriteLine("----------------------------------------------");
                totalUsers += channel.Value.ChatUsers.Count;
            }
            Console.WriteLine("Total channels: " + this.chatClient.Channels.Count);
            Console.WriteLine("Total users: " + totalUsers);
            Console.WriteLine("----------------------------------------------");
        }

        private void ShowChannelStats()
        {
            Console.WriteLine("********************************");
            foreach (KeyValuePair<string, ChatChannel> channel in this.chatClient.Channels)
            {
                Console.WriteLine(channel.Value.ChannelName);
                Console.WriteLine("Subscribers only mode: " + channel.Value.SubscribersOnlyModeEnabled);
                Console.WriteLine("R9K mode: " + channel.Value.R9KModeEnabled);
                Console.WriteLine("Slow mode: " + channel.Value.SlowModeEnabled);
                Console.WriteLine("Slow interval: " + channel.Value.SlowModeInterval);
                Console.WriteLine("Language: " + channel.Value.BroadcasterLanguage);
                Console.WriteLine("********************************");
                Console.WriteLine("Total channels: " + this.chatClient.Channels.Count);
                Console.WriteLine("********************************");
            }
        }
    }
}
