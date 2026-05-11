using System.ServiceModel;

namespace EVCharger.Common
{
    [ServiceContract]
    public interface IEVChargerService
    {
        [OperationContract]
        [FaultContract(typeof(ChargingValidationFault))]
        void StartSession(string vehicleId);

        [OperationContract]
        [FaultContract(typeof(ChargingValidationFault))]
        void PushSample(ChargingSample sample);

        [OperationContract]
        [FaultContract(typeof(ChargingValidationFault))]
        void EndSession(string vehicleId);
    }
}
