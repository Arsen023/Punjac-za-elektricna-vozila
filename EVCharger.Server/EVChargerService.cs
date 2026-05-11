using EVCharger.Common;
using System;

namespace EVCharger.Server
{
    public class EVChargerService : IEVChargerService
    {
        public void StartSession(string vehicleId)
        {
            ChargingServiceValidator.ThrowIfInvalidVehicleId(vehicleId);

            Console.WriteLine($"[StartSession] VehicleId={vehicleId}");
        }

        public void PushSample(ChargingSample sample)
        {
            ChargingServiceValidator.ThrowIfInvalidSample(sample);

            Console.WriteLine($"[PushSample] VehicleId={sample.VehicleId}, RowIndex={sample.RowIndex}, Ts={sample.Timestamp:O}");
        }

        public void EndSession(string vehicleId)
        {
            ChargingServiceValidator.ThrowIfInvalidVehicleId(vehicleId);

            Console.WriteLine($"[EndSession] VehicleId={vehicleId}");
        }
    }
}
