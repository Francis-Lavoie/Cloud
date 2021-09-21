using System;
using System.Collections.Generic;
using System.Text;

namespace Objects
{
    public class Interface
    {
        public static void WriteLine(object content)
        {
            Console.WriteLine(content);
        }

        public static void Write(object content)
        {
            Console.WriteLine(content);
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
