using Data;
using SimulationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services
{
    public class DataServices
    {
        public DataServices() { 
        
        }
        public async Task<List<Region>> LoadRegionsFromJson(string path)
        {
            return await JsonParser.LoadRegionsFromJson(path);
        }
    }
}
