using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Objects
{
    public class Input
    {
        public string SentDate { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ValueType { get; set; }
        public object Value { get; set; }
        public string SensorId { get; set; }
        public string TransmitterId { get; set; }
        public string ZoneId { get; set; }
        public string DataType { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
