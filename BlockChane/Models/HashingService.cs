using System.Security.Cryptography;
using System.Text;
using BlockChane.Models;

namespace BlockChane.Service
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            string rawData = $"{block.Index}{block.Timestamp}{block.Data}{block.Author}{block.PrevHash}{block.Nonce}";
            return ComputeHash(rawData);
        }

        private string ComputeHash(string rawData)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(rawData);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}