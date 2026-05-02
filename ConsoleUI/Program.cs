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
            Console.WriteLine("""
            ---Nová Simulace---
            Zadejte cestu k souboru s daty: 
            """);

            appService.SetSimulation();
            await LoadData();
            await CreateOrLoadDisease();
            await SetDiseaseAbilities();
            await SetStartingRegion();
            await ShowRegionsForEdit();
            

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
        }

        static async Task SetDiseaseAbilities()
        {
            Dictionary<int, DiseaseAbility> availableAbilities = appService.GetAvailableDiseaseAbilities();
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
        
        static async Task SetStartingRegion()
        {
            Dictionary<int, Region> regions = appService.GetAllRegions();
            foreach (int key in regions.Keys)
            {
                Console.WriteLine($"[{key}] {regions[key].Name}");
            }

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
        }

        static async Task ShowRegionsForEdit()
        {

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
                [4] - Upravit vlastnosti nemoce
                [9] - Breakpoint
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
                    case "4":
                        await SetDiseaseAbilities();
                        break;
                    case "9":
                        Console.WriteLine("Breakpoint");
                        break;
                    default:
                        Console.WriteLine("Neznámý příkaz");
                        break;
                }
            }
        }

        static async Task LoadData()
        {
            while (true)
            {
                try
                {
                    string input = Console.ReadLine()?.Trim() ?? string.Empty;
                    await appService.LoadData(input);
                    break;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Soubor nebyl nalezen. Zkuste to znovu.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Chyba při načítání: {e.Message}");
                }
            }
        }

    }
}