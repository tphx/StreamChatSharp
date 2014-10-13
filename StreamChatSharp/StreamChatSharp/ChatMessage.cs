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

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Contains the information for a chat message.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Creates a message with null properties.
        /// </summary>
        public ChatMessage()
        {

        }

        /// <summary>
        /// Creates a message that only contains a command.
        /// </summary>
        /// <param name="command">Irc command.></param>
        public ChatMessage(string command)
        {
            this.Command = command;
        }

        /// <summary>
        /// Creates a message that only contains a command and message.
        /// </summary>
        /// <param name="command">Irc command.></param>
        /// <param name="message">Message.</param>
        public ChatMessage(string command, string message)
        {
            this.Command = command;
            this.Message = message;
        }

        /// <summary>
        /// Creates a message that contains a command, message, and channel.
        /// </summary>
        /// <param name="command">IRC command.</param>
        /// <param name="channel">Channel the message is meant for.</param>
        /// <param name="message">Message.</param>
        public ChatMessage(string command, string message, string channel)
        {
            this.Command = command;
            this.Message = message;
            this.Channel = channel;
        }

        /// <summary>
        /// Creates a new message that defines a command, message, channel, and source.
        /// </summary>
        /// <param name="command">IRC command.</param>
        /// <param name="channel">Channel the message is meant for.</param>
        /// <param name="message">Message.</param>
        /// <param name="source">Source of the message.</param>
        public ChatMessage(string command, string message, string channel, string source)
        {
            this.Command = command;
            this.Message = message;
            this.Channel = channel;
            this.Source = source;
        }

        /// <summary>
        /// Creates a new message that defines all properties.
        /// </summary>
        /// <param name="command">IRC command.</param>
        /// <param name="channel">Channel the message is meant for.</param>
        /// <param name="message">Message.</param>
        /// <param name="source">Source of the message.</param>
        /// <param name="target">target of the message.</param>
        public ChatMessage(string command, string message, string channel, string source, string target)
        {
            this.Command = command;
            this.Message = message;
            this.Channel = channel;
            this.Source = source;
            this.Target = target;
        }
        
        /// <summary>
        /// The sender of the message.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The target of a command.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The channel the message is being sent to or received from.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// The IRC command being issued.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The message that was sent.
        /// </summary>
        public string Message { get; set; }
    }
}
