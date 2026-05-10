using EVCharger.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace EVCharger.Server
{
    public class EVChargerService : IEVChargerService
    {
        [OperationBehavior(AutoDisposeParameters = true)]
        public ChargingDataResult SendChargingData(ChargingDataOptions options)
        {
            try
            {
                if(options == null)
                {
                    return new ChargingDataResult(
                        ResultType.Warning, 
                        "Options object is null.");
                }

                if(options.MemoryStream == null || options.MemoryStream.Length == 0)
                {
                    return new ChargingDataResult(
                        ResultType.Warning,
                        "Memory stream is empty.");
                }

                string folderPath = "ReceivedFiles";

                if(!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, options.FileName);

                using (FileStream fs = File.Create(filePath))
                {
                    options.MemoryStream.Position = 0;

                    options.MemoryStream.CopyTo(fs);
                }


                Console.WriteLine("--------------------------------");
                Console.WriteLine("NEW FILE RECEIVED");
                Console.WriteLine($"Vehicle ID: {options.VehicleId}");
                Console.WriteLine($"File Name : {options.FileName}");
                Console.WriteLine("--------------------------------");


                return new ChargingDataResult(
                    ResultType.Success,
                    "Charging data successfully received.");

            }
            catch(Exception ex)
            {
                return new ChargingDataResult(
                    ResultType.Failed,
                    ex.Message);
            }
        }
    }
}
