using EVCharger.Common;
using System;
using System.Globalization;
using System.Linq;

namespace EVCharger.Client
{
    /// <summary>
    /// Parsiranje jednog reda Charging_Profile.csv (InvariantCulture).
    /// </summary>
    internal static class ChargingProfileCsvParser
    {
        private const int ExpectedColumnCount = 19;

        private static readonly NumberStyles DoubleStyle =
            NumberStyles.Float | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

        /// <summary>
        /// Prvi red fajla (zaglavlje) — za brzu proveru.
        /// </summary>
        internal static bool LooksLikeChargingProfileHeader(string headerLine)
        {
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return false;
            }

            string t = headerLine.TrimStart();
            return t.StartsWith("Date Time", StringComparison.Ordinal)
                && t.IndexOf("Voltage RMS Min", StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// Parsira jedan red podataka (ne zaglavlje). dataRowIndex je 1-based indeks reda podataka u fajlu.
        /// </summary>
        internal static bool TryParseDataLine(
            string line,
            int dataRowIndex,
            string vehicleId,
            out ChargingSample sample,
            out string errorMessage)
        {
            sample = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(line))
            {
                errorMessage = "Prazan red.";
                return false;
            }

            string[] parts = line.Split(',');
            if (parts.Length != ExpectedColumnCount)
            {
                errorMessage = $"Očekivano {ExpectedColumnCount} kolona, dobijeno {parts.Length}.";
                return false;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            if (!TryParseDateTime(parts[0], out DateTime timestamp))
            {
                errorMessage = "Nevalidan Date Time.";
                return false;
            }

            if (!TryParseAggregate(parts, 1, out RmsAggregate voltage, out string vErr))
            {
                errorMessage = "Voltage RMS: " + vErr;
                return false;
            }

            if (!TryParseAggregate(parts, 4, out RmsAggregate current, out string cErr))
            {
                errorMessage = "Current RMS: " + cErr;
                return false;
            }

            if (!TryParseAggregate(parts, 7, out RmsAggregate realP, out string rpErr))
            {
                errorMessage = "Real Power: " + rpErr;
                return false;
            }

            if (!TryParseAggregate(parts, 10, out RmsAggregate reactP, out string rqErr))
            {
                errorMessage = "Reactive Power: " + rqErr;
                return false;
            }

            if (!TryParseAggregate(parts, 13, out RmsAggregate appP, out string apErr))
            {
                errorMessage = "Apparent Power: " + apErr;
                return false;
            }

            if (!TryParseAggregate(parts, 16, out RmsAggregate freq, out string fErr))
            {
                errorMessage = "Frequency: " + fErr;
                return false;
            }

            sample = new ChargingSample
            {
                Timestamp = timestamp,
                VoltageRms = voltage,
                CurrentRms = current,
                RealPower = realP,
                ReactivePower = reactP,
                ApparentPower = appP,
                Frequency = freq,
                RowIndex = dataRowIndex,
                VehicleId = vehicleId
            };

            return true;
        }

        private static bool TryParseDateTime(string s, out DateTime value)
        {
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out value))
            {
                return true;
            }

            string[] formats =
            {
                "yyyy/MM/dd HH:mm:ss",
                "yyyy/M/d H:mm:ss",
                "yyyy/MM/dd H:mm:ss"
            };

            return DateTime.TryParseExact(
                s,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out value);
        }

        private static bool TryParseAggregate(string[] parts, int startIndex, out RmsAggregate agg, out string err)
        {
            agg = null;
            err = null;

            if (!TryParseDouble(parts[startIndex], out double min))
            {
                err = "Min nije broj.";
                return false;
            }

            if (!TryParseDouble(parts[startIndex + 1], out double avg))
            {
                err = "Avg nije broj.";
                return false;
            }

            if (!TryParseDouble(parts[startIndex + 2], out double max))
            {
                err = "Max nije broj.";
                return false;
            }

            agg = new RmsAggregate { Min = min, Avg = avg, Max = max };
            return true;
        }

        private static bool TryParseDouble(string s, out double value)
        {
            return double.TryParse(s, DoubleStyle, CultureInfo.InvariantCulture, out value);
        }
    }
}
