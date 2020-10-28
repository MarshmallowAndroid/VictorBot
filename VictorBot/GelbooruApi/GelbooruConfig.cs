using System.Collections.Generic;

namespace VictorBot.GelbooruApi
{
    class GelbooruConfig
    {
        public Dictionary<ulong, UserConfig> UserConfigs { get; set; }
    }

    public class UserConfig
    {
        public Dictionary<ulong, string> MessageChannelTags { get; set; }
    }
}
