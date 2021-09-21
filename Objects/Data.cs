using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Objects
{
    public class Data
    {
        public List<Zone> zones { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
