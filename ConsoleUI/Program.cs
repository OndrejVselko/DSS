using Services;
using SimulationCore;
using System.Diagnostics;
using System.Text.Json;
using Shared;

namespace ConsoleUI
{
    /// <summary>
    /// Console UI entry point and menus for the disease spreading simulator.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Central application service used by the UI (single instance).
        /// </summary>
        static public AppService appService { get; set; }

        /// <summary>
        /// Program entry point; initializes the AppService and shows the main menu.
        /// </summary>
        static async Task Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("---Disease spreading simulator---\n");
            appService = new AppService();
            await MainMenu();
        }

        /// <summary>
        /// Shows the main menu and handles top-level commands.
        /// </summary>
        static async Task MainMenu()
        {
            bool exit = false;
            Console.WriteLine("""
                ---Hlavní Menu---
                Příkazy: 
                [1] - Nová simulace
                [0] - Ukončit aplikaci
                """);
            while (!exit)
            {
                string input = Console.ReadLine()?.ToLower().Trim() ?? string.Empty;
                switch (input)
                {
                    case "0":
                        Console.WriteLine("---Konec---");
                        exit = true;
                        break;
                    case "1":
                        await NewSimulation();
                        break;
                }
            }
        }

        /// <summary>
        /// Walks through creating a new simulation (load data, disease, region).
        /// </summary>
        static async Task NewSimulation()
        {
            Console.Clear();
            Console.WriteLine("""
            ---Vytvoření Nové Simulace---
            Zadejte cestu k souboru s daty: 
            """);

            appService.SetSimulation();
            await LoadData();
            await CreateOrLoadDisease();
            await SetDiseaseAbilities();
            await SetStartingRegion();
            
            await SimulationMenu();
        }

        /// <summary>
        /// Lets the user create a new disease or (future) load one.
        /// </summary>
        static async Task CreateOrLoadDisease()
        {
            Console.Clear();
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

        /// <summary>
        /// Prompts the user for disease parameters and sets the disease in AppService.
        /// </summary>
        static async Task CreateDisease()
        {
            Console.Clear();
            Console.WriteLine("--- Vytvoření nové nemoci ---");
            Console.Write("Název nemoci: ");
            string name = Console.ReadLine() ?? "Neznámá nemoc";

            Console.Write("Základní rychlost šíření (např. 1,2): ");
            double.TryParse(Console.ReadLine(), out double speed);

            Console.Write("Šance na smrt (0-1): ");
            double.TryParse(Console.ReadLine(), out double deathProbability);

            Console.Write("Délka onemocnění (dny): ");
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

        /// <summary>
        /// Shows available disease abilities and allows adding/removing them for the current disease.
        /// </summary>
        static async Task SetDiseaseAbilities()
        {
            Console.Clear();
            Console.WriteLine("--- Nastavení vlastností nemoci ---");

            Dictionary<int, DiseaseAbility> availableAbilities = appService.GetAvailableDiseaseAbilities();
            List<int> selectedIds = new List<int>();

            bool adding = true;
            while (adding)
            {
                Console.WriteLine("\n--- Dostupné vlastnosti (Zadejte ID pro přidání, 0 pro dokončení) ---");
                foreach (var ab in availableAbilities.Values)
                {
                    string status = selectedIds.Contains(ab.Id) ? "[VYBRÁNO]" : "";
                    Console.WriteLine($"{status} [{ab.Id}] {ab.Name} (Mod: {ab.SpreadingModifier})");
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
        
        /// <summary>
        /// Prompts user to pick the starting region for the simulation.
        /// </summary>
        static async Task SetStartingRegion()
        {
            Console.Clear();
            Console.WriteLine("--- Výběr počátečního regionu ---");
            await ShowRegions();
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

        /// <summary>
        /// Writes the list of regions to the console.
        /// </summary>
        static async Task ShowRegions()
        {
            Dictionary<int, Region> regions = appService.GetAllRegions();
            foreach (int key in regions.Keys)
            {
                Console.WriteLine($"[{key}] {regions[key].Name}");
            }
        }

        /// <summary>
        /// Lets the user add/remove abilities for a specific region.
        /// </summary>
        static async Task SetRegionAbilities(int regionId)
        {
            Console.Clear();
            Console.WriteLine("--- Nastavení vlastností regionu ---");

            Dictionary<int, RegionAbility> availableAbilities = appService.GetAvailableRegionAbilities();

            Region region = appService.GetRegion(regionId);
            List<int> selectedIds = region.Abilities.Select(a => a.Id).ToList();

            bool adding = true;
            while (adding)
            {
                Console.WriteLine("\n--- Dostupné vlastnosti regionu (Zadejte ID pro přidání, 0 pro dokončení) ---");
                foreach (var ab in availableAbilities.Values)
                {
                    string status = selectedIds.Contains(ab.Id) ? "[VYBRÁNO]" : "";
                    Console.WriteLine($"{status} [{ab.Id}] {ab.Name} (Mod: {ab.SpreadingModifier})");
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
                        appService.RemoveRegionAbility(regionId, availableAbilities[id]);
                        selectedIds.Remove(id);
                        Console.WriteLine($"Odebráno: {availableAbilities[id].Name}");
                    }
                    else
                    {
                        appService.AddRegionAbility(regionId, availableAbilities[id]);
                        selectedIds.Add(id);
                        Console.WriteLine($"Přidáno: {availableAbilities[id].Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Shows the regions menu and allows selecting a region to edit.
        /// </summary>
        static async Task RegionsMenu()
        {
            Console.Clear();
            Console.WriteLine("\n---Menu regionů---");
            ShowRegions();
            while (true)
            {
                Console.Write("Vyberte region (0 pro konec): ");
                string input = Console.ReadLine();
                if (input == "0")
                    break;
                try
                {
                    Console.WriteLine(appService.GetRegionString(input));
                }
                catch (ArgumentException e) {
                    Console.WriteLine(e.Message);
                    continue;
                }
                await EditRegionMenu(input);

            }
        }

        /// <summary>
        /// Edit menu for a single region (change speed, healthcare index, abilities).
        /// </summary>
        static async Task EditRegionMenu(string input)
        {
            int regionId = Int32.Parse(input); // Volano az po kontrole v jiné funkci, mel by vzdy byt platny
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Úprava regionu ---");
                Console.WriteLine("""
                [1] - Změnit zákl. rychlost šíření
                [2] - Změnit index zdravotnictví
                [3] - Změnit vlastnosti regionu
                [0] - Zpět
                """);
                input = Console.ReadLine();
                switch (input)
                {
                    case "0":
                        return;
                    case "1":
                        while (true)
                        {
                            Console.Write("Zadejte hodnotu (kladné, desetinné): ");
                            try
                            {
                                appService.SetRegionSpreadingSpeed(regionId, Console.ReadLine());
                                break;
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        break;

                    case "2":
                        while (true)
                        {
                            Console.Write("Zadejte hodnotu (kladné, desetinné): ");
                            try
                            {
                                appService.SetRegionHealthcareIndex(regionId, Console.ReadLine());
                                break;
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        break;

                    case "3":
                        await SetRegionAbilities(regionId);
                        break;

                    default:
                        Console.WriteLine("Neplatná akce");
                        break;
                
                }
            }
        }

        /// <summary>
        /// Simulation control menu (start/stop/change parameters/manage regions).
        /// Note: subscribes to OnDaySimulated for console output.
        /// </summary>
        static async Task SimulationMenu()
        {
            appService.OnDaySimulated += Console.WriteLine;
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("--- Menu simulace ---");
                Console.WriteLine("""
                Příkazy: 
                [0] - Spustit simulaci
                [1] - Pozastavit simulaci
                [2] - Změnit rychlost šíření
                [3] - Změnit šanci na smrt
                [4] - Upravit vlastnosti nemoce
                [5] - Spravovat regiony
                [9] - Breakpoint
                """);
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
                    case "5":
                        await RegionsMenu();
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

        /// <summary>
        /// Loads simulation data from a file path provided by the user; retries on error.
        /// </summary>
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
                    Console.Write("Soubor nebyl nalezen. Zkuste to znovu: ");
                }
                catch (Exception e)
                {
                    Console.Write($"Chyba při načítání: {e.Message}. Zkuste to znovu: ");
                }
            }
        }

    }
}