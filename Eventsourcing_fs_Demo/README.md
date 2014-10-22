# Eventsourcing demonstration

The aim of this project is to demonstrate event sourcing in a simplified CQRS context without the use of external frameworks.
Focus is the redundancy-free definition of projections of the event stream into momentary 'state'. 

## Caveats

To simplify the code, this example deviates in some ways from a 'real-world' solution. 
All simplifications are omissions of optimizations and hence servs to bring out concepts more clearly.

### Runtime properties of projections

All projections are evaluated "on demand", replaying the event history every time.
While is is totally feasible for evaluation business rules, in pratice a precomputed or lazily caching projection would be chosen for the 
query side of the implementation, in particular for any kind of lists and aggregations.

### Aggregates, streams, event sources etc.

In this demonstration, there is no explicit notion of event streams, nor of aggregates. All projections have access to all events in the history, 
and filter the relevant events by actual data instead of metadata. In a real situation, the events would be available pre-filtered to increase performance.

### The 'event store'

For the sake of simplicity, the event store is just a list of objects. A real event store would, as a minimum, need the concept of a 'commit' and 
offer a function to subscribe to new commits. Precomputing event handlers / projections would make use of such a subscription to receive partial updates.
