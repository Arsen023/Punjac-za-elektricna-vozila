using System;
using System.Runtime.Serialization;

namespace EVCharger.Common
{
    /// <summary>
    /// Jedan red uzorka iz Charging_Profile (telemetrija sesije punjenja).
    /// </summary>
    [DataContract]
    public class ChargingSample
    {
        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public RmsAggregate VoltageRms { get; set; }

        [DataMember]
        public RmsAggregate CurrentRms { get; set; }

        [DataMember]
        public RmsAggregate RealPower { get; set; }

        [DataMember]
        public RmsAggregate ReactivePower { get; set; }

        [DataMember]
        public RmsAggregate ApparentPower { get; set; }

        [DataMember]
        public RmsAggregate Frequency { get; set; }

        [DataMember]
        public int RowIndex { get; set; }

        [DataMember]
        public string VehicleId { get; set; }
    }
}
