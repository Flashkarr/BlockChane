using BlockChane.Models;
using System.Collections.Generic;
using System;

namespace BlockChane.Service
{
    public class DisplayService
    {
        public void DisplayBlockChain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"Timestamp: {block.Timestamp}");
                Console.WriteLine($"Data: {block.Data}");
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine($"PrevHash: {block.PrevHash}");
                Console.WriteLine($"Difficulty: {block.DifficultyAtMining}");
                Console.WriteLine(new string('-', 50));
            }
        }
    }
}