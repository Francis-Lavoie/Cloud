using System;

namespace Mqtt_Client
{

    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Live();
        }
    }
}
