using EVCharger.Common;
using System;
using System.ServiceModel;

namespace EVCharger.Server
{
    public class EVChargerService : IEVChargerService
    {
        public void StartSession(string vehicleId)
        {
            try
            {
                ChargingServiceValidator.ThrowIfInvalidVehicleId(vehicleId);
            }
            catch (FaultException<ChargingValidationFault> ex)
            {
                //za 6 tacku ako je greska upisi u rejects.csv
                ChargingSessionFileStore.LogReject(ex.Detail);
                throw;
            }

            string sessionPath = ChargingSessionFileStore.StartSession(vehicleId);
            Console.WriteLine($"[StartSession] VehicleId={vehicleId}");
            Console.WriteLine($"             session.csv → {sessionPath}");
        }

        public void PushSample(ChargingSample sample)
        {
            try
            {
                ChargingServiceValidator.ThrowIfInvalidSample(sample);
            }
            catch (FaultException<ChargingValidationFault> ex)
            {
                ChargingSessionFileStore.LogReject(ex.Detail);
                throw;
            }

            ChargingSessionFileStore.AppendSample(sample);
            Console.WriteLine($"[PushSample] VehicleId={sample.VehicleId}, RowIndex={sample.RowIndex}, Ts={sample.Timestamp:O}");
        }

        public void EndSession(string vehicleId)
        {
            try
            {
                ChargingServiceValidator.ThrowIfInvalidVehicleId(vehicleId);
            }
            catch (FaultException<ChargingValidationFault> ex)
            {
                ChargingSessionFileStore.LogReject(ex.Detail);
                throw;
            }

            ChargingSessionFileStore.EndSession(vehicleId);
            Console.WriteLine($"[EndSession] VehicleId={vehicleId}");
        }
    }
}
