using Discord;

namespace VictorBot.Services.Audio
{
    public class UserState
    {
        public UserState(IUser user)
        {
            User = user;
        }

        public IUser User { get; }

        public TrackFile[] Results { get; set; }

        public bool IsChoosing { get; set; }
    }
}
