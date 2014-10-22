namespace Eventsourcing_fs_Demo

open System
    
module Layout = 
    open Html
    open Eventsourcing_fs_Demo.WebApplication
    open Common


    let html_page title content = tag "html" (tags [tag "head" (tag "title" title []) []; tag "body" (tags content) []]) []
    
    let menuitem (current_key:String) (item:MenuItem) = 
        if (item.Key = current_key) 
        then item.Title 
        else a ("/"+item.Key) item.Title

    let navigation (menu:MenuItem seq) key = ul (tags <| Seq.map (menuitem key >> li) menu)

    let page_header title = h1 title
    let page_content content = div content

    let page_frame = html_page

    let page title menu key content = 
        page_frame title <| 
            [
                page_header title
                navigation menu key
                page_content content
            ]

    let section_title content = Html.h 3 content

    let datatable rows = rows |> Seq.map tr |> tags |> table
    let datatable2 headerrow rows = datatable (headerrow::rows)
    
    let datarow columns = columns |> Seq.map td |> tags |> tr
    let headerrow columns = columns |> Seq.map th |> tags |> tr

    let link url text = 
        a url text


    let hidden_field key (value:string) =
        input "hidden" "" [("name",key); ("value", value)]

    let id_field key (initialvalue:System.Guid) =
        hidden_field key (initialvalue.ToString())

    let inline number_field key (initialvalue) caption =
        [
            caption
            "&nbsp"
            input "number" "" [("name",key); ("value", initialvalue.ToString())]
        ] |> tags |> span

    let text_field key (initialvalue:string) caption =
        [
            caption
            "&nbsp"
            input "text" "" [("name",key); ("value", initialvalue.ToString())]
        ] |> tags |> span

    let form url submit_caption fields =
        let submit = input "submit" "" [("value", submit_caption)]
        Html.form "POST" url ([fields;submit]|>tags)

    let collect = Html.tags
