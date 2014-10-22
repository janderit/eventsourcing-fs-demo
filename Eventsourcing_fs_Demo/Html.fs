namespace Eventsourcing_fs_Demo

open System

module Html =

    let render_attributes attributes = (" " + (List.map (fun (key,value) -> key+"=\""+value+"\"") attributes |> (fun s -> String.Join(" ", s)))).TrimEnd()

    let tags (parts: string seq) = String.Join("",parts)

    let tag id content attributes = sprintf "<%s%s>%s</%s>" id (render_attributes attributes) content id


    let div content = tag "div" content []
    let span content = tag "span" content []
    
    let h level content = tag ("h"+level.ToString()) content []
    let h1 content = h 1 content
    
    let li content = tag "li" content []
    let ul content = tag "ul" content []
    
    let a href content = tag "a" content ["href",href]
    
    let table rows = tag "table" rows []
    let tr content = tag "tr" content []
    let th content = tag "th" content []
    let td content = tag "td" content []
            
    let form httpmethod url content = tag "form" content ["method",httpmethod; "action",url]
    let input inputtype content attrs =
        tag "input" content (("type",inputtype)::attrs)

    