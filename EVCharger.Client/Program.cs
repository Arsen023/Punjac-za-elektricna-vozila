using System;

namespace EVCharger.Client
{
    internal class Program
    {
        private const int RuleWidth = 58;

        static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (args != null && args.Length > 0
                && string.Equals(args[0], "--demo-phaseC", StringComparison.OrdinalIgnoreCase))
            {
                PrintAppHeader();
                Task4DisposeDemos.RunAllSequential();
            }
            else
            {
                RunMainMenuLoop();
            }

            Console.WriteLine();
            WriteRule();
            Console.WriteLine("Kraj rada programa. Pritisnite bilo koji taster za izlaz.");
            Console.ReadKey(intercept: true);
            return 0;
        }

        private static void RunMainMenuLoop()
        {
            while (true)
            {
                PrintAppHeader();
                WriteRule();
                Console.WriteLine(" Glavni meni");
                WriteRule();
                Console.WriteLine();
                Console.WriteLine("  1   Zadatak 5 — punjenje: CSV → WCF (izbor vozila, pun prenos)");
                Console.WriteLine();
                Console.WriteLine("  2   Zadatak 4 — Dispose: test zatvaranja fajla i WCF konekcije");
                Console.WriteLine();
                Console.WriteLine("  0   Izlaz");
                Console.WriteLine();
                Console.Write("  Vaš izbor (0–2): ");

                string choice = Console.ReadLine()?.Trim();
                Console.WriteLine();

                switch (choice)
                {
                    case "0":
                        return;
                    case "2":
                        Task4DisposeDemos.RunInteractive();
                        break;
                    case "1":
                        Task5ChargingCsvTransfer.RunInteractive();
                        Console.WriteLine();
                        Console.WriteLine("  Pritisnite Enter za povratak u glavni meni…");
                        Console.ReadLine();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  Nevalidan unos — unesite 0, 1 ili 2.");
                        Console.ResetColor();
                        Console.WriteLine();
                        Console.WriteLine("  Pritisnite Enter…");
                        Console.ReadLine();
                        break;
                }
            }
        }

        private static void PrintAppHeader()
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
    }
}
