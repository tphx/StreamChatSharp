namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// The data for a chat message.
    /// </summary>
    public class ChatMessage
    {
        private string source;
        private string target;
        private string channelName;
        private string command;
        private string message;
        private string tags;

        /// <summary>
        /// Creates a message.
        /// </summary>
        public ChatMessage()
        {
        }

        /// <summary>
        /// Creates a message that defines a command.
        /// </summary>
        /// <param name="command">Irc command.></param>
        public ChatMessage(string command)
            : this()
        {
            this.command = command;
        }

        /// <summary>
        /// Creates a message that defines a command and message.
        /// </summary>
        /// <param name="command">Irc command.></param>
        /// <param name="message">Message.</param>
        public ChatMessage(string command, string message)
            : this(command)
        {
            this.message = message;
        }

        /// <summary>
        /// Creates a message that defines a command, message, and channel name.
        /// </summary>
        /// <param name="command">IRC command.</param>
        /// <param name="channelName">Name of the channel the message is meant for.</param>
        /// <param name="message">Message.</param>
        public ChatMessage(string command, string message, string channelName)
            : this(command, message)
        {
            this.channelName = channelName;
        }

        /// <summary>
        /// Creates a new message that defines a command, message, channel name, and source.
        /// </summary>
        /// <param name="command">IRC command.</param>
        /// <param name="channelName">Name of the channel the message is meant for.</param>
        /// <param name="message">Message.</param>
        /// <param name="source">Source of the message.</param>
        public ChatMessage(string command, string message, string channelName, string source)
            : this(command, message, channelName)
        {
            this.source = source;
        }

        /// <summary>
        /// Creates a new message that defines all properties.
        /// </summary>
        /// <param name="command">IRC command.</param>
        /// <param name="channelName">Name of the channel the message is meant for.</param>
        /// <param name="message">Message.</param>
        /// <param name="source">Source of the message.</param>
        /// <param name="target">target of the message.</param>
        public ChatMessage(string command, string message, string channelName, string source, string target)
            : this(command, message, channelName, source)
        {
            this.target = target;
        }
        
        /// <summary>
        /// The sender of the message.
        /// </summary>
        public string Source 
        {
            get
            {
                return source ?? "";
            }
            set
            {
                source = value;
            }
        }

        /// <summary>
        /// The target of a command.
        /// </summary>
        public string Target
        {
            get
            {
                return target ?? "";
            }
            set
            {
                target = value;
            }
        }

        /// <summary>
        /// The name of channel the message is for.
        /// </summary>
        public string ChannelName
        {
            get
            {
                return channelName ?? "";
            }
            set
            {
                channelName = value;
            }
        }

        /// <summary>
        /// The IRC command being issued.
        /// </summary>
        public string Command
        {
            get
            {
                return command ?? "";
            }
            set
            {
                command = value;
            }
        }

        /// <summary>
        /// The message that was sent.
        /// </summary>
        public string Message
        {
            get
            {
                return message ?? "";
            }
            set
            {
                message = value;
            }
        }

        /// <summary>
        /// IRC message tags that are included when the tags capability has been requested from the IRC server.
        /// </summary>
        public string Tags
        {
            get
            {
                return tags ?? "";
            }
            set
            {
                tags = value;
            }
        }
    }
}
