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
                        newHashes.Add(
                            ComputeHash(hashes[i] + hashes[i + 1])
                        );
                    }
                    else
                    {
                        newHashes.Add(
                            ComputeHash(hashes[i] + hashes[i])
                        );
                    }
                }

                hashes = newHashes;
            }

            return hashes[0];
        }

        public List<string> GetMerkleProof(List<Transaction> transactions, string targetTxId)
        {
            var hashes = transactions
                .Select(tx => ComputeHash(tx.ToRawString()))
                .ToList();

            int index = transactions.FindIndex(tx => tx.Id == targetTxId);

            if (index == -1)
                return new List<string>();

            List<string> proof = new();

            while (hashes.Count > 1)
            {
                if (index % 2 == 0)
                {
                    if (index + 1 < hashes.Count)
                        proof.Add(hashes[index + 1]);
                }
                else
                {
                    proof.Add(hashes[index - 1]);
                }

                List<string> newHashes = new();

                for (int i = 0; i < hashes.Count; i += 2)
                {
                    if (i + 1 < hashes.Count)
                        newHashes.Add(ComputeHash(hashes[i] + hashes[i + 1]));
                    else
                        newHashes.Add(hashes[i]);
                }

                index /= 2;
                hashes = newHashes;
            }

            return proof;
        }

        public bool VerifyMerkleProof(string txHash, List<string> proof, string expectedMerkleRoot)
        {
            string currentHash = txHash;

            foreach (var siblingHash in proof)
            {
                currentHash = ComputeHash(currentHash + siblingHash);
            }

            return currentHash == expectedMerkleRoot;
        }
    }
}