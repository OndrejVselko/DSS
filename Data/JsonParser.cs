using SimulationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data
{
    public class JsonParser
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static async Task<ScenarioData> LoadScenarioFromJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Soubor nebyl nalezen.", filePath);

            using FileStream openStream = File.OpenRead(filePath);
            ScenarioData? scenario = await JsonSerializer.DeserializeAsync<ScenarioData>(openStream, _options);
            return scenario ?? new ScenarioData();
        }
    }
}
