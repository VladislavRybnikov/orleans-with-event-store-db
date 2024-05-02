using System.Reflection;
using System.Text;
using EventStore.Client;
using Newtonsoft.Json;
using OrleansPlusEventStoreDb.Demo.Events;
using OrleansPlusEventStoreDb.Demo.States;

namespace OrleansPlusEventStoreDb.Demo;

public static class Extensions
{
    public static void ApplyDomainEvents<TState>(this TState state, IEnumerable<IDomainEvent?> domainEvents)
        where TState : IState<TState>
    {
        foreach (var domainEvent in domainEvents.Where(x => x != null))
        {
            var apply = typeof(TState).GetMethod("Apply", [domainEvent!.GetType()]);
            if (apply != null)
            {
                apply.Invoke(state, [domainEvent]);
            }
        }
    }

    public static T? GetFromJson<T>(this EventRecord eventRecord)
    {
        var json = Encoding.UTF8.GetString(eventRecord.Data.Span);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static IDomainEvent? MapToDomainEvent(this EventRecord eventRecord)
    {
        var json = Encoding.UTF8.GetString(eventRecord.Data.Span);
        var domainEventType = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && t.GetInterface("IDomainEvent") != null)
            .FirstOrDefault(t => t.Name == eventRecord.EventType);
        return domainEventType != null 
            ? JsonConvert.DeserializeObject(json, domainEventType) as IDomainEvent
            : null;
    }

    public static EventData GetEventData(this IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType().Name;
        var eventJson = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(domainEvent));
        return new EventData(Uuid.FromGuid(domainEvent.Id), eventType, eventJson);
    }
    
    public static EventData GetEventData<TState>(this IState<TState> state, string id) where TState : IState<TState>
    {
        var eventType = typeof(TState).Name;
        var eventJson = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(state));
        return new EventData(Uuid.Parse(id), eventType, eventJson);
    }
}