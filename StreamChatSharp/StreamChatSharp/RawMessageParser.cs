// The MIT License (MIT)
//
// Copyright (c) 2014 TPHX
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Parses raw IRC messages.
    /// </summary>
    static class RawMessageParser
    {
        private enum RawMessagePart
        {
            Source = 0, // Sender of the message.
            PingPong = 0, // PING or PONG IRC command.
            MessageTags = 0, // Message tags that are sent with messages when the tags capability is requested.
            Command = 1, // IRC command contained in the raw message.
            MessageChannel = 2, // The target of the raw message.
            EndNamesListChannelName = 3, // The channel name for the 366 end of names command.
            MessageStart = 3, // The "default" beginning position of a message.
            InvalidCommand = 3, // The invalid command that was given if there is one.
            Mode = 3, // The mode that is set on the ModeTarget. Only used with MODE irc command.
            CapStatus = 3, // Whether or not a capability request was ACK'd or NAK'd.
            CapType = 4, // The type of capability that was requested.
            ModeTarget = 4, // The target of a MODE IRC command. Only used with MODE irc command.
            InvalidCommandMessageStart = 4, // The start of the message received when a bad command is issued.
            NamesListChannelName = 4, // The channel name in the 353 names list.
            EndNamesListMessageStart = 4 ,// Where the message for the 366 end of names command starts.
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
            List<string> rawMessageParts = rawMessage.Split(' ').ToList();
            ChatMessage chatMessage = new ChatMessage();

            try 
            {
                // Message tags are prepended to the begining of the raw message and are identified by the prefix '@'.
                // In order for all of the other RawMessagePart indexes to be correct we need to remove them if they
                // exist.
                if (rawMessage.StartsWith("@"))
                {
                    // Remove the '@' prefix.
                    chatMessage.Tags = rawMessageParts[(int)RawMessagePart.MessageTags].Remove(0, 1);
                    rawMessageParts.RemoveAt((int)RawMessagePart.MessageTags);
                }

                // Ping and pong commands are sent and received slightly differently than other commands; therefore, 
                // they must be checked separately.
                if (String.Equals(rawMessageParts[(int)RawMessagePart.PingPong],"PING", 
                    StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(rawMessageParts[(int)RawMessagePart.PingPong], "PONG",
                    StringComparison.OrdinalIgnoreCase))
                {
                    chatMessage.Command = rawMessageParts[(int)RawMessagePart.PingPong];
                }
                else
                {
                    switch (rawMessageParts[(int)RawMessagePart.Command])
                    {
                        case "JOIN":
                            // :mynickname!mynickname@mynickname.tmi.twitch.tv JOIN #channel
                        case "PART":
                            // :mynickname!mynickname@mynickname.tmi.twitch.tv PART #channel
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.MessageChannel];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            break;
                        case "MODE":
                            // :jtv MODE #channel +o nickname
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.Message = rawMessageParts[(int)RawMessagePart.Mode];
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.MessageChannel];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            chatMessage.Target = rawMessageParts[(int)RawMessagePart.ModeTarget];
                            break;
                        case "353":
                            // Names List.
                            // :nickname!nickname@nickname.tmi.twitch.tv 353 nickname = #channelName :Names
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.Message = GetMessageFromRawMessage(rawMessageParts, 
                                (int)RawMessagePart.NamesStart);
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.NamesListChannelName];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            break;
                        case "366":
                            // :mynickname.tmi.twitch.tv 366 mynickname #channelname :End of /NAMES list
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.Message = GetMessageFromRawMessage(rawMessageParts, 
                                (int)RawMessagePart.EndNamesListMessageStart);
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.MessageChannel];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            break;
                        case "421":
                            // Invalid command.
                            // :tmi.twitch.tv 421 nickname BADCOMMAND :Unknown command.
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.Message = String.Format("{0}: {1}", GetMessageFromRawMessage(rawMessageParts,
                                    (int)RawMessagePart.InvalidCommandMessageStart),
                                    rawMessageParts[(int)RawMessagePart.InvalidCommand]);
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.MessageChannel];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            break;
                        case "USERSTATE":
                            // :tmi.twitch.tv USERSTATE #channelname
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.MessageChannel];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            break;
                        case "ROOMSTATE":
                            // :tmi.twitch.tv ROOMSTATE #channelname
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.MessageChannel];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            break;
                        case "CAP":
                            // :tmi.twitch.tv CAP * ACK :twitch.tv/tags
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.Message = String.Format("{0} {1}", 
                                rawMessageParts[(int)RawMessagePart.CapStatus], 
                                GetMessageFromRawMessage(rawMessageParts, (int)RawMessagePart.CapType));
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.MessageChannel];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            break;
                        case "PRIVMSG":
                            // :nickname!nickname@nickname.tmi.twitch.tv PRIVMSG #channelname :This is the message.
                        case "NOTICE":
                            // :tmi.twitch.tv NOTICE #channelname :This room is now in r9k mode.
                        case "HOSTTARGET":
                        // :tmi.twitch.tv HOSTTARGET #channelname :username 1
                        case "CLEARCHAT":
                            // :tmi.twitch.tv CLEARCHAT #channelname :issuerusername
                        default:
                            // Try to parse the message in the standard message format
                            // :nickname!nickname@nickname.tmi.twitch.tv PRIVMSG #channel :This is the message.
                            chatMessage.Command = rawMessageParts[(int)RawMessagePart.Command];
                            chatMessage.Message = GetMessageFromRawMessage(rawMessageParts,
                                (int)RawMessagePart.MessageStart);
                            chatMessage.ChannelName = rawMessageParts[(int)RawMessagePart.MessageChannel];
                            chatMessage.Source = GetSourceFromRawMessage(rawMessageParts[(int)RawMessagePart.Source]);
                            break;
                    }
                }       
            }
            // The message is unknown or malformed but might still be important, send it back in it's raw form 
            // so thats it isn't lost.
            catch (ArgumentOutOfRangeException)
            {
                chatMessage.Command = "RAW";
                chatMessage.Message = rawMessage;
            }

            return chatMessage;
        }

        /// <summary>
        /// Parses a ChatMessage into a raw message string.
        /// </summary>
        /// <param name="chatMessage">The chat message to convert to a raw message.</param>
        /// <returns>Raw message string from ChatMessage.</returns>
        public static string ChatMessageToRawMessage(ChatMessage chatMessage)
        {
            if (String.Equals(chatMessage.Command.ToUpper(), "RAW", StringComparison.OrdinalIgnoreCase))
            {
                return (!String.IsNullOrEmpty(chatMessage.Message) ? chatMessage.Message : "");
            }
            else
            {
                return String.Format("{0} {1} {2}", chatMessage.Command, chatMessage.ChannelName, 
                    String.Format(":{0}", chatMessage.Message)).Trim();
            }
        }

        private static string GetSourceFromRawMessage(string rawSource)
        {
            // The source received in the parameter should be in one of the following formats:
            // :nickname!nickname@nickname.tmi.twitch.tv
            // :tmi.twitch.tv
            // :jtv
            return rawSource.Split('.')[0].Split('!')[0].Remove(0, 1);
        }

        private static string GetMessageFromRawMessage(IList<string> rawMessage, int messageStartIndex)
        {
            // IRC prefixes most messages with ':' to indicate the message may contain spaces. It's not actually part of
            // the message and should be removed.
            return (messageStartIndex < rawMessage.Count) ? String.Join(" ", rawMessage.ToArray(), messageStartIndex, 
                (rawMessage.Count - messageStartIndex)).Trim().Remove(0, 1) : "";
        }
    }
}
