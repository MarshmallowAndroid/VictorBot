using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
