using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BlockChane.Models;

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
                Console.WriteLine(new string('-', 50));
            }
        }
    }
}
