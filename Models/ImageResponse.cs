using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationBOT.Models
{
    internal class ImageResponse
    {
        [JsonProperty("data")]
        public List<ImageData> Data { get; set; }
    }
}
