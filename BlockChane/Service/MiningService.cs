using BlockChane.Models;
using System;

namespace BlockChane.Service
{
    public class MiningService
    {
        private readonly HashingService _hashingService;

        public MiningService(HashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public long MineBlock(Block block, int difficulty)
        {
            var target = new string('0', difficulty);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            int maxAttempts = 10_000_000;

            while (block.Nonce < maxAttempts)
            {
                block.Nonce++;
                block.Hash = _hashingService.ComputeHash(block);

                if (block.Nonce % 10000 == 0)
                {
                    Console.Write(".");
                }

                if (block.Hash.StartsWith(target))
                {
                    stopwatch.Stop();

                    block.MiningTimeMs = stopwatch.ElapsedMilliseconds;
                    block.MiningDurationSeconds = stopwatch.Elapsed.TotalSeconds;

                    return block.Nonce;
                }
            }

            stopwatch.Stop();

            block.MiningTimeMs = stopwatch.ElapsedMilliseconds;
            block.MiningDurationSeconds = stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine("Mining failed: too hard");
            return block.Nonce;
        }
    }
}