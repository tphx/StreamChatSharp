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
using System.Text;
using System.Threading.Tasks;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Data for a chat user.
    /// </summary>
    public class ChatUser
    {
        /// <summary>
        /// The various types of special users.
        /// </summary>
        public enum SpecialUserType
        {
            /// <summary>
            /// Chat moderator.
            /// </summary>
            Moderator,
            /// <summary>
            /// Channel subscriber.
            /// </summary>
            Subscriber,
            /// <summary>
            /// Twitch Turbo user.
            /// </summary>
            Turbo,
            /// <summary>
            /// Twitch global moderator.
            /// </summary>
            GlobalModerator,
            /// <summary>
            /// Twitch staff.
            /// </summary>
            Staff
        }

        /// <summary>
        /// Creates a new chat user with the name specified.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public ChatUser(string userName)
        {
            this.UserName = userName;
        }

        /// <summary>
        /// Name of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Whether or not the user is a moderator.
        /// </summary>
        public bool IsModerator { get; set; }

        /// <summary>
        /// Whether or not the user a subscriber.
        /// </summary>
        public bool IsSubscriber { get; set; }

        /// <summary>
        /// Whether or not the user is a Twitch Turbo user.
        /// </summary>
        public bool IsTurbo { get; set; }

        /// <summary>
        /// Whether or not the user is a Twitch global moderator.
        /// </summary>
        public bool IsGlobalModerator { get; set; }

        /// <summary>
        /// Whether or not the user is a member of Twitch staff.
        /// </summary>
        public bool IsStaff { get; set; }

        /// <summary>
        /// Toggles the special user type.
        /// </summary>
        /// <param name="specialUserType">Special user type to toggle.</param>
        /// <param name="enabled">Whether or not the special user type is enabled.</param>
        public void ToggleSpecialUserType(SpecialUserType specialUserType, bool enabled)
        {
            switch (specialUserType)
            {
                case SpecialUserType.Moderator:
                    this.IsModerator = enabled;
                    break;
                case SpecialUserType.Subscriber:
                    this.IsSubscriber = enabled;
                    break;
                case SpecialUserType.Turbo:
                    this.IsTurbo = enabled;
                    break;
                case SpecialUserType.GlobalModerator:
                    this.IsGlobalModerator = enabled;
                    break;
                case SpecialUserType.Staff:
                    this.IsStaff = enabled;
                    break;
            }
        }
    }
}
