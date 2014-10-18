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
    /// Contains the information needed to connect to the Twitch IRC server.
    /// </summary>
    public class ConnectionData
    {
        /// <summary>
        /// Defines the arguments required to connect to a server.
        /// </summary>
        /// <param name="nick">Nickname to connect with.</param>
        /// <param name="nickname">OAuth to connect with.</param>
        /// <param name="hostName">Hostname to connect to.</param>
        /// <param name="port">Port to connect on.</param>
        public ConnectionData(string nick, string nickname, string hostName, int port)
        {
            this.Nickname = nick;
            this.Password = nickname;
            this.HostName = hostName;
            this.Port = port;
        }
        
        /// <summary>
        /// Nickname to connect with.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Password to connect with.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Hostname to connect to.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Port to connect on.
        /// </summary>
        public int Port { get; set; }
    }
}
