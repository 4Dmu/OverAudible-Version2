using Dinah.Core.Net.Http;
using OverAudible.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.DownloadQueue
{
    public record QueueFile(string asin, string name);

    public interface IDownloadQueue
    {
        public void Enqueue(QueueFile job);
        public event Action<ProgressChangedObject>? ProgressChanged;
        public event Action? QueueEmptied;
        public event Action<int>? UpdatedQueueCount;
        public List<QueueFile> GetQueue();
    }

    public class MyQueue : IDownloadQueue
    {
        public event Action<ProgressChangedObject>? ProgressChanged;
        public event Action? QueueEmptied;
        public event Action<int>? UpdatedQueueCount;
        private bool isBusy;

        public Queue<QueueFile> queue = new Queue<QueueFile>();

        public async void Enqueue(QueueFile job)
        {
            queue.Enqueue(job);
            UpdatedQueueCount?.Invoke(queue.Count);
            await Task.Run(DoWork);
        }

        private async Task DoWork()
        {
            if (queue.Count == 0 || isBusy)
                return;

            isBusy = true;

            QueueFile file = queue.First();

            var prog = new Progress<DownloadProgress>();
            EventHandler<DownloadProgress> progHandler = (object? sender, DownloadProgress p) => 
            {
                ProgressChanged?.Invoke(new ProgressChangedObject(file.asin, file.name, p));
            };
            prog.ProgressChanged += progHandler;

            

            var api = await ApiClient.GetInstance();

            await api.Api.DownloadAsync(file.asin, new AudibleApi.LibraryPath(Constants.DownloadFolder), 
                new AudibleApi.AsinTitlePair(file.asin, file.name), prog);

            prog.ProgressChanged -= progHandler;

            queue.Dequeue();

            isBusy = false;

            await DoWork();
        }

        public List<QueueFile> GetQueue()
        {
            return queue.ToList();
        }
    }
}
