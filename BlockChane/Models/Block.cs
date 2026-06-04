using System;
using System.Collections.Generic;

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

        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public string MerkleRoot { get; set; } = "";
        public decimal TotalSupply { get; set; } = 0;

        public int DifficultyAtMining { get; set; }

        public Block()
        {
            Data = "";
            Author = "";
            Hash = "";
            PrevHash = "";
            Transactions = new List<Transaction>();
        }

        public Block(int index, string data, string author, string prevHash, DateTime timestamp)
        {
            Index = index;
            Timestamp = timestamp;
            Data = data;
            Author = author;
            PrevHash = prevHash;
            Hash = "";
            Nonce = 0;
            Transactions = new List<Transaction>();
        }

        public decimal GetTotalSupply()
        {
            return TotalSupply;
        }
    }
}