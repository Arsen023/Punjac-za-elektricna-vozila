using EVCharger.Common;
using System;
using System.ServiceModel;

namespace EVCharger.Server
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ServiceHost host = null;
            try
            {
                host = new ServiceHost(typeof(EVChargerService));
                host.Open();

                Console.WriteLine("EV Charger WCF service is running...");
                Console.WriteLine("Address: net.tcp://localhost:4000/EVChargerService");
                Console.WriteLine("Press ENTER to stop server.");

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server error: " + ex.Message);
            }
            finally
            {
                WcfResourceHelper.SafeShutdown(host);
            }

            return 0;
        }
    }
}
