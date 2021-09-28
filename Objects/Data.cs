using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Objects
{
    public class Data
    {
        public List<Zone> Zones { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
