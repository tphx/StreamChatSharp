using System;
using System.Drawing;

namespace Tphx.StreamChatSharp
{
    /// <summary>
    /// Data for a chat user.
    /// </summary>
    public class ChatUser
    {
        private Color color;

        /// <summary>
        /// Creates a new chat user with the name specified.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channelName">Name of the channel the user is currently in.</param>
        public ChatUser(string userName, string channelName)
        {
            UserName = userName;
            
            //We need to remove the # from the beginning of the channel name.
            if(String.Equals(UserName, channelName.Remove(0, 1), StringComparison.OrdinalIgnoreCase))
            {
                IsChannelOwner = true;
                IsModerator = true;
            }
        }

        /// <summary>
        /// Name of the user. Formatted with capitals if the user has set it.
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
                return color.IsEmpty ? ColorTranslator.FromHtml("#000000") : color;
            }

            set
            {
                color = value;
            }
        }

        /// <summary>
        /// Emote sets the user has access to.
        /// </summary>
        public string EmoteSets { get; set; }

        /// <summary>
        /// User's ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// User's type.
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// Whether or not the user is banned.
        /// </summary>
        public bool IsBanned { get; set; }

        /// <summary>
        /// How long the user was banned for. -1 is permanent.
        /// </summary>
        public int BanDuration { get; set; }

        /// <summary>
        /// The reason the user was banned.
        /// </summary>
        public string BanReason { get; set; }

        /// <summary>
        /// Bans the user. -1 means the user is permanently banned.
        /// </summary>
        /// <param name="banDuration">Amount of time the user has been banned for. -1 for permanent.</param>
        /// <param name="reason"></param>
        public void BanUser(int banDuration, string reason)
        {
            IsBanned = true;
            BanDuration = banDuration;
            BanReason = reason;
        }

        /// <summary>
        /// Unbans the user.
        /// </summary>
        public void UnbanUser()
        {
            BanReason = "";
            BanDuration = 0;
            IsBanned = false;
        }

        /// <summary>
        /// Processes a chat message.
        /// </summary>
        /// <param name="chatMessage">Message to process.</param>
        internal void ProcessChatMessage(ChatMessage chatMessage)
        {
            // If we are receiving messages from the user they are unbanned.
            if(IsBanned)
            {
                UnbanUser();
            }

            ProccessMessageTags(chatMessage.Tags);
        }


        private void ProccessMessageTags(string tags)
        {
            // The user states contain various states delimited by semicolons.
            // color=#FF0000;display-name=username;emote-sets=0;subscriber=0;turbo=0;user-type=
            string[] states = tags.Split(';');

            for (int a = 0; a < states.Length; a++)
            {
                string[] state = states[a].Split('=');

                switch (state[0])
                {
                    case "color":
                        Color = String.IsNullOrEmpty(state[1]) ? Color.Empty : ColorTranslator.FromHtml(state[1]);
                        break;
                    case "display-name":
                        if (!String.IsNullOrEmpty(state[1]))
                        {
                            UserName = state[1];
                        }
                        break;
                    case "emote-sets":
                        EmoteSets = state[1];
                        break;
                    case "mod":
                        IsModerator = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "subscriber":
                        IsSubscriber = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "turbo":
                        IsTurbo = Convert.ToBoolean(Convert.ToInt32(state[1]));
                        break;
                    case "user-id":
                        UserId = state[1];
                        break;
                    case "user-type":
                        SetUserType(state[1]);
                        break;
                }
            }
        }

        private void SetUserType(string userType)
        {
            if (!String.IsNullOrEmpty(userType))
            {
                switch (userType)
                {
                    case "mod":
                        IsModerator = true;
                        break;
                    case "global_mod":
                        IsGlobalModerator = true;
                        IsModerator = true;
                        break;
                    case "admin":
                        IsAdmin = true;
                        IsModerator = true;
                        break;
                    case "staff":
                        IsStaff = true;
                        break;
                }
            }
        }
    }
}
