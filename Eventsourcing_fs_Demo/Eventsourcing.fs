namespace Eventsourcing_fs_Demo

module Eventsourcing =

    type Projection<'Intermediate, 'Final, 'Key> = 
        {
            Selector:obj -> 'Key option

            Init:'Intermediate;
            Step:obj->'Intermediate->'Intermediate; 
            Final:'Intermediate->'Final
        }
    
    let inline no_change x = x

    type OnDemandProjector<'T> = obj seq -> 'T

    let evaluate_ondemand<'I, 'F> (projection:Projection<'I,'F, unit>) 
        : OnDemandProjector<'F> = 
        Seq.fold (fun state event -> projection.Step event state) projection.Init >> projection.Final
    
    let evaluate_ondemand_filtered<'I, 'F, 'Key when 'Key:equality> (projection:Projection<'I,'F, 'Key>) (selection:'Key) 
        : OnDemandProjector<'F> = 
        Seq.filter (fun e -> projection.Selector e = Some selection) >> Seq.fold (fun state event -> projection.Step event state) projection.Init >> projection.Final

