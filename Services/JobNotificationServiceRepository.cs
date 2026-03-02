using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ATS.API.Interfaces;

namespace ATS.API.Services
{
    public class JobNotificationServiceRepository: IJobNotificationService
    {
        private readonly List<(string Id, HttpResponse Response)> _clients = new();
        private readonly object _lock = new();
        public void AddClient(string clientId, HttpResponse response)
        {
            lock (_lock) _clients.Add((clientId, response));
        }
        public void RemoveClient(string clientId)
        {
            lock (_lock) _clients.RemoveAll(c => c.Id == clientId);
        }
        public async Task BroadcastJobAsync(object job)
        {
            var json = JsonSerializer.Serialize(job);
            var message = $"data: {json}\n\n";

            List<(string Id, HttpResponse Response)> snapshot;
            lock (_lock) snapshot = _clients.ToList();

            Console.WriteLine($"Client count: {_clients.Count}");

            foreach (var client in snapshot)
            {
                try
                {
                    await client.Response.WriteAsync(message);
                    await client.Response.Body.FlushAsync();
                }
                catch
                {
                    RemoveClient(client.Id);
                }
            }
        }
    }
}