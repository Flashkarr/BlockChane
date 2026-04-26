using System;

namespace BlockChane.Models
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }

        public string Data { get; set; }

        public string Author { get; set; }

        public string Hash { get; set; }

        public string PrevHash { get; set; }

        public int Nonce { get; set; }

        public long MiningTimeMs { get; set; }

        public double MiningDurationSeconds { get; set; } = 0;

        public int DifficultyAtMining { get; set; }

        public Block(int index, string data, string author, string prevHash, DateTime timestamp)
        {
            Index = index;
            Timestamp = timestamp;
            Data = data;
            Author = author;
            PrevHash = prevHash;
            Hash = "";
            Nonce = 0;
        }
    }
}