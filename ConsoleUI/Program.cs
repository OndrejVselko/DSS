using Services;
using SimulationCore;
using System.Text.Json;
namespace ConsoleUI
{
    internal class Program
    {
        static public SimulationServices simulationServices { get; set; }
        static public DataServices dataServices { get; set; }
        static async Task Main(string[] args)
        {
            Console.WriteLine("---Disease spreading simulator---");
            simulationServices = new SimulationServices();
            dataServices = new DataServices();
            await MainMenu();
        }
        static async Task MainMenu()
        {
            bool exit = false;
            Console.WriteLine("""
                ---Hlavní Menu---
                Příkazy: 
                [N] - Nová simulace
                [E] - Ukončit aplikaci
                """);
            while (!exit)
            {
                string input = Console.ReadLine()?.ToLower().Trim() ?? String.Empty;
                switch (input)
                {
                    case "e":
                        Console.WriteLine("---Konec---");
                        exit = true;
                        break;
                    case "n":
                        await NewSimulation();
                        break;
                }
            }
        }
        static async Task NewSimulation()
        {
            Console.WriteLine("""
        ---Nová Simulace---
        Zadejte cestu k souboru s regiony: 
        """);
            List<Region> regions = null;
            Disease diseaseMock = new Disease("MockJmeno", 2, 1);
            while (regions == null)
            {
                string input = Console.ReadLine()?.Trim() ?? string.Empty;
                if (input.ToLower() == "zpet")
                    return;
                try
                {
                    regions = await dataServices.LoadRegionsFromJson(input);
                    if (regions.Count == 0)
                    {
                        Console.WriteLine("Soubor je prázdný, zadejte znovu");
                        regions = null;
                    }
                    else
                    {
                        Console.WriteLine($"Načteno {regions.Count} regionů:");
                        foreach (var region in regions)
                            Console.WriteLine($"[{region.id}] {region.name}");                        
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Soubor nebyl nalezen. Zkuste to znovu.");
                }
                catch (JsonException)
                {
                    Console.WriteLine("Soubor má špatný formát. Zkuste jiný soubor.");
                }
            }

            simulationServices.setSimulation(diseaseMock, regions);


            while (true)
            {
                Console.WriteLine("Zadejte číslo počátečního regionu :");
                try
                {
                    simulationServices.setStartingRegion(Console.ReadLine());
                    break;
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message); ;
                }
            }

            await SimulationMenu();
            return;
        }

        static async Task SimulationMenu()
        {
            simulationServices.OnDaySimulated += Console.WriteLine;
            bool exit = false;
            Console.WriteLine("""
                ---Menu simulace---
                Příkazy: 
                [P] - Spustit simulaci
                [S] - Pozastavit simulaci
                """);
            while (!exit)
            {
                string input = Console.ReadLine()?.ToLower().Trim() ?? String.Empty;
                switch (input)
                {
                    case "p":
                        simulationServices.startSimulation();
                        break;
                    case "s":
                        simulationServices.stopSimulation();
                        break;
                    default:
                        Console.WriteLine("Neznámý příkaz");
                        break;
                }
            }
        }
    }
}