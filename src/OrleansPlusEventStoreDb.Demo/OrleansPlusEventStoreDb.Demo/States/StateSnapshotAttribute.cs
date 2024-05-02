namespace OrleansPlusEventStoreDb.Demo.States;

[AttributeUsage(AttributeTargets.Class)]
public class StateSnapshotAttribute : Attribute
{
    public int SnapshotPerEventsNumber { get; init; } = 100;
}