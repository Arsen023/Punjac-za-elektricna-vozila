using EVCharger.Common;
using System;
using System.IO;
using System.ServiceModel;

namespace EVCharger.Client
{
    /// <summary>
    /// Zadatak 4: Dispose pattern — FileStream, StreamReader, WCF kanal; simulacija prekida i urednog završetka.
    /// </summary>
    internal static class Task4DisposeDemos
    {
        private const int RuleWidth = 58;

        internal static void RunInteractive()
        {
            while (true)
            {
                WriteBanner();
                WriteRule();
                Console.WriteLine(" Zadatak 4 — Dispose pattern (demo)");
                WriteRule();
                Console.WriteLine();
       
                Console.WriteLine();
                Console.WriteLine("  1   Fajl     — prekid čitanja (FileStream + StreamReader)");
                Console.WriteLine("  2   WCF      — prekid prenosa (StartSession, BEZ EndSession)");
                Console.WriteLine("  3   WCF      — uredan završetak (StartSession + EndSession)");
                Console.WriteLine("  4   Sve      — pokreni demo 1 → 2 → 3 redom");
                Console.WriteLine();
                Console.WriteLine("  0   Nazad na glavni meni");
                Console.WriteLine();
                Console.Write("  Vaš izbor (0–4): ");

                string choice = Console.ReadLine()?.Trim();
                Console.WriteLine();

                switch (choice)
                {
                    case "0":
                        return;
                    case "1":
                        RunFileInterruptDemo(interactive: true);
                        break;
                    case "2":
                        RunWcfInterruptDemo(interactive: true);
                        break;
                    case "3":
                        RunWcfGracefulDemo(interactive: true);
                        break;
                    case "4":
                        RunAllSequential();
                        break;
                    default:
                        WriteWarning("  Nevalidan unos — unesite broj od 0 do 4.");
                        break;
                }

                if (choice != "0")
                {
                    Console.WriteLine();
                    WriteMuted("  Pritisnite Enter za povratak u meni Zadatka 4…");
                    Console.ReadLine();
                    Console.WriteLine();
                }
            }
        }

        internal static void RunAllSequential()
        {
            WriteBanner();
            WriteRule();
            Console.WriteLine(" Zadatak 4 — sva tri demo-a redom");
            WriteRule();
            Console.WriteLine();

            RunFileInterruptDemo(interactive: false);
            PauseBetweenDemos();

            RunWcfInterruptDemo(interactive: false);
            PauseBetweenDemos();

            RunWcfGracefulDemo(interactive: false);

            Console.WriteLine();
            WriteSuccess("  Svi demo-i završeni.");
        }

        private static void RunFileInterruptDemo(bool interactive)
        {
            if (interactive)
            {
                WriteDemoHeader(
                    "Demo 1 / 3",
                    "Prekid čitanja fajla",
                    "Čita se samo prvi red iz privremenog fajla, zatim using poziva Dispose().");
            }

            string path = Path.Combine(Path.GetTempPath(), "evcharger_zadatak4_demo.txt");
            File.WriteAllText(path, "linija1\r\nlinija2\r\nlinija3\r\n");

            WriteStep("Otvaranje DisposableTextFileReader (FileStream + StreamReader)…");
            try
            {
                using (var reader = new DisposableTextFileReader(path))
                {
                    string first = reader.ReadLine();
                    WriteInfo($"  Pročitan 1. red: \"{first}\"");
                    WriteMuted("  Linije 2 i 3 namerno NISU pročitane (simulacija prekida).");
                }

                WriteSuccess("  [OK] Dispose zatvorio StreamReader i FileStream.");
                WriteMuted("  Isti mehanizam koristi meni 1 pri čitanju Charging_Profile.csv.");
            }
            catch (Exception ex)
            {
                WriteError("  [GREŠKA] " + ex.Message);
            }
            finally
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    // privremeni fajl
                }
            }
        }

        private static void RunWcfInterruptDemo(bool interactive)
        {
            if (interactive)
            {
                WriteDemoHeader(
                    "Demo 2 / 3",
                    "Prekid WCF prenosa",
                    "StartSession se šalje, ali EndSession NE — zatim se klijent gasi (Dispose).");
            }

            WriteStep("Otvaranje EvChargerWcfClient i slanje StartSession(\"SIM-INTERRUPT\")…");
            WriteWarning("  EndSession se NE poziva (namerni prekid).");

            try
            {
                using (var client = new EvChargerWcfClient())
                {
                    client.Channel.StartSession("SIM-INTERRUPT");
                    WriteInfo("  StartSession uspešno poslat.");
                    WriteMuted("  Izlazak iz using → Dispose() zatvara kanal i fabriku.");
                }

                WriteSuccess("  [OK] WCF kanal i ChannelFactory zatvoreni (SafeShutdown).");
                WriteMuted("  Na serveru: videćete StartSession bez EndSession za SIM-INTERRUPT.");
            }
            catch (FaultException<ChargingValidationFault> ex)
            {
                WriteError("  [FAULT] " + ex.Detail.Message);
            }
            catch (CommunicationException ex)
            {
                WriteError("  [KOMUNIKACIJA] Server verovatno nije pokrenut.");
                WriteMuted("  " + ex.Message);
            }
            catch (Exception ex)
            {
                WriteError("  [" + ex.GetType().Name + "] " + ex.Message);
            }
        }

        private static void RunWcfGracefulDemo(bool interactive)
        {
            if (interactive)
            {
                WriteDemoHeader(
                    "Demo 3 / 3",
                    "Uredan završetak WCF sesije",
                    "StartSession + EndSession, pa Dispose — ispravan protokol kao u meniju 1.");
            }

            WriteStep("Otvaranje EvChargerWcfClient…");

            try
            {
                using (var client = new EvChargerWcfClient())
                {
                    client.Channel.StartSession("SIM-OK");
                    WriteInfo("  StartSession(\"SIM-OK\") poslat.");
                    client.Channel.EndSession("SIM-OK");
                    WriteInfo("  EndSession(\"SIM-OK\") poslat.");
                }

                WriteSuccess("  [OK] Sesija uredno zatvorena, resursi klijenta oslobođeni.");
                WriteMuted("  Na serveru: StartSession i EndSession za SIM-OK.");
            }
            catch (FaultException<ChargingValidationFault> ex)
            {
                WriteError("  [FAULT] " + ex.Detail.Message);
            }
            catch (CommunicationException ex)
            {
                WriteError("  [KOMUNIKACIJA] Server verovatno nije pokrenut.");
                WriteMuted("  " + ex.Message);
            }
            catch (Exception ex)
            {
                WriteError("  [" + ex.GetType().Name + "] " + ex.Message);
            }
        }

        private static void WriteDemoHeader(string label, string title, string description)
        {
            WriteRule();
            Console.WriteLine($"  {label}  |  {title}");
            WriteRule();
            WriteMuted("  " + description);
            Console.WriteLine();
        }

        private static void PauseBetweenDemos()
        {
            Console.WriteLine();
            WriteMuted("  --- sledeći demo za 2 sekunde ---");
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine();
        }

        private static void WriteBanner()
        {
            Console.Clear();
            Console.WriteLine(new string('=', RuleWidth));
            Console.WriteLine(" EVCharger — klijentska aplikacija");
            Console.WriteLine(new string('=', RuleWidth));
            Console.WriteLine();
        }

        private static void WriteRule()
        {
            Console.WriteLine(new string('-', RuleWidth));
        }

        private static void WriteStep(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteInfo(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteSuccess(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteMuted(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
