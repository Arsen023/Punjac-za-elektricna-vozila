using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace EVCharger.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(EVChargerService));

            try
            {
                host.Open();

                Console.WriteLine("EV Charger WCF service is running...");
                Console.WriteLine("Address: net.tcp://localhost:4000/EVChargerService");
                Console.WriteLine("Press ENTER to stop server.");

                Console.ReadLine();

                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server error: " + ex.Message);
                host.Abort();
            }
        }
    }
}
