using SimulationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data
{
    /// <summary>
    /// Simple JSON parser for loading scenario data from a file.
    /// </summary>
    public class JsonParser
    {
        /// <summary>
        /// JsonSerializer options used for deserialization (case-insensitive, indented).
        /// </summary>
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        /// <summary>
        /// Loads a ScenarioData instance from the provided JSON file path.
        /// Throws FileNotFoundException if the file does not exist.
        /// </summary>
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
