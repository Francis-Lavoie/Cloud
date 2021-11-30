using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Objects
{
    public class Authentication
    {
        private static string basePath = "C:\\Cloud\\";

        public static string GetFunctionKey()
        {
            try
            {
                StreamReader reader = new StreamReader($"{basePath}Access\\functionKey.pem");
                return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                Interface.WriteLine($"Unable to get access key. {e.Message}");
                return "";
            }
        }

        public static string GetAzureFunctionURL()
        {
            try
            {
                StreamReader reader = new StreamReader($"{basePath}Access\\AzureFunctionUrl.txt");
                return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                Interface.WriteLine($"Unable to get access key. {e.Message}");
                return "";
            }
        }
    }
}
