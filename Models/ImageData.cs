using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationBOT.Models
{
    internal class ImageData
    {
        [JsonProperty("taskType")]
        public string TaskType { get; set; }

        [JsonProperty("taskUUID")]
        public string TaskUUID { get; set; }

        [JsonProperty("imageUUID")]
        public string ImageUUID { get; set; }

        [JsonProperty("NSFWContent")]
        public bool NSFWContent { get; set; }

        [JsonProperty("imageURL")]
        public string ImageURL { get; set; }
    }
}
