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
            return genesis;
        }

        public void AddBlock(string data)
        {
            var lastBlock = Chain[^1];

            var newBlock = new Block(
                lastBlock.Index + 1,
                data,
                "User",
                lastBlock.Hash,
                DateTime.UtcNow
            );

            newBlock.DifficultyAtMining = Difficulty;

            _miningService.MineBlock(newBlock, Difficulty);

            Chain.Add(newBlock);

            if (newBlock.Index % _difficultyAdjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }

        private void AdjustDifficulty()
        {
            var recentBlocks = Chain
                .Skip(Math.Max(0, Chain.Count - _difficultyAdjustmentInterval))
                .Take(_difficultyAdjustmentInterval)
                .ToList();

            var totalTime = recentBlocks.Sum(b => b.MiningDurationSeconds);
            var avgTime = totalTime / _difficultyAdjustmentInterval;

            double ratio = avgTime / _targetBlockTimeSeconds;

            if (ratio < 0.2)
            {
                Difficulty += 1;
            }
            else if (ratio > 5)
            {
                Difficulty -= 1;
            }
            else if (avgTime < _targetBlockTimeSeconds)
            {
                Difficulty += 1;
            }
            else if (avgTime > _targetBlockTimeSeconds)
            {
                Difficulty -= 1;
            }

            if (Difficulty < 1)
                Difficulty = 1;

            if (Difficulty > 6)
                Difficulty = 6;
        }

        public void PrintDifficultyHistory()
        {
            foreach (var block in Chain)
            {
                Console.WriteLine($"Index: {block.Index} | Difficulty: {block.DifficultyAtMining}");
            }
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
    }
}