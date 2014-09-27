namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Contains the information needed to connect to the Twitch IRC server.
    /// </summary>
    public class ConnectionData
    {
        /// <summary>
        /// The nickname to connect with.
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// The OAuth to connect with.
        /// </summary>
        public string OAuth { get; set; }

        /// <summary>
        /// The hostname to connect to.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The port to connect on.
        /// </summary>
        public int Port { get; set; }
    }
}
