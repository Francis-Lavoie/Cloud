using System;
using System.Collections.Generic;
using System.Text;

namespace Objects
{
    public static class TopicManager
    {
        public static Dictionary<string, string> ParseTopic(string topic)
        {
            string baseKeyword = "LOCAL";
            int partCount = 5;
            string[] keys = new string[] { "baseKeyword","zoneId", "transmitterId", "sensorId", "dataType" };
            string[] values = topic.Split('/');
            Dictionary<string, string> keyValues = new Dictionary<string, string>();

            if (topic == null || topic == "")
                return null;
            if (topic.Split('/').Length != partCount)
                return null;
            if (!topic.ToUpper().StartsWith(baseKeyword))
                return null;

            for (int i = 0; i < keys.Length; i++)
                keyValues[keys[i]] = values[i];
            return keyValues;
        }
    }
}
