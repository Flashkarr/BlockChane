using BlockChane.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockChane.Service
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; } = new List<Block>();
        public int Difficulty { get; set; } = 2;

        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;

        private Dictionary<string, decimal> balances = new Dictionary<string, decimal>();

        private decimal MiningReward = 50m;
        private int HalvingInterval = 5;

        private int _difficultyAdjustmentInterval = 3;
        private double _targetBlockTimeSeconds = 0.8;

        public BlockChainService()
        {
            _hashingService = new HashingService();
            _miningService = new MiningService(_hashingService);

            Chain.Add(CreateGenesisBlock());
        }

        private Block CreateGenesisBlock()
        {
            var genesis = new Block(0, "Genesis Block", "System", "0", DateTime.UtcNow);
            genesis.Hash = _hashingService.ComputeHash(genesis);
            genesis.DifficultyAtMining = Difficulty;
            genesis.Transactions = new List<Transaction>();

            return genesis;
        }

        public void MineBlock(string minerAddress, List<Transaction> transactions)
        {
            if (Chain.Count % HalvingInterval == 0 && Chain.Count != 0)
            {
                MiningReward /= 2;
                Console.WriteLine($"HALVING! New reward: {MiningReward}");
            }

            var rewardTransaction = new Transaction("COINBASE", minerAddress, MiningReward);
            transactions.Add(rewardTransaction);

            var lastBlock = Chain[^1];

            var newBlock = new Block(
                lastBlock.Index + 1,
                "",
                minerAddress,
                lastBlock.Hash,
                DateTime.UtcNow
            );

            newBlock.Transactions = transactions;

            newBlock.Data = string.Join("\n", transactions.Select(t =>
                t.From == "COINBASE"
                    ? $"Transaction: {t.Id}, From: {t.From}, To: {t.To}, Amount: {t.Amount}, TimeStamp: {t.TimeStamp}, Signature: COINBASE"
                    : t.ToString()
            ));

            newBlock.DifficultyAtMining = Difficulty;

            _miningService.MineBlock(newBlock, Difficulty);

            Chain.Add(newBlock);
            RebuildState();

            Console.WriteLine($"Block mined with nonce: {newBlock.Nonce}, hash: {newBlock.Hash}");

            if (newBlock.Index % _difficultyAdjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }

        private void AdjustDifficulty()
        {
            var recentBlocks = Chain
                .Skip(Math.Max(1, Chain.Count - _difficultyAdjustmentInterval))
                .Take(_difficultyAdjustmentInterval)
                .ToList();

            if (recentBlocks.Count == 0)
                return;

            double realTotalTime = recentBlocks.Sum(b => b.MiningDurationSeconds);
            double targetTotalTime = _targetBlockTimeSeconds * recentBlocks.Count;

            if (realTotalTime <= 0)
                realTotalTime = 0.001;

            double newDifficulty = Difficulty * (targetTotalTime / realTotalTime);

            Difficulty = (int)Math.Round(newDifficulty);

            if (Difficulty < 1)
                Difficulty = 1;

            if (Difficulty > 6)
                Difficulty = 6;

            Console.WriteLine($"Difficulty changed to {Difficulty} (real time: {realTotalTime:0.00}s, target: {targetTotalTime:0.00}s)");
        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var current = Chain[i];
                var previous = Chain[i - 1];

                if (current.PrevHash != previous.Hash)
                    return false;

                if (_hashingService.ComputeHash(current) != current.Hash)
                    return false;
            }

            return true;
        }

        public decimal GetTotalSupply()
        {
            decimal total = 0;

            foreach (var block in Chain)
            {
                if (block.Transactions == null) continue;

                foreach (var tx in block.Transactions)
                {
                    if (tx.From == "COINBASE")
                        total += tx.Amount;
                }
            }

            return total;
        }

        public decimal GetBalance(string address)
        {
            if (!balances.ContainsKey(address))
                return 0;

            return balances[address];
        }

        public void RebuildState()
        {
            balances.Clear();

            foreach (var block in Chain)
            {
                if (block.Transactions == null)
                    continue;

                foreach (var tx in block.Transactions)
                {
                    if (tx.From != "COINBASE")
                    {
                        if (!balances.ContainsKey(tx.From))
                            balances[tx.From] = 0;

                        balances[tx.From] -= tx.Amount;
                    }

                    if (!balances.ContainsKey(tx.To))
                        balances[tx.To] = 0;

                    balances[tx.To] += tx.Amount;
                }
            }
        }

        public void ClearState()
        {
            balances.Clear();
        }
    }
}