using BlockChane.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Diagnostics;

namespace BlockChane.Models
{
    public class BlockChainServise
    {
        public List<Block> Chain { get; set; }
        private HashingService _hashingService;

        public BlockChainServise()
        {
            _hashingService = new HashingService();
            Chain = new List<Block>();
            AddGenesisBlock();
        }

        private void AddGenesisBlock()
        {
            Block genesis = new Block(0, "Genesis Block", "System", "0", DateTime.UtcNow);
            genesis.Hash = _hashingService.ComputeHash(genesis);
            Chain.Add(genesis);
        }

        private void MineBlockWithPrefix(Block block, string prefix)
        {
            prefix = prefix.ToLower();

            do
            {
                block.Nonce++;
                block.Hash = _hashingService.ComputeHash(block);
            }
            while (!block.Hash.StartsWith(prefix));
        }

        public void AddBlock(string data, string author, string prefix)
        {
            var lastBlock = Chain.Last();

            var newBlock = new Block(
                lastBlock.Index + 1,
                data,
                author,
                lastBlock.Hash,
                DateTime.UtcNow
            );

            Stopwatch sw = new Stopwatch();
            sw.Start();

            MineBlockWithPrefix(newBlock, prefix);

            sw.Stop();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 80));
            Console.ResetColor();

            Console.WriteLine($"Mining block {newBlock.Index}...");

            sw.Start();

            MineBlockWithPrefix(newBlock, prefix);

            sw.Stop();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Block {newBlock.Index} FOUND!");
            Console.ResetColor();

            Console.WriteLine($"Nonce: {newBlock.Nonce}");
            Console.WriteLine($"Hash: {newBlock.Hash}");
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 80));
            Console.ResetColor();

            Chain.Add(newBlock);

        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var prevBlock = Chain[i - 1];

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return false;

                if (currentBlock.PrevHash != prevBlock.Hash)
                    return false;
            }

            return true;
        }
    }
}