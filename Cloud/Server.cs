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
using System.Net.Http;
using MQTTnet.Protocol;
using System.DirectoryServices.Protocols;
using System.Security.Authentication;
using System.Net;

namespace Mqtt_Server
{
    class Server
    {
        private List<Input> inputs1;
        private List<Input> inputs2;
        private bool useInputs1;
        private string versionId = "1.1";

        List<Transmitter> transmitters = new List<Transmitter>();
        List<Zone> zones = new List<Zone>();
        private Timer timer;
        private int intervalSeconds = 5;

        private List<string> Users;
        private List<string> Passwords;

        public Server()
        {
            inputs1 = new List<Input>();
            inputs2 = new List<Input>();
            useInputs1 = true;

            Users = new List<string>() { "user1" };
            Passwords = new List<string>() { "password" };

            SetTimer();
            MqttServerOptions serverOptions = new MqttServerOptions();
            serverOptions.TlsEndpointOptions.IsEnabled = true;

            MqttServerOptionsBuilder options = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(8883)
                .WithConnectionValidator(OnNewConnection)
                .WithApplicationMessageInterceptor(OnNewMessage)
                .WithConnectionValidator(ValidateClient);

            IMqttServer mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(options.Build()).GetAwaiter().GetResult();

            Interface.ReadLine();
        }

        private void ValidateClient(MqttConnectionValidatorContext c)
        {
            try
            {
                if (!c.Username.ToUpper().StartsWith("SVEA"))
                {
                    c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    return;
                }

                using (LdapConnection connection = new LdapConnection("cegepjonquiere.ca"))
                {
                    NetworkCredential credential = new NetworkCredential(c.Username, c.Password);
                    connection.Credential = credential;
                    connection.Bind();
                    c.ReasonCode = MqttConnectReasonCode.Success;
                    return;
                }
            }
            catch (LdapException lex)
            {
                Interface.WriteLine(lex.Message + " " + lex.InnerException);
                c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                return;
            }
            catch (Exception e)
            {
                Interface.WriteLine(e.Message + " " + e.InnerException);
            }

            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i] == c.Username && Passwords[i] == c.Password)
                {
                    c.ReasonCode = MqttConnectReasonCode.Success;
                    return;
                }
            }
            c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
        }

        /// <summary>
        /// Creates and configures the timer property of the server used to send, log and display the average data.
        /// </summary>
        private void SetTimer()
        {
            timer = new Timer(intervalSeconds * 1000);
            timer.Elapsed += ProcessData;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        /// <summary>
        /// Logs the specified Data to a log file.
        /// </summary>
        /// <param name="data">The data to use.</param>
        private void LogData(Data data)
        {
            string fileName = Directory.GetCurrentDirectory() + "\\logs.txt";
            string content = File.ReadAllText(fileName);
            StreamWriter writer = new StreamWriter(fileName);

            content += JsonConvert.DeserializeObject(data.ToString()) + ",\n";
            writer.Write(content);
            writer.Close();
        }

        /// <summary>
        /// Fills the zone list with averaged and aggregated data with the given inputs.
        /// </summary>
        /// <param name="inputs">The list of inputs to use.</param>
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

        /// <summary>
        /// Formats the given value depending of the given valueType.
        /// </summary>
        /// <param name="value">The value to use.</param>
        /// <param name="valueType">The valueType to use.</param>
        /// <returns></returns>
        private string FormatValue(double value, string valueType)
        {
            switch (valueType)
            {
                case "float": return value.ToString();
                default: return (Math.Truncate(value)).ToString();
            }
        }

        /// <summary>
        /// Gets the average of the value of the given Input list.
        /// </summary>
        /// <param name="inputs">The Input list to use.</param>
        /// <returns>Return the average of the values.</returns>
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

        /// <summary>
        /// Aggregates, displays, logs and pushes the data in the given Input list.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void ProcessData(Object source, ElapsedEventArgs e)
        {
            List<Input> inputs = GetUsableInputList();
            if (inputs.Count == 0)
                return;

            DateTime startDate = inputs.OrderBy(x => x.TimeStamp).FirstOrDefault().TimeStamp;
            DateTime endDate = inputs.OrderByDescending(x => x.TimeStamp).FirstOrDefault().TimeStamp;
            useInputs1 = !useInputs1;

            AggregateData(inputs);
            Data data = new Data() { VersionId = versionId, Zones = zones, StartDate = startDate, EndDate = endDate };

            DisplayInfo(data);
            LogData(data);
            PushData(data);
        }

        /// <summary>
        /// Pushes the data to the 
        /// </summary>
        /// <param name="data"></param>
        private async void PushData(Data data)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string key = Authentication.GetFunctionKey();
                    string url = "https://svea21app.azurewebsites.net/api/HttpConnection";

                    client.DefaultRequestHeaders.Add("x-functions-key", key);
                    StringContent stringContent = new StringContent(data.ToString());
                    HttpResponseMessage response = await client.PostAsync(url, stringContent);
                    string responseString = await response.Content.ReadAsStringAsync();
                    Interface.WriteLine(responseString);
                }
                catch (Exception e)
                {
                    Interface.WriteLine($"Unable to push data : {e.Message}");
                }
            }
        }

        private void DisplayInfo(Data data)
        {
            Interface.Clear();
            Interface.WriteLine(JsonConvert.DeserializeObject(data.ToString()));
            List<Input> inputs = GetUsableInputList();
            inputs = new List<Input>();
            zones = new List<Zone>();
        }

        private List<Input> GetUsableInputList()
        {
            return useInputs1 ? inputs1 : inputs2;
        }

        private void OnNewConnection(MqttConnectionValidatorContext context)
        {
            Interface.WriteLine($"Client id: {context.ClientId}");
        }

        private void OnNewMessage(MqttApplicationMessageInterceptorContext context)
        {
            try
            {
                List<Input> inputs = GetUsableInputList();
                string topic = context.ApplicationMessage.Topic;
                Interface.WriteLine(topic);
                inputs.Add(JsonConvert.DeserializeObject<Input>(System.Text.Encoding.Default.GetString(context.ApplicationMessage?.Payload)));
                inputs[inputs.Count - 1].TimeStamp = DateTime.Parse(inputs[inputs.Count - 1].SentDate, new CultureInfo("fr-CA", false));
            }
            catch (Exception e)
            {
                Interface.WriteLine(e.Message);
                Interface.WriteLine(System.Text.Encoding.Default.GetString(context.ApplicationMessage?.Payload));
            }
        }
    }
}
