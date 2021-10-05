using System;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Objects;

namespace Mqtt_Client
{
    class Client
    {
        private IManagedMqttClient mqttClient;
        private Random rnd;

        public Client()
        {
            rnd = new Random();
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
                                                       .WithClientId("Development")
                                                       .WithTcpServer("localhost", 707);

            ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
                                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(10))
                                    .WithClientOptions(builder.Build())
                                    .Build();

            mqttClient = new MqttFactory().CreateManagedMqttClient();
            SetHandlers();
            mqttClient.StartAsync(options).GetAwaiter().GetResult();
        }

        public void Live()
        {
            while (true)
            {
                SendRandomData();
                Task.Delay(1000).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Sends random data to the server.
        /// </summary>
        private void SendRandomData()
        {
            Random rnd = new Random();
            string format = "yyyy-MM-dd HH:mm:ss";

            int value = rnd.Next(1, 25);
            string valueType = "Integer";
            string sensorId = "1";
            string transmitterId = "DICJLab02";
            string zoneId = "1";
            string dataType = "Temperature";
            string date = DateTime.Now.ToString(format);
            Input input = new Input()
            {
                Value = value,
                ValueType = valueType,
                SensorId = sensorId,
                TransmitterId = transmitterId,
                ZoneId = zoneId,
                SentDate = date,
                DataType = dataType
            };

            string json = JsonConvert.SerializeObject(input);
            mqttClient.PublishAsync("dev.to/topic/json", json);

            value = rnd.Next(1, 25);
            valueType = "float";
            sensorId = "2";
            transmitterId = "DICJLab01";
            zoneId = "2";
            date = DateTime.Now.ToString(format);
            input = new Input()
            {
                Value = value,
                ValueType = valueType,
                SensorId = sensorId,
                TransmitterId = transmitterId,
                ZoneId = zoneId,
                SentDate = date,
                DataType = dataType
            };

            json = JsonConvert.SerializeObject(input);
            mqttClient.PublishAsync("dev.to/topic/json", json);
        }

        /// <summary>
        /// Sets the event handler of the client.
        /// </summary>
        private void SetHandlers()
        {
            mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);
        }

        private void OnConnected(MqttClientConnectedEventArgs obj)
        {
            //Log.Logger.Information("Successfully connected.");
            Interface.WriteLine("Connected");
        }

        private void OnConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
            //Log.Logger.Warning("Couldn't connect to broker.");
            Interface.WriteLine("Connection failed");
        }

        private void OnDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            //Log.Logger.Information("Successfully disconnected.");
            Interface.WriteLine("Disconnected");
        }
    }
}
