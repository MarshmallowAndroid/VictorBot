using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhentaiApi
{
    public class NhentaiSearchResult
    {
        public NhentaiGallery[] Result { get; set; }
        public int Num_Pages { get; set; }
        public int Per_Page { get; set; }
    }
}
