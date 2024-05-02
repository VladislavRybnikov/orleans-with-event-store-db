# Orleans with EventStore DB
An example of event sourcing implementation on .NET using Orleans and EventStoreDb.

## Event Sourcing
Event sourcing - is a technique of representing changes of the domain model in the form of sequence of incremental changes (events).

### Terms:

- Event - immutable information about data changes
- State - aggregated data based on events which represents the state of the domain model in some point of time.
- Stream - a sequence of events grouped by some condition.
- Projection - some data model based on one or multiple streams of events

### Benefits:
- Provide the whole history of data changes which helps to investigate issues
- No need to implement a transactional outbox pattern to deal with distributed transaction of saving changes to the database and publishing an event about changes
- Gives you a better understanding of the domain

## EventStore DB
TBD

## Actors

## Orleans
TBD

### Terms

- Grain
- Silo

### JournaledGrain
