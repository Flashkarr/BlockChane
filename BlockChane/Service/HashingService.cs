using System.Security.Cryptography;
using System.Text;
using BlockChane.Models;

namespace BlockChane.Service
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            string rawData =
                $"{block.Index}{block.Timestamp}{block.MerkleRoot}{block.Author}{block.PrevHash}{block.Nonce}";

            return ComputeHash(rawData);
        }

        public string ComputeHash(string rawData)
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

        public string BuildMerkleRoot(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
                return string.Empty;

            var hashes = transactions
                .Select(tx => ComputeHash(tx.ToRawString()))
                .ToList();

            while (hashes.Count > 1)
            {
                var newHashes = new List<string>();

                for (int i = 0; i < hashes.Count; i += 2)
                {
                    if (i + 1 < hashes.Count)
                    {
                        string combinedHash = hashes[i] + hashes[i + 1];
                        newHashes.Add(ComputeHash(combinedHash));
                    }
                    else
                    {
                        newHashes.Add(hashes[i]);
                    }
                }

                hashes = newHashes;
            }

            return hashes[0];
        }
    }
}