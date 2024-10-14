using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationBOT.Models
{
    public class UserPreference
    {
        [Key]
        public long UserId { get; set; }  
        public required string SelectedModel { get; set; }
        public int Money { get; set; }
        public int CountGeneration { get; set; }
    }
}
