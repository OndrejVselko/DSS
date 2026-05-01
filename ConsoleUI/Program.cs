using Services;
using SimulationCore;
using System.Text.Json;

namespace ConsoleUI
{
    internal class Program
    {
        static public AppService appService { get; set; }

        static async Task Main(string[] args)
        {
            Console.WriteLine("---Disease spreading simulator---");
            appService = new AppService();
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
                string input = Console.ReadLine()?.ToLower().Trim() ?? string.Empty;
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
            appService.SetSimulation();
            await CreateOrLoadDisease();

            Console.WriteLine("""
                ---Nová Simulace---
                Zadejte cestu k souboru s regiony: 
                """);
            await SetRegions();   

            while (true)
            {
                Console.WriteLine("Zadejte číslo počátečního regionu:");
                try
                {
                    appService.SetStartingRegion(Console.ReadLine());
                    break;
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            await SimulationMenu();
        }

        static async Task CreateOrLoadDisease()
        {
            Console.WriteLine("""
                --- Nastavení nemoci ---
                [1] - Vytvořit novou nemoc
                [2] - Načíst nemoc (není implementováno)
                [0] - Návrat do menu
                """);

            while (true)
            {
                string input = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (input == "0") return;
                else if (input == "1") { await CreateDisease(); break; }
                else if (input == "2") { Console.WriteLine("Funkce načítání bude doplněna později..."); return; }
                else Console.WriteLine("Neznámý příkaz");
            }
        }

        static async Task CreateDisease()
        {
            Console.Write("Název nemoci: ");
            string name = Console.ReadLine() ?? "Neznámá nemoc";

            Console.Write("Základní rychlost šíření (např. 1,2): ");
            double.TryParse(Console.ReadLine(), out double speed);

            Console.Write("Šance na smrt (0-1): ");
            double.TryParse(Console.ReadLine(), out double deathProbability);

            Console.Write("Délka onemocnění: ");
            int.TryParse(Console.ReadLine(), out int length);

            while (true)
            {
                try
                {
                    appService.SetDisease(name, speed, deathProbability, length);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Zadejte hodnoty znovu");
                }
            }

            Console.WriteLine("Zadejte cestu k souboru s vlastnostmi nemoci (JSON):");
            string path = Console.ReadLine() ?? "";

            try
            {
                Dictionary<int, DiseaseAbility> availableAbilities = await appService.LoadDiseaseAbilities(path);
                List<int> selectedIds = new List<int>();

                bool adding = true;
                while (adding)
                {
                    Console.WriteLine("\n--- Dostupné vlastnosti (Zadejte ID pro přidání, 0 pro dokončení) ---");
                    foreach (var ab in availableAbilities.Values)
                    {
                        string status = selectedIds.Contains(ab.Id) ? "[VYBRÁNO]" : "";
                        Console.WriteLine($"{status} [{ab.Id}] {ab.Name} (Mod: {ab.PrimaryModifier})");
                    }

                    if (int.TryParse(Console.ReadLine(), out int id))
                    {
                        if (id == 0)
                        {
                            adding = false;
                        }
                        else if (!availableAbilities.ContainsKey(id))
                        {
                            Console.WriteLine("Neznámé ID.");
                        }
                        else if (selectedIds.Contains(id))
                        {
                            appService.RemoveDiseaseAbilityFromDisease(id);
                            selectedIds.Remove(id);
                            Console.WriteLine($"Odebráno: {availableAbilities[id].Name}");
                        }
                        else
                        {
                            appService.AddDiseaseAbilityToDisease(id);
                            selectedIds.Add(id);
                            Console.WriteLine($"Přidáno: {availableAbilities[id].Name}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Nepodařilo se načíst vlastnosti: {e.Message}");
            }
        }

        static async Task SimulationMenu()
        {
            appService.OnDaySimulated += Console.WriteLine;
            bool exit = false;
            Console.WriteLine("""
                ---Menu simulace---
                Příkazy: 
                [0] - Spustit simulaci
                [1] - Pozastavit simulaci
                [2] - Změnit rychlost šíření
                [3] - Změnit šanci na smrt
                """);
            while (!exit)
            {
                string input = Console.ReadLine()?.ToLower().Trim() ?? string.Empty;
                switch (input)
                {
                    case "0":
                        appService.StartSimulation();
                        break;
                    case "1":
                        appService.StopSimulation();
                        break;
                    case "2":
                        Console.WriteLine("Zadejte hodnotu: ");
                        appService.ChangeDefaultSpreadingSpeed(Console.ReadLine());
                        break;
                    case "3":
                        appService.ChangeDeathProbability(Console.ReadLine());
                        break;
                    default:
                        Console.WriteLine("Neznámý příkaz");
                        break;
                }
            }
        }

        static async Task SetRegions()
        {
            List<Region>? regions = null;
            while (regions == null)
            {
                string input = Console.ReadLine()?.Trim() ?? string.Empty;
                if (input.ToLower() == "zpet")
                    return;
                try
                {
                    regions = await appService.LoadRegionsFromJson(input); 
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
        }
    }
}