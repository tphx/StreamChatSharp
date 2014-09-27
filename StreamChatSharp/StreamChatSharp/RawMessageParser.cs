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

using System.Text;

namespace Tphx.StreamChatSharp
{
    static class RawMessageParser
    {
        private enum RawMessagePart
        {
            Source = 0, // Sender of the message.
            PingPong = 0, // PING or PONG IRC command.
            Command = 1, // IRC command contained in the raw message.
            Channel = 2, // The target of the raw message.
            PrivateMessageStart = 3, // The beginning of the message from a IRC PRIVMSG command.
            InvalidCommand = 3, // The invalid command that was given if there is one.
            Mode = 3, // The mode that is set on the ModeTarget. Only used with MODE irc command.
            ModeTarget = 4, // The target of a MODE IRC command. Only used with MODE irc command.
            InvalidCommandMessageStart = 4, // The start of the message received when a bad command is issued.
            NamesListChannelName = 4, // The channel name in the 353 names list.
            NamesStart = 5 // The first name in a list of names received from the IRC 353 command.
        }

        /// <summary>
        /// Parses a raw message into a ChatMessage.
        /// </summary>
        /// <remarks>
        /// Raw message strings received from the Twitch IRC server should be in one of the following formats:
        /// :[source] [IRC command] [Target] [IRC command parameters]
        /// :[source] [IRC MODE command] [Target] [Mode] [Mode target]
        /// </remarks>
        /// <param name="rawMessage">Raw message to parse.</param>
        /// <returns>ChatMessage from raw message.</returns>
        public static ChatMessage ReceivedRawMessageToChatMessage(string rawMessage)
        {
            string[] rawMessageParts = rawMessage.Split(' ');

            // Ping and pong commands are sent and received slightly differently than other commands; therefore, they
            // must be checked separately.
            if ((rawMessageParts[(int)RawMessagePart.PingPong] == "PING") ||
                (rawMessageParts[(int)RawMessagePart.PingPong] == "PONG"))
            {
                return CreateChatMessage("", "", "", rawMessageParts[(int)RawMessagePart.PingPong], "");
            }
            else
            {
                switch (rawMessageParts[(int)RawMessagePart.Command])
                {
                    case "PRIVMSG":
                        // :nickname!nickname@nickname.tmi.twitch.tv PRIVMSG #channel :This is the message.
                        return CreateChatMessage(GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]),
                            "",
                            rawMessageParts[(int)RawMessagePart.Channel],
                            rawMessageParts[(int)RawMessagePart.Command], 
                            GetMessageFromRawMessage(rawMessageParts, (int)RawMessagePart.PrivateMessageStart));
                    case "JOIN":
                        // :nickname!nickname@nickname.tmi.twitch.tv JOIN #channel
                        return CreateChatMessage(GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]),
                            "",
                            rawMessageParts[(int)RawMessagePart.Channel], 
                            rawMessageParts[(int)RawMessagePart.Command], 
                            "");
                    case "PART":
                        // :nickname!nickname@nickname.tmi.twitch.tv PART #channel
                        return CreateChatMessage(GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]),
                            "",
                            rawMessageParts[(int)RawMessagePart.Channel], 
                            rawMessageParts[(int)RawMessagePart.Command], 
                            "");
                    case "MODE":
                        // :jtv MODE #channel +o nickname
                        return CreateChatMessage(GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]),
                            rawMessageParts[(int)RawMessagePart.ModeTarget],
                            rawMessageParts[(int)RawMessagePart.Channel],
                            rawMessageParts[(int)RawMessagePart.Command], 
                            rawMessageParts[(int)RawMessagePart.Mode]);
                    case "353":
                        // Names List.
                        // :nickname!nickname@nickname.tmi.twitch.tv 353 nickname = #channelName :Names
                        return CreateChatMessage(GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]),
                            "",
                            rawMessageParts[(int)RawMessagePart.NamesListChannelName],
                            "NAMES", 
                            GetMessageFromRawMessage(rawMessageParts, (int)RawMessagePart.NamesStart));
                    case "421": 
                        // Bad command.
                        // :tmi.twitch.tv 421 nickname BADCOMMAND :Unknown command
                        return CreateChatMessage(GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]),
                            "",
                            rawMessageParts[(int)RawMessagePart.Channel],
                            rawMessageParts[(int)RawMessagePart.Command], 
                            string.Format("{0}: {1}", GetMessageFromRawMessage(rawMessageParts, 
                                (int)RawMessagePart.InvalidCommandMessageStart), 
                                rawMessageParts[(int)RawMessagePart.InvalidCommand]));
                    default:
                        return CreateChatMessage("", "", "", "RAW", rawMessage);
                }
            }
        }

        /// <summary>
        /// Parses a ChatMessage into a raw message string.
        /// </summary>
        /// <param name="chatMessage">The chat message to convert to a raw message.</param>
        /// <returns>Raw message string from ChatMessage.</returns>
        public static string ChatMessageToRawMessage(ChatMessage chatMessage)
        {
            if (chatMessage.Command.ToUpper() == "RAW")
            {
                return (!string.IsNullOrEmpty(chatMessage.Message) ? chatMessage.Message : "");
            }
            else
            {
                return string.Format("{0} {1} {2}", chatMessage.Command,
                    (!string.IsNullOrWhiteSpace(chatMessage.Channel) ? chatMessage.Channel : ""),
                    (!string.IsNullOrWhiteSpace(chatMessage.Message) ? string.Format(":{0}", chatMessage.Message) :
                        "")).Trim();
            }
        }

        /// <summary>
        /// Creates an IRC message.
        /// </summary>
        /// <param name="source">The sender of the message.</param>
        /// <param name="target">The target of the messsage.</param>
        /// <param name="channel">The chcannel the message is for or from.</param>
        /// <param name="command">The IRC command to issue in the message.</param>
        /// <param name="message">The message portion of the message.</param>
        /// <returns>Chat message.</returns>
        public static ChatMessage CreateChatMessage(string source, string target, string channel, string command, 
            string message)
        {
            return new ChatMessage()
            {
                Source = source,
                Target = target,
                Channel = channel,
                Command = command,
                Message = message
            };
        }

        private static string GetSourceFromRawMessage(string rawSource)
        {
            // The source received in the parameter should be in one of the following formats:
            // :nickname!nickname@nickname.tmi.twitch.tv
            // :tmi.twitch.tv
            // :jtv
            return rawSource.Split('.')[0].Split('!')[0].Remove(0, 1);
        }

        private static string GetMessageFromRawMessage(string[] rawMessage, int messageStartIndex)
        {
            StringBuilder message = new StringBuilder(string.Format("{0} ",
                rawMessage[messageStartIndex].Remove(0, 1)));

            for (int a = (messageStartIndex + 1); a < rawMessage.Length; a++)
            {
                message.Append(string.Format("{0} ", rawMessage[a]));
            }

            return message.ToString().Trim();
        }
    }
}
