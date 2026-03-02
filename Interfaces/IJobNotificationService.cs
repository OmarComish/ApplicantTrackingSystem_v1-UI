using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATS.API.Interfaces
{
    public interface IJobNotificationService
    {
        void AddClient(string clientId, HttpResponse response);
        void RemoveClient(string clientId);
        Task BroadcastJobAsync(object job);
    }
}