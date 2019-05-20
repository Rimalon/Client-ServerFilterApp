using ServerLib;
using System;
using System.ServiceModel;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(ServerService)))
            {
                host.Open();
                Console.WriteLine("Press any button to stop hosting");
                Console.ReadKey();
            }
        }
    }
}
