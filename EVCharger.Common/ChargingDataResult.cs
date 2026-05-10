using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EVCharger.Common
{
    [DataContract]
    public class ChargingDataResult
    {
        [DataMember]
        public ResultType ResultType { get; set; }

        [DataMember]
        public string ResultMessage { get; set; }

        public ChargingDataResult()
        {
        }

        public ChargingDataResult(ResultType resultType, string resultMessage)
        {
            ResultType = resultType;
            ResultMessage = resultMessage;
        }
    }
}
