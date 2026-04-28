using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class EventAggregator
    {
        private static EventAggregator? _instance;
        public static EventAggregator Instance => _instance ??= new EventAggregator();

        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();
            _handlers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.ContainsKey(type))
                _handlers[type].Remove(handler);
        }

        public void Publish<T>(T message)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
                foreach (var handler in handlers.ToList())
                    ((Action<T>)handler)(message);
        }

    }

    public record DaySimulatedEvent(DateOnly Date, StatisticUpdate Stats);
    public record RegionsLoadedEvent(List<Region> Regions);
    public record RegionsLoadErrorEvent(string Message);

}
