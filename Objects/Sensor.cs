using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Objects
{
    public class Sensor
    {
        public string ID { get; set; }
        public string DataType { get; set; }
        public string ValueType { get; set; }
        public object Value { get; set; }
    }
}
