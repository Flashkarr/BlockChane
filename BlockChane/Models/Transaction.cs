using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChane.Models
{
    public class Transaction
    {
        public string Id { get; set; }

        public string From { get; set; }

        public string To { get; set; }
        public byte[] Signature { get; set; }

        public decimal Amount { get; set; }

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public int LockTime { get; set; } = 0;
        public Transaction(string from, string to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
            Id = Guid.NewGuid().ToString();
        }

        public string ToRawString()
        {
            return $"{From}{To}{Amount}{TimeStamp:O}";
        }

        public override string ToString()
        {
            return $"Transaction: {Id}, From: {From}, To: {To}, Amount: {Amount}, TimeStamp: {TimeStamp}, Signature: {Convert.ToBase64String(Signature)}";
            ;
        }
    }
}
