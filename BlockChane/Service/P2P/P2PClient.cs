using BlockChane.Models;
using System.Net.Sockets;
using System.Text.Json;

namespace BlockChane.Service.P2P
{
    public class P2PClient
    {
        private readonly List<string> _peers = new List<string>();

        public void Connect(string peerAddress)
        {
            if (!_peers.Contains(peerAddress))
            {
                _peers.Add(peerAddress);
                Console.WriteLine($"Connected to peer: {peerAddress}");
            }
        }

        public async Task BroadcastTransactionAsync(Transaction transaction)
        {
            var jsonTransaction = JsonSerializer.Serialize(transaction);

            foreach (var peer in _peers)
            {
                try
                {
                    var parts = peer.Split(':');

                    var ip = parts[0];
                    var port = int.Parse(parts[1]);

                    using var client = new TcpClient();
                    await client.ConnectAsync(ip, port);

                    using var stream = client.GetStream();
                    using var writer = new StreamWriter(stream);

                    await writer.WriteLineAsync(jsonTransaction);
                    await writer.FlushAsync();

                    Console.WriteLine($"Transaction sent to {peer}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending to peer {peer}: {ex.Message}");
                }
            }
        }
    }
}