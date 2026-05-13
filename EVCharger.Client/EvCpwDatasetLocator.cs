using System;
using System.IO;

namespace EVCharger.Client
{
    /// <summary>
    /// Pronalazi koren foldera „EV-CPW Dataset“ (12 vozila) relativno od izvršnog fajla.
    /// </summary>
    internal static class EvCpwDatasetLocator
    {
        internal const string DatasetFolderName = "EV-CPW Dataset";

        internal static string TryFindDatasetRoot()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int depth = 0; depth < 10 && dir != null; depth++)
            {
                var candidate = Path.Combine(dir.FullName, DatasetFolderName);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                dir = dir.Parent;
            }

            return null;
        }
    }
}
