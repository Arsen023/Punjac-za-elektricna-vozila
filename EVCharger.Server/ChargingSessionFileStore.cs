using EVCharger.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace EVCharger.Server
{
    /// <summary>
    /// Zadatak 6: Data/&lt;VehicleId&gt;/&lt;YYYY-MM-DD&gt;/session.csv i rejects.csv.
    /// </summary>
    internal static class ChargingSessionFileStore
    {
        /// <summary>Excel na Windowsu zahteva UTF-8 BOM da bi prikazao ć, č, š, ž, đ.</summary>
        private static readonly Encoding Utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private static readonly object Gate = new object();
        private static readonly Dictionary<string, SessionPaths> ActiveSessions =
            new Dictionary<string, SessionPaths>(StringComparer.OrdinalIgnoreCase);

        internal static string StartSession(string vehicleId)
        {
            lock (Gate)
            {
                SessionPaths paths = EnsureSessionPaths(vehicleId, DateTime.Now);
                EnsureCsvHeader(paths.SessionCsvPath, ChargingSessionCsvFormatter.SessionHeader);
                EnsureCsvHeader(paths.RejectsCsvPath, ChargingSessionCsvFormatter.RejectsHeader);
                ActiveSessions[vehicleId] = paths;
                return paths.SessionCsvPath;
            }
        }

        internal static void AppendSample(ChargingSample sample)
        {
            lock (Gate)
            {
                SessionPaths paths = ResolvePathsForSample(sample);
                EnsureCsvHeader(paths.SessionCsvPath, ChargingSessionCsvFormatter.SessionHeader);
                AppendLine(paths.SessionCsvPath, ChargingSessionCsvFormatter.FormatSampleLine(sample));
            }
        }

        internal static void LogReject(ChargingValidationFault fault)
        {
            if (fault == null)
            {
                return;
            }

            lock (Gate)
            {
                SessionPaths paths = ResolvePathsForReject(fault);
                EnsureCsvHeader(paths.RejectsCsvPath, ChargingSessionCsvFormatter.RejectsHeader);
                AppendLine(paths.RejectsCsvPath, ChargingSessionCsvFormatter.FormatRejectLine(fault));
            }
        }

        internal static void EndSession(string vehicleId)
        {
            lock (Gate)
            {
                ActiveSessions.Remove(vehicleId);
            }
        }

        private static SessionPaths ResolvePathsForSample(ChargingSample sample)
        {
            if (sample != null
                && !string.IsNullOrWhiteSpace(sample.VehicleId)
                && ActiveSessions.TryGetValue(sample.VehicleId, out SessionPaths active))
            {
                return active;
            }

            string vehicleId = sample?.VehicleId;
            DateTime day = sample?.Timestamp != default(DateTime) ? sample.Timestamp.Date : DateTime.Now.Date;
            return EnsureSessionPaths(vehicleId, day);
        }

        private static SessionPaths ResolvePathsForReject(ChargingValidationFault fault)
        {
            if (!string.IsNullOrWhiteSpace(fault.VehicleId)
                && ActiveSessions.TryGetValue(fault.VehicleId, out SessionPaths active))
            {
                return active;
            }

            return EnsureSessionPaths(fault.VehicleId, DateTime.Now);
        }

        private static SessionPaths EnsureSessionPaths(string vehicleId, DateTime day)
        {
            string safeVehicleId = SanitizePathSegment(
                string.IsNullOrWhiteSpace(vehicleId) ? "_unknown" : vehicleId.Trim());
            string dateFolder = day.ToString("yyyy-MM-dd");
            string directory = Path.Combine(GetDataRoot(), safeVehicleId, dateFolder);
            Directory.CreateDirectory(directory);

            return new SessionPaths
            {
                DirectoryPath = directory,
                SessionCsvPath = Path.Combine(directory, "session.csv"),
                RejectsCsvPath = Path.Combine(directory, "rejects.csv")
            };
        }

        private static string GetDataRoot()
        {
            string configured = ConfigurationManager.AppSettings["ChargingDataRoot"];
            string folder = string.IsNullOrWhiteSpace(configured) ? "Data" : configured.Trim();
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);
        }

        private static void EnsureCsvHeader(string filePath, string headerLine)
        {
            if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var writer = new StreamWriter(filePath, append: false, Utf8WithBom))
            {
                writer.WriteLine(headerLine);
            }
        }

        private static void AppendLine(string filePath, string line)
        {
            using (var stream = new FileStream(
                filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read))
            using (var writer = new StreamWriter(stream, Utf8NoBom))
            {
                writer.WriteLine(line);
            }
        }

        private static string SanitizePathSegment(string segment)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            var chars = segment.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
            return new string(chars).Trim();
        }

        private sealed class SessionPaths
        {
            internal string DirectoryPath { get; set; }
            internal string SessionCsvPath { get; set; }
            internal string RejectsCsvPath { get; set; }
        }
    }
}
