using System;
using MQTTnet;
using MQTTnet.Server;
using Newtonsoft.Json;
using Objects;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;
using System.IO;
using System.Linq;

namespace Mqtt_Server
{
    class Server
    {
        private List<Input> inputs;
        List<Transmitter> transmitters = new List<Transmitter>();
        List<Zone> zones = new List<Zone>();
        private Timer timer;

        public Server()
        {
            inputs = new List<Input>();
            SetTimer();

            MqttServerOptionsBuilder options = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(707)
                .WithConnectionValidator(OnNewConnection)
                .WithApplicationMessageInterceptor(OnNewMessage);

            IMqttServer mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(options.Build()).GetAwaiter().GetResult();

            Interface.ReadLine();
        }

        private void SetTimer()
        {
            timer = new Timer(5000);
            timer.Elapsed += ProcessData;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void LogData(Data data)
        {
            string fileName = Directory.GetCurrentDirectory() + "\\logs.txt";
            string content = File.ReadAllText(fileName);
            StreamWriter writer = new StreamWriter(fileName);

            content += JsonConvert.DeserializeObject(data.ToString()) + "\n,";
            writer.Write(content);
            writer.Close();
        }

        private void AggregateData(List<Input> inputs)
        {
            if (inputs == null || inputs.Count == 0)
                return;
            transmitters = new List<Transmitter>();

            List<Input> inps = inputs.Where(x => x.SensorId == inputs[0].SensorId)?.ToList();
            Input firstInput = inps[0];
            double avg = GetAvg(inps);
            string avgFormatted = FormatValue(avg, firstInput.ValueType);

            Sensor sensor = new Sensor() { ID = firstInput.SensorId, DataType = firstInput.DataType, ValueType = firstInput.ValueType, Value = avgFormatted };
            Zone zone = zones.Where(x => x.Room == firstInput.ZoneId)?.FirstOrDefault();
            if (zone == null)
            {
                List<Sensor> sensors = new List<Sensor>();
                sensors.Add(sensor);
                Transmitter transmitter = new Transmitter() { ID = firstInput.TransmitterId, Sensors = sensors };
                transmitters.Add(transmitter);
                zones.Add(new Zone() { Room = firstInput.ZoneId, Transmitters = transmitters });
            }
            else
            {
                Transmitter transmitter = zone.Transmitters.Where(x => x.ID == firstInput.TransmitterId)?.FirstOrDefault();
                transmitter.Sensors.Add(sensor);
            }

            AggregateData(inputs.Where(x => x.SensorId != firstInput.SensorId)?.ToList());
        }

        private string FormatValue(double value, string valueType)
        {
            switch (valueType)
            {
                case "float": return value.ToString();
                default: return (Math.Truncate(value)).ToString();
            }
        }

        private double GetAvg(List<Input> inputs)
        {
            try
            {
                double total = 0.0;
                foreach (Input input in inputs)
                    total += Convert.ToDouble(input.Value);
                return total / inputs.Count;
            }
            catch (Exception e)
            {
                Interface.WriteLine("Wrong value");
                return default(double);
            }
        }

        private void ProcessData(Object source, ElapsedEventArgs e)
        {
            AggregateData(inputs);
            Data data = new Data() { TimeStamp = DateTime.Now, zones = zones };

            LogData(data);
            PushData(data);
        }

        private void PushData(Data data)
        {
            Interface.Clear();
            Interface.WriteLine(JsonConvert.DeserializeObject(data.ToString()));
            inputs = new List<Input>();
            zones = new List<Zone>();
        }

        private void OnNewConnection(MqttConnectionValidatorContext context)
        {
            Interface.WriteLine($"Client id: {context.ClientId}");
        }

        private void OnNewMessage(MqttApplicationMessageInterceptorContext context)
        {
            inputs.Add(JsonConvert.DeserializeObject<Input>(System.Text.Encoding.Default.GetString(context.ApplicationMessage?.Payload)));
            inputs[inputs.Count - 1].TimeStamp = DateTime.Parse(inputs[inputs.Count - 1].SentDate, new CultureInfo("fr-FR", false));
            //Interface.WriteLine($"Content : {JsonConvert.DeserializeObject(System.Text.Encoding.Default.GetString(context.ApplicationMessage?.Payload))}\tTopic: {context.ApplicationMessage?.Topic}");
        }
    }
}
