﻿// The MIT License (MIT)
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