using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EVCharger.Common
{
    [DataContract]
    //datacontract znaci da ovu klasu mogu da prebacujem snimm preko mreze
    public class ChargingDataOptions : IDisposable
    {
        [DataMember]
        public string VehicleId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public MemoryStream MemoryStream { get; set; }

        private bool disposed = false;

        public ChargingDataOptions()
        {

        }

        public ChargingDataOptions(string vehicleId, string fileName, MemoryStream memoryStream)
        {
            VehicleId = vehicleId;
            FileName = fileName;
            MemoryStream = memoryStream;
        }

        ~ChargingDataOptions()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (MemoryStream != null)
                    {
                        MemoryStream.Dispose();
                        MemoryStream = null;
                    }
                }

                disposed = true;
            }
        }
    }
}
