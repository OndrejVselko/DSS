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

        public static async Task<List<Region>> LoadRegionsFromJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Soubor nebyl nalezen.", filePath);

            using FileStream openStream = File.OpenRead(filePath);
            List<Region>? regions = await JsonSerializer.DeserializeAsync<List<Region>>(openStream, _options);
            return regions ?? new List<Region>();
        }

        public static async Task<Dictionary<int, DiseaseAbility>> LoadDiseaseAbilitiesFromJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Soubor nebyl nalezen.", filePath);

            using FileStream openStream = File.OpenRead(filePath);
            List<DiseaseAbility>? abilities = await JsonSerializer.DeserializeAsync<List<DiseaseAbility>>(openStream, _options);

            return (abilities ?? new List<DiseaseAbility>())
                .ToDictionary(a => a.Id);
        }
    }
}
