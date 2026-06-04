using BlockChane.Models;
using System;
using System.Collections.Generic;

namespace BlockChane.Service
{
    public class DisplayService
    {
        public void DisplayBlockChain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"Timestamp: {block.Timestamp:dd/MM/yyyy HH:mm:ss}");
                Console.WriteLine($"Previous Hash: {block.PrevHash}");
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine($"Merkle Root: {block.MerkleRoot}");
                Console.WriteLine($"Author: {block.Author}");
                Console.WriteLine($"Nonce: {block.Nonce}");
                Console.WriteLine($"Mining Time: {block.MiningTimeMs} ms");
                Console.WriteLine($"Difficulty: {block.DifficultyAtMining}");

                Console.WriteLine($"Data: {block.Data}");

                Console.WriteLine(new string('-', 50));
            }
        }
    }
}