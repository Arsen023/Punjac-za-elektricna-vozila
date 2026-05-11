using System;
using System.IO;
using System.Text;

namespace EVCharger.Client
{
    /// <summary>
    /// Čitanje tekstualnog fajla red po red: drži FileStream i StreamReader i primenjuje Dispose pattern
    /// (priprema za Charging_Profile.csv u sledećoj fazi).
    /// </summary>
    public sealed class DisposableTextFileReader : IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly StreamReader _reader;
        private bool _disposed;

        public DisposableTextFileReader(string path, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Putanja ne sme biti prazna.", nameof(path));
            }

            _fileStream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            _reader = new StreamReader(_fileStream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        }

        public string ReadLine()
        {
            ThrowIfDisposed();
            return _reader.ReadLine();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _reader.Dispose();
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DisposableTextFileReader));
            }
        }
    }
}
