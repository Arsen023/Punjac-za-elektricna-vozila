using System;
using System.IO;
using System.Text;

namespace EVCharger.Client
{
    /// <summary>
    /// Beleženje nevalidnih CSV redova u tekstualni log (zadatak 5).
    /// </summary>
    internal sealed class ClientParseLog : IDisposable
    {
        private readonly StreamWriter _writer;
        private bool _disposed;

        public ClientParseLog(string logFilePath)
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                throw new ArgumentException("Putanja log fajla je obavezna.", nameof(logFilePath));
            }

            string dir = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _writer = new StreamWriter(
                new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };
        }

        public void WriteBadRow(int rowIndex, string reason, string rawLine)
        {
            ThrowIfDisposed();
            const int maxRaw = 800;
            string excerpt = rawLine ?? string.Empty;
            if (excerpt.Length > maxRaw)
            {
                excerpt = excerpt.Substring(0, maxRaw) + "…";
            }

            excerpt = excerpt.Replace("\r", "\\r").Replace("\n", "\\n");
            _writer.WriteLine($"{DateTime.Now:O}\trow={rowIndex}\t{reason}\t{excerpt}");
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _writer.Dispose();
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ClientParseLog));
            }
        }
    }
}
