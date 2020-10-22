using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBot.GelbooruApi
{
    class GelbooruConfig
    {
        public List<UserConfig> UserConfigs { get; set; }
    }

    public class UserConfig
    {
        public ulong UserId { get; set; }
        public List<GuildConfig> GuildConfigs { get; set; }
    }

    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public List<ChannelConfig> ChannelConfigs { get; set; }
    }

    public class ChannelConfig
    {
        public ulong ChannelId { get; set; }
        public string LastGelbooruTags { get; set; }
    }
}
