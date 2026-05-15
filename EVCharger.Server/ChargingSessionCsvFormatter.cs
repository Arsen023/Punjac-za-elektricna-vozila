using EVCharger.Common;
using System;
using System.Globalization;
using System.Text;

namespace EVCharger.Server
{
    /// <summary>
    /// CSV format usklađen sa Charging_Profile.csv (InvariantCulture).
    /// </summary>
    internal static class ChargingSessionCsvFormatter
    {
        internal const string SessionHeader =
            "Date Time,Voltage RMS Min (V),Voltage RMS Avg (V),Voltage RMS Max (V)," +
            "Current RMS Min (A),Current RMS Avg (A),Current RMS Max (A)," +
            "Real Power Min (kW),Real Power Avg (kW),Real Power Max (kW)," +
            "Reactive Power Min (kVAR),Reactive Power Avg (kVAR),Reactive Power Max (kVAR)," +
            "Apparent Power Min (kVA),Apparent Power Avg (kVA),Apparent Power Max (kVA)," +
            "Frequency Min (Hz),Frequency Avg (Hz),Frequency Max (Hz)";

        internal const string RejectsHeader = "UtcTimestamp,VehicleId,RowIndex,Reason";

        internal static string FormatSampleLine(ChargingSample sample)
        {
            var sb = new StringBuilder();
            sb.Append(sample.Timestamp.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
            AppendAggregate(sb, sample.VoltageRms);
            AppendAggregate(sb, sample.CurrentRms);
            AppendAggregate(sb, sample.RealPower);
            AppendAggregate(sb, sample.ReactivePower);
            AppendAggregate(sb, sample.ApparentPower);
            AppendAggregate(sb, sample.Frequency);
            return sb.ToString();
        }

        internal static string FormatRejectLine(ChargingValidationFault fault)
        {
            string vehicle = fault.VehicleId ?? string.Empty;
            string reason = (fault.Message ?? string.Empty).Replace("\"", "\"\"");
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:O},\"{1}\",{2},\"{3}\"",
                DateTime.UtcNow,
                vehicle.Replace("\"", "\"\""),
                fault.RowIndex,
                reason);
        }

        private static void AppendAggregate(StringBuilder sb, RmsAggregate aggregate)
        {
            sb.Append(',');
            sb.Append(aggregate.Min.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(aggregate.Avg.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(aggregate.Max.ToString(CultureInfo.InvariantCulture));
        }
    }
}
