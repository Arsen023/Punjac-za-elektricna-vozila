using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace EVCharger.Common
{
    [ServiceContract]
    public interface IEVChargerService
    {
        [OperationContract]
        ChargingDataResult SendChargingData(ChargingDataOptions options);
    }
}
