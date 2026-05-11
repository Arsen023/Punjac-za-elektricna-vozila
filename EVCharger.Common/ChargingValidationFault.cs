using System.Runtime.Serialization;

namespace EVCharger.Common
{
    /// <summary>
    /// Detalj greške pri validaciji (FaultContract).
    /// </summary>
    [DataContract]
    public class ChargingValidationFault
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string VehicleId { get; set; }

        /// <summary>
        /// Indeks reda u CSV-u; -1 ako se greška odnosi na StartSession/EndSession.
        /// </summary>
        [DataMember]
        public int RowIndex { get; set; }
    }
}
