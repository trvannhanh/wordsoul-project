using System.Threading.Channels;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.BackgroundServices
{
    public class SystemLogQueue
    {
        private readonly Channel<SystemLog> _queue;

        public SystemLogQueue()
        {
            // Bounded channel with DropOldest behavior if queue is full
            var options = new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            };
            _queue = Channel.CreateBounded<SystemLog>(options);
        }

        public async ValueTask EnqueueAsync(SystemLog log, CancellationToken cancellationToken = default)
        {
            await _queue.Writer.WriteAsync(log, cancellationToken);
        }

        public IAsyncEnumerable<SystemLog> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return _queue.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
