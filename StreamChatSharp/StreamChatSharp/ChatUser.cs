using System;
using System.Drawing;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Data for a chat user.
    /// </summary>
    public class ChatUser
    {
        private string emoteSets;
        private Color color;

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
        /// Whether or not the user is a Twitch administrator.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Whether or not the user is a the owner of the channel.
        /// </summary>
        public bool IsChannelOwner { get; set; }

        /// <summary>
        /// Color the user has selected for their name to appear in chat.
        /// </summary>
        public Color Color
        {
            get
            {
                return this.color.IsEmpty ? ColorTranslator.FromHtml("#000000") : this.color;
            }

            set
            {
                this.color = value;
            }
        }

        /// <summary>
        /// Emote sets the user has access to.
        /// </summary>
        public string EmoteSets
        {
            get
            {
                return this.emoteSets ?? "0";
            }

            set
            {
                this.emoteSets = value;
            }
        }

        /// <summary>
        /// Sets the user state based on the chat message.
        /// </summary>
        /// <param name="userStateMessage">Message containing the user states. </param>
        internal void SetUserState(ChatMessage userStateMessage)
        {
            // The user states contain various states delimited by semicolons.
            // color=#FF0000;display-name=username;emote-sets=0;subscriber=0;turbo=0;user-type=
            string[] states = userStateMessage.Tags.Split(';');

            for (int a = 0; a < states.Length; a++)
            {
                string[] state = states[a].Split('=');

                switch (state[0])
                {
                    case "color":
                        // A color value will only be present if the user set one on Twitch.
                        this.Color = String.IsNullOrEmpty(state[1]) ? ColorTranslator.FromHtml("#000000") : 
                            ColorTranslator.FromHtml(state[1]);
                        break;
                    case "display-name":
                        if (!String.IsNullOrEmpty(state[1]))
                        {
                            this.UserName = state[1];
                        }
                        break;
                    case "emote-sets":
                        this.EmoteSets = state[1];
                        break;
                    case "subscriber":
                        this.IsSubscriber = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "turbo":
                        this.IsTurbo = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "user-type":
                        SetUserType(state[1]);
                        break;
                }
            }
        }

        private void SetUserType(string userType)
        {
            if(!String.IsNullOrEmpty(userType))
            {
                switch(userType)
                {
                    case "mod":
                        this.IsModerator = true;
                        break;
                    case "global_mod":
                        this.IsGlobalModerator = true;
                        this.IsModerator = true;
                        break;
                    case "admin":
                        this.IsAdmin = true;
                        this.IsModerator = true;
                        break;
                    case "staff":
                        this.IsStaff = true;
                        break;
                }
            }
            else
            {
                // If any user type was set, moderator or staff should at least be true. If the user type is blank then 
                // the use lost privileges.
                if (this.IsModerator && this.IsStaff && !this.IsChannelOwner)
                {
                    this.IsModerator = false;
                    this.IsGlobalModerator = false;
                    this.IsAdmin = false;
                    this.IsStaff = false;
                }
            }
        }
    }
}
