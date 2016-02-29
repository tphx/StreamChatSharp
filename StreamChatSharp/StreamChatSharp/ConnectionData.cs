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
        /// <param name="nickname">Nickname to connect with.</param>
        /// <param name="password">OAuth to connect with.</param>
        /// <param name="serverAddress">Server address to connect to.</param>
        /// <param name="port">Port to connect on.</param>
        public ConnectionData(string nickname, string password, string serverAddress, int port)
        {
            this.Nickname = nickname;
            this.Password = password;
            this.ServerAddress = serverAddress;
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
        /// Server address to connect to.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Port to connect on.
        /// </summary>
        public int Port { get; set; }
    }
}
