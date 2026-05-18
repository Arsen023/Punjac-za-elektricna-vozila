using EVCharger.Common;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace EVCharger.Client
{
    /// <summary>
    /// Zadatak 5: izbor vozila (folder), parsiranje Charging_Profile.csv, slanje redova, log grešaka.
    /// </summary>
    internal static class Task5ChargingCsvTransfer
    {
        private const string CsvFileName = "Charging_Profile.csv";
        private const int RuleWidth = 58;

        internal static void RunInteractive()
        {
            WriteRule();
            Console.WriteLine(" Zadatak 5 — punjenje: CSV (InvariantCulture) → WCF");
            WriteRule();
            Console.WriteLine();

            string datasetRoot = EvCpwDatasetLocator.TryFindDatasetRoot();
            if (datasetRoot == null)
            {
                Console.WriteLine("Greška: nije pronađen folder „" + EvCpwDatasetLocator.DatasetFolderName + "“.");
                Console.WriteLine("        Postavite ga u isti koren kao rešenje ili pokrenite klijent iz bin\\Debug repozitorijuma.");
                return;
            }

            string[] vehicleDirs = Directory.GetDirectories(datasetRoot)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (vehicleDirs.Length == 0)
            {
                Console.WriteLine("Greška: u „" + datasetRoot + "“ nema podfoldera (vozila).");
                return;
            }

            Console.WriteLine("Skup podataka: " + datasetRoot);
            Console.WriteLine();
            Console.WriteLine("Izaberite vozilo (svaki folder sadrži " + CsvFileName + "):");
            Console.WriteLine();
            for (int i = 0; i < vehicleDirs.Length; i++)
            {
                Console.WriteLine($"  {i + 1,2}.  {Path.GetFileName(vehicleDirs[i])}");
            }

            Console.WriteLine();
            Console.Write($"Broj (1–{vehicleDirs.Length}): ");
            string pick = Console.ReadLine();
            if (!int.TryParse(pick?.Trim(), out int idx) || idx < 1 || idx > vehicleDirs.Length)
            {
                Console.WriteLine();
                Console.WriteLine("Nevalidan unos — pokušajte ponovo.");
                return;
            }

            string vehicleFolder = vehicleDirs[idx - 1];
            string vehicleId = Path.GetFileName(vehicleFolder);
            string csvPath = Path.Combine(vehicleFolder, CsvFileName);

            if (!File.Exists(csvPath))
            {
                Console.WriteLine();
                Console.WriteLine("Greška: ne postoji fajl");
                Console.WriteLine("        " + csvPath);
                return;
            }

            string logPath = Path.Combine(vehicleFolder, "Charging_Profile_parse_errors.log");
            Console.WriteLine();
            WriteRule();
            Console.WriteLine(" Sesija");
            WriteRule();
            Console.WriteLine($"  Vozilo (VehicleId):  {vehicleId}");
            Console.WriteLine($"  CSV:                {csvPath}");
            Console.WriteLine($"  Log grešaka:        {logPath}");
            Console.WriteLine();
            Console.WriteLine("  Otvaranje WCF sesije i slanje uzoraka…");
            Console.WriteLine();

            int sent = 0;
            int skipped = 0;

            try
            {
                using (var parseLog = new ClientParseLog(logPath))
                using (var client = new EvChargerWcfClient())
                {
                    client.Channel.StartSession(vehicleId);

                    using (var reader = new DisposableTextFileReader(csvPath))
                    {
                        string header = reader.ReadLine();
                        if (header == null)
                        {
                            Console.WriteLine("Greška: fajl je prazan.");
                            return;
                        }

                        if (!ChargingProfileCsvParser.LooksLikeChargingProfileHeader(header))
                        {
                            parseLog.WriteBadRow(0, "Neočekivano zaglavlje CSV-a.", header);
                            Console.WriteLine("  Upozorenje: zaglavlje ne liči na Charging_Profile — nastavljam parsiranje.");
                            Console.WriteLine();
                        }

                        int dataRowIndex = 0;
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            dataRowIndex++;
                            if (ChargingProfileCsvParser.TryParseDataLine(line, dataRowIndex, vehicleId, out ChargingSample sample, out string err))
                            {
                                try
                                {
                                    client.Channel.PushSample(sample);
                                    sent++;
                                }
                                catch (FaultException<ChargingValidationFault> fex)
                                {
                                    skipped++;
                                    parseLog.WriteBadRow(
                                        dataRowIndex,
                                        "WCF validacija: " + fex.Detail.Message,
                                        line);
                                }
                            }
                            else
                            {
                                skipped++;
                                parseLog.WriteBadRow(dataRowIndex, err, line);
                            }
                        }
                    }

                    client.Channel.EndSession(vehicleId);
                }

                Console.WriteLine();
                WriteRule();
                Console.WriteLine(" Rezultat");
                WriteRule();
                Console.WriteLine($"  Poslato na server:     {sent,5}  uzoraka");
                Console.WriteLine($"  Preskočeno (greška):   {skipped,5}  redova  (detalji u logu)");
                Console.WriteLine();
                if (skipped > 0)
                {
                    Console.WriteLine("  Napomena: preskočeni redovi su u log fajlu");
                }

                Console.WriteLine();
                Console.WriteLine("  WCF sesija je uredno zatvorena (EndSession).");
            }
            catch (FaultException<ChargingValidationFault> ex)
            {
                Console.WriteLine();
                Console.WriteLine("WCF greška (fault): " + ex.Detail.Message);
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine();
                Console.WriteLine("Greška komunikacije (da li je server pokrenut na net.tcp:4000?):");
                Console.WriteLine("  " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Neočekivana greška: " + ex.GetType().Name);
                Console.WriteLine("  " + ex.Message);
            }
        }

        private static void WriteRule()
        {
            Console.WriteLine(new string('-', RuleWidth));
        }
    }
}
