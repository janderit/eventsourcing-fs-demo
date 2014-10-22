namespace Eventsourcing_fs_Demo.WebApplication

module Common =

    open Eventsourcing_fs_Demo
    open QueryHandling

    type MenuItem = { Key:string; Title:string }

    type WebContext (company, menu, readmodels) =
        member this.Menu : MenuItem list = menu
        member this.Company :string = company
        member this.Readmodels :Readmodels = readmodels

    let inline details_url< ^T when ^T: ( member Id : System.Guid)> entity (id:^T) = 
        sprintf "/%s/%s" entity (( ^T : (member Id : System.Guid) id).ToString())


    let form_data (fielddata:(string*string) list) (key:string) : string =
        match fielddata |> List.tryFind (fun (x,_)->x=key) |> Option.map (fun (_,y) -> y) with
        | Some x -> x
        | _ -> failwith "required form data: "+key