using System;
using System.ServiceModel;

namespace EVCharger.Common
{
    /// <summary>
    /// Bezbedno zatvaranje WCF kanala/hosta/fabrike (Close uz fallback na Abort).
    /// </summary>
    public static class WcfResourceHelper
    {
        public static void SafeShutdown(ICommunicationObject communicationObject)
        {
            if (communicationObject == null)
            {
                return;
            }

            try
            {
                if (communicationObject.State == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                    return;
                }

                if (communicationObject.State != CommunicationState.Closed)
                {
                    communicationObject.Close(TimeSpan.FromSeconds(5));
                }
            }
            catch (TimeoutException)
            {
                communicationObject.Abort();
            }
            catch (CommunicationException)
            {
                communicationObject.Abort();
            }
            catch
            {
                communicationObject.Abort();
            }
        }
    }
}
