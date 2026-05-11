using System.Runtime.Serialization;

namespace EVCharger.Common
{
    /// <summary>
    /// RMS statistika u tri tačke (Min, Avg, Max) za napon, struju, snage ili frekvenciju.
    /// </summary>
    [DataContract]
    public class RmsAggregate
    {
        [DataMember]
        public double Min { get; set; }

        [DataMember]
        public double Avg { get; set; }

        [DataMember]
        public double Max { get; set; }
    }
}
