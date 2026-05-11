using EVCharger.Common;
using System;
using System.ServiceModel;

namespace EVCharger.Client
{
    /// <summary>
    /// Omotač oko ChannelFactory i proxy-ja sa ispravnim zatvaranjem konekcije (Dispose pattern).
    /// </summary>
    public sealed class EvChargerWcfClient : IDisposable
    {
        private readonly ChannelFactory<IEVChargerService> _factory;
        private IEVChargerService _proxy;
        private bool _disposed;

        public EvChargerWcfClient(string endpointConfigurationName = "EVChargerServiceEndpoint")
        {
            if (string.IsNullOrWhiteSpace(endpointConfigurationName))
            {
                throw new ArgumentException("Ime endpoint konfiguracije je obavezno.", nameof(endpointConfigurationName));
            }

            _factory = new ChannelFactory<IEVChargerService>(endpointConfigurationName);
        }

        public IEVChargerService Channel
        {
            get
            {
                ThrowIfDisposed();

                if (_proxy == null)
                {
                    _proxy = _factory.CreateChannel();
                    var comm = (ICommunicationObject)_proxy;
                    comm.Open();
                }

                return _proxy;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            WcfResourceHelper.SafeShutdown(_proxy as ICommunicationObject);
            _proxy = null;

            WcfResourceHelper.SafeShutdown(_factory as ICommunicationObject);

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EvChargerWcfClient));
            }
        }
    }
}
