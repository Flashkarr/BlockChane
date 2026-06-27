using System;

namespace BlockChane.Models
{
    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string From { get; set; } = "";

        public string To { get; set; } = "";

        public decimal Amount { get; set; }

        public decimal Fee { get; set; } = 1m;

        public string TokenSymbol { get; set; } = "MAIN";

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public int LockTime { get; set; }

        public byte[] Signature { get; set; } = Array.Empty<byte>();

        public Transaction()
        {
        }

        public Transaction(string from, string to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
        }

        public string ToRawString()
        {
            return
                $"{From}" +
                $"{To}" +
                $"{Amount}" +
                $"{Fee}" +
                $"{TokenSymbol}" +
                $"{TimeStamp:O}" +
                $"{LockTime}";
        }

        public override string ToString()
        {
            return
                $"Id: {Id}\n" +
                $"From: {From}\n" +
                $"To: {To}\n" +
                $"Amount: {Amount}\n" +
                $"Fee: {Fee}\n" +
                $"Token: {TokenSymbol}\n" +
                $"Time: {TimeStamp}\n";
        }
    }
}