using BlockChane.Models;
using System.Text.Json;

namespace BlockChane.Service
{
    public class StorageService
    {
        private const string FilePath = "blockchain_data.json";

        public void SaveBlockchain(List<Block> blockchain)
        {
            var json = JsonSerializer.Serialize(blockchain, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(FilePath, json);
        }

        public List<Block> LoadBlockchain()
        {
            if (!File.Exists(FilePath))
                return new List<Block>();

            var json = File.ReadAllText(FilePath);

            return JsonSerializer.Deserialize<List<Block>>(json) ?? new List<Block>();
        }
    }
}