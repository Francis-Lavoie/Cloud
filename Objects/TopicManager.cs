using System;
using System.Collections.Generic;
using System.Text;

namespace Objects
{
    public static class TopicManager
    {
        public static Input ParseTopic(string topic)
        {
            string prefix = "LOCAL";
            int partCount = 5;
            string[] values = topic.Split('/');
            
            if (topic == null || topic == "")
                return null;
            if (topic.Split('/').Length != partCount)
                return null;
            if (!topic.ToUpper().StartsWith(prefix))
                return null;

            return new Input() { ZoneId = values[1], TransmitterId = values[2], SensorId = values[3], DataType = values[4] };
        }
    }
}
