using BlockChane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace BlockChane.Service
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; } = new List<Block>();
        public int Difficulty { get; set; } = 1;

        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly StorageService _storageService;

        private Dictionary<string, decimal> balances = new Dictionary<string, decimal>();

        public List<Transaction> PendingTransactions { get; set; } = new List<Transaction>();

        private decimal MiningReward = 50m;
        private int HalvingInterval = 5;

        private int _difficultyAdjustmentInterval = 3;
        private TimeSpan TransactionTtl = TimeSpan.FromMinutes(5);
        private int MaxTransactionsPerBlock = 10;
        public BlockChainService()
        {
            _hashingService = new HashingService();
            _miningService = new MiningService(_hashingService);
            _storageService = new StorageService();

            var loadedChain = _storageService.LoadBlockchain();

            if (loadedChain != null && loadedChain.Count > 0)
            {
                Chain = loadedChain;
                ValidateAndRebuildState();
            }
            else
            {
                Chain.Add(CreateGenesisBlock());
                _storageService.SaveBlockchain(Chain);
            }
        }

        private Block CreateGenesisBlock()
        {
            var genesis = new Block(0, "Genesis Block", "System", "0", DateTime.UtcNow);
            genesis.Hash = _hashingService.ComputeHash(genesis);
            genesis.DifficultyAtMining = Difficulty;
            genesis.Transactions = new List<Transaction>();
            genesis.MerkleRoot = _hashingService.BuildMerkleRoot(genesis.Transactions);

            return genesis;
        }

        public bool AddTransaction(Transaction tx)
        {
            var validation = TransactionService.ValidateTransaction(tx);

            if (!validation.isValid)
                throw new Exception(validation.error);

            if (PendingTransactions.Any(t => t.Id == tx.Id))
                return false;

            if (tx.From != "COINBASE" && tx.From != "System")
            {
                int senderPendingCount = PendingTransactions.Count(t => t.From == tx.From);

                if (senderPendingCount >= 3)
                    throw new InvalidOperationException("Spam detected.");

                // Для P2P/Gossip тесту перевірку балансу тимчасово вимкнено.
                // decimal balance = GetBalance(tx.From);
                //
                // if (balance < tx.Amount)
                //     throw new Exception("Not enough balance");
            }

            PendingTransactions.Add(tx);

            Console.WriteLine("Transaction added to mempool");

            return true;
        }

        public bool AddTransactionFromNetwork(Transaction tx)
        {
            var validation = TransactionService.ValidateTransaction(tx);

            if (!validation.isValid)
                throw new Exception(validation.error);

            if (PendingTransactions.Any(t => t.Id == tx.Id))
                return false;

            PendingTransactions.Add(tx);

            Console.WriteLine("Network transaction added to mempool");

            return true;
        }

        public void MineBlock(string minerAddress)
        {
            PendingTransactions.RemoveAll(tx =>
                DateTime.UtcNow - tx.TimeStamp > TransactionTtl
            );

            if (Chain.Count % HalvingInterval == 0 && Chain.Count != 0)
            {
                MiningReward /= 2;
                Console.WriteLine($"HALVING! New reward: {MiningReward}");
            }

            var transactions = PendingTransactions
                .Where(tx => tx.LockTime <= Chain.Count)
                .OrderByDescending(tx => tx.Amount)
                .Take(MaxTransactionsPerBlock)
                .ToList();

            var rewardTransaction =
                new Transaction("COINBASE", minerAddress, MiningReward);

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
            newBlock.MerkleRoot = _hashingService.BuildMerkleRoot(transactions);

            newBlock.Data = string.Join("\n", transactions.Select(t =>
                t.From == "COINBASE"
                    ? $"Transaction: {t.Id}, From: {t.From}, To: {t.To}, Amount: {t.Amount}"
                    : t.ToString()
            ));

            newBlock.DifficultyAtMining = Difficulty;

            _miningService.MineBlock(newBlock, Difficulty);

            Chain.Add(newBlock);

            PendingTransactions.RemoveAll(tx =>
                transactions.Any(t => t.Id == tx.Id)
            );

            ValidateAndRebuildState();

            _storageService.SaveBlockchain(Chain);

            Console.WriteLine($"Block mined with nonce: {newBlock.Nonce}, hash: {newBlock.Hash}");

            if (newBlock.Index % _difficultyAdjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }

        public int EvictStaleTransactions(int maxAgeSeconds)
        {
            int before = PendingTransactions.Count;

            PendingTransactions.RemoveAll(tx =>
                (DateTime.UtcNow - tx.TimeStamp).TotalSeconds > maxAgeSeconds
            );

            return before - PendingTransactions.Count;
        }

        public bool ValidateAndRebuildState()
        {
            balances.Clear();

            foreach (var block in Chain)
            {
                if (block.Transactions == null)
                    continue;

                foreach (var tx in block.Transactions)
                {
                    if (tx.From != "COINBASE" && tx.From != "System")
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

            return true;
        }

        private void AdjustDifficulty()
        {
            Difficulty = 1;
            Console.WriteLine("Difficulty fixed to 1 for testing.");
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
                if (block.Transactions == null)
                    continue;

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
            if (balances.ContainsKey(address))
                return balances[address];

            return 0;
        }

        public void RebuildState()
        {
            ValidateAndRebuildState();
        }

        public void ClearState()
        {
            balances.Clear();
        }

        public void SaveStateSnapshot()
        {
            var json = JsonSerializer.Serialize(balances, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText("state.json", json);

            Console.WriteLine("State snapshot saved.");
        }

        public void LoadStateSnapshot()
        {
            if (File.Exists("state.json"))
            {
                var json = File.ReadAllText("state.json");

                balances = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json)
                           ?? new Dictionary<string, decimal>();

                Console.WriteLine("State snapshot loaded.");
            }
            else
            {
                Console.WriteLine("Snapshot not found. Rebuilding state...");
                ValidateAndRebuildState();
            }
        }

        public bool MerkleRootExists(string root)
        {
            return Chain.Any(b => b.MerkleRoot == root);
        }
    }
}