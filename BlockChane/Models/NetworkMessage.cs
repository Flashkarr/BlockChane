using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChane.Models
{
    public class NetworkMessage
    {
        public string Type { get; set; } = "";
        public string Data { get; set; } = "";

        public NetworkMessage()
        {
        }

        public NetworkMessage(string type, string data)
        {
            Type = type;
            Data = data;
        }
    }
}