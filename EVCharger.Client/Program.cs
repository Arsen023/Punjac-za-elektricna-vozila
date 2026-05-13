using EVCharger.Common;
using System;
using System.IO;
using System.ServiceModel;

namespace EVCharger.Client
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(new string('=', 58));
            Console.WriteLine(" EVCharger — klijentska aplikacija");
            Console.WriteLine(new string('=', 58));
            Console.WriteLine();

            if (args != null && args.Length > 0 && string.Equals(args[0], "--demo-phaseC", StringComparison.OrdinalIgnoreCase))
            {
                RunPhaseCDemos();
            }
            else
            {
                Console.WriteLine("Glavni meni:");
                Console.WriteLine();
                Console.WriteLine("  1  Citanje CSV fajlova");
                Console.WriteLine();
                Console.WriteLine("  2  Simulacija : Dispose, prekid / uredan završetak WCF");
                Console.WriteLine();
                Console.Write("Vaš izbor (1 ili 2): ");
                string c = Console.ReadLine();
                if (string.Equals(c?.Trim(), "2", StringComparison.Ordinal))
                {
                    RunPhaseCDemos();
                }
                else
                {
                    Task5ChargingCsvTransfer.RunInteractive();
                }
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 58));
            Console.WriteLine("Kraj rada programa. Pritisnite bilo koji taster za izlaz.");
            Console.ReadKey(intercept: true);
            return 0;
        }

        private static void RunPhaseCDemos()
        {
            Console.WriteLine("Faza C — Dispose: FileStream/StreamReader i WCF kanal.");
            Console.WriteLine();

            DemoEarlyDisposeFileReader();

            DemoWcfInterruptedTransfer();

            DemoWcfNormalShutdown();
        }

        /// <summary>
        /// Simulacija prekida: čita se deo fajla, zatim se resursi oslobađaju pre kraja sadržaja.
        /// </summary>
        private static void DemoEarlyDisposeFileReader()
        {
            string path = Path.Combine(Path.GetTempPath(), "evcharger_phaseC_demo.txt");
            File.WriteAllText(path, "linija1\r\nlinija2\r\nlinija3\r\n");

            Console.WriteLine("[1] DisposableTextFileReader — rani Dispose (prekidan čitanje)");
            try
            {
                using (var reader = new DisposableTextFileReader(path))
                {
                    string first = reader.ReadLine();
                    Console.WriteLine($"    Pročitan prvi red: {first}");
                }

                Console.WriteLine("    StreamReader i FileStream su zatvoreni u Dispose().");
            }
            catch (Exception ex)
            {
                Console.WriteLine("    Greška: " + ex.Message);
            }
            finally
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    // privremeni fajl — ignoriši
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Simulacija prekida mrežnog prenosa: sesija se otvara, kanal se gasi bez EndSession.
        /// </summary>
        private static void DemoWcfInterruptedTransfer()
        {
            Console.WriteLine("[2] WCF — prekid prenosa (Dispose klijenta bez EndSession)");
            try
            {
                using (var client = new EvChargerWcfClient())
                {
                    client.Channel.StartSession("SIM-INTERRUPT");
                    Console.WriteLine("    StartSession poslat; kanal se zatvara bez EndSession.");
                }

                Console.WriteLine("    Kanal i fabrika su zatvoreni (SafeShutdown).");
            }
            catch (FaultException<ChargingValidationFault> ex)
            {
                Console.WriteLine("    Fault: " + ex.Detail.Message);
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine("    (Servis verovatno nije pokrenut) " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("    " + ex.GetType().Name + ": " + ex.Message);
            }

            Console.WriteLine();
        }

        private static void DemoWcfNormalShutdown()
        {
            Console.WriteLine("[3] WCF — uredan završetak (StartSession + EndSession + Dispose)");
            try
            {
                using (var client = new EvChargerWcfClient())
                {
                    client.Channel.StartSession("SIM-OK");
                    client.Channel.EndSession("SIM-OK");
                    Console.WriteLine("    StartSession i EndSession uspešno.");
                }

                Console.WriteLine("    Resursi klijenta oslobođeni.");
            }
            catch (FaultException<ChargingValidationFault> ex)
            {
                Console.WriteLine("    Fault: " + ex.Detail.Message);
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine("    (Servis verovatno nije pokrenut) " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("    " + ex.GetType().Name + ": " + ex.Message);
            }
        }
    }
}
