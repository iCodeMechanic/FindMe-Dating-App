using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class MessageParams
    {
        public string? Username { get; set; }
        
        public string? Container { get; set; } = "Unread";
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize { get; set; } = 5;
           
    }
}