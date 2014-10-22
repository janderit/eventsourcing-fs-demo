
(*
 * Simple code generator F# script using immediate string output
 *
 * parses some sections of the included Specifications.md file 
 * 
 * Usage:
 * interactive: mark all lines of this file and choose "Execute in F# Interactive" from the editor's context menu
 * build: get yourself a FAKE (F# Make) build script and wire up the code generator in the build process
 *)

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

open System

module Model =

    type TypeAnnotation = { Identifier_match:string->bool; Typecode:string }

    type EventDefinition = { Identifier:string; Module:string; Properties:EventPropertyDefinition list }
    and EventPropertyDefinition = { Identifier:string; Type:string }




module Parser =

    open Model

    let rec do_parse_entities (speclines:string list) acc =
        match speclines with 
            | head :: rem when head.StartsWith("* ") -> 
                let identifier = head.TrimStart(' ','*').Trim()
                do_parse_entities rem (identifier::acc)
            | _-> acc
    
    let rec parse_entities speclines = 
        match speclines with        
            | [] -> []
            | head :: rem when head = "### Business entities" -> do_parse_entities rem []
            | _ :: rem -> parse_entities rem


    let identifier_match (identifier_template:string) =
        let startswith_wildcard = identifier_template.StartsWith("%")
        let endswith_wildcard = identifier_template.EndsWith("%")
        let identifier_template = identifier_template.Trim('%')
        if (not startswith_wildcard && not endswith_wildcard)
        then
            (fun identifier -> identifier = identifier_template)
        else
            if (startswith_wildcard && endswith_wildcard)
            then
                (fun identifier -> (" "+identifier+" ").Contains identifier_template)
            else
                if (startswith_wildcard)
                then
                    (fun identifier -> (" "+identifier).EndsWith identifier_template)
                else
                    (fun identifier -> (identifier+" ").StartsWith identifier_template)
            
    
    let rec do_parse_definitions (speclines:string list) acc =
        match speclines with 
            | head :: rem when head.StartsWith("* ") -> 
                let (identifier, typecode) = head.TrimStart(' ','*').Split([|':'|],2) |> (fun ar->(ar.[0].Trim(),ar.[1].Trim()))
                let typeannotation = { TypeAnnotation.Identifier_match = identifier_match identifier; Typecode=typecode }
                do_parse_definitions rem (typeannotation::acc)
            | _-> acc
    
    let rec parse_definitions speclines = 
        match speclines with        
            | [] -> []
            | head :: rem when head = "### Definitions" -> do_parse_definitions rem []
            | _ :: rem -> parse_definitions rem
    
    
    
    let determine_type typedefinitions (identifier:string) = 
        let tdef = typedefinitions |> Seq.tryFind (fun td -> td.Identifier_match identifier)
        match tdef with
        | Some t when t.Typecode.Contains("$") -> t.Typecode.Replace("$", identifier.Replace(" id",""))
        | Some t -> t.Typecode
        | None -> failwith ("unknown (for '"+identifier+"')")
    
    let rec parse_event_properties (speclines:string list) acc typedefinitions =
        match speclines with 
            | head :: rem when head.StartsWith("  - ") -> 
                let identifier = head.TrimStart().Split([|' '|],2).[1].Trim()
                let property_type = determine_type typedefinitions identifier
                let property = { EventPropertyDefinition.Identifier = identifier; Type=property_type }
                parse_event_properties rem (property::acc) typedefinitions
            | _-> (acc, speclines)
    
    let rec do_parse_events_in_event_section (speclines:string list) acc current_module typedefinitions = 
        match speclines with 
            | head :: rem when head.StartsWith("#### ")-> 
                let new_module = head.Split([|' '|],2).[1]
                do_parse_events_in_event_section rem acc new_module typedefinitions
            | head :: rem when head.StartsWith("* ")-> 
                let identifier = head.Split([|' '|],2).[1]
                let (properties, rem) = parse_event_properties rem [] typedefinitions
                let event = { EventDefinition.Module=current_module; Identifier = identifier; Properties = properties |> List.rev }
                do_parse_events_in_event_section rem (event::acc) current_module typedefinitions
            | _ ->  acc
    
    let rec do_parse_events typedefinitions speclines = 
        match speclines with        
            | [] -> []
            | head :: rem when head = "### Business events" -> do_parse_events_in_event_section rem [] "main" typedefinitions
            | _ :: rem -> do_parse_events typedefinitions rem

    let parse_events speclines = 
        let typedefinitions = parse_definitions speclines
        do_parse_events typedefinitions speclines |> List.rev
    


module CodeGen_FS =

    let join (inter:string) (enum:string seq): string = String.Join(inter, enum)

    let indent = sprintf "    %s"

    let module_identifier (id:string) = if (id.Contains(" ")) then ("``"+id+"``") else id
     
    let fsharp_module modulename (content: string seq) =
        seq {
            yield "module "+(module_identifier modulename)+" ="
            yield! content |> Seq.map indent
            yield ""
        }

    let fsharp_parameter identifier typestring = (identifier, typestring)

    let fsharp_parameter_declaration (identifier, typestring) =  sprintf "%s: %s" identifier typestring
    
    let fsharp_record_declaration_list parameters = String.Join(";", parameters |> Seq.map fsharp_parameter_declaration)

    let fsharp_parameter_declaration_list parameters = "(" + String.Join(") (", parameters |> Seq.map fsharp_parameter_declaration) + ")"

    let fsharp_record identifier properties =
        sprintf "type %s = {%s}" identifier (fsharp_record_declaration_list properties)



module CodeGen =

    open CodeGen_FS 

    let identifier (id:string) = if (id.Contains(" ")) then ("``"+id+"``") else id
    let underscore_identifier (id:string) = id.Replace(" ","_")

    let CLITypes = 
        ["text","String";
         "number","Int32";
         "currency","Common.Currency";
         "date","Common.Date";
         "timespan in days","Common.Days"] 
        |> Map.ofList    
    
    let entity_identifier (entity:string) =
        entity.Substring(0,1).ToUpper() + entity.Substring(1)

    let clitype (typecode:string) = 
        if (typecode.StartsWith("domain entity <"))
        then
            typecode.Replace("domain entity", "").Trim('<','>',' ') |> entity_identifier
        else
            CLITypes.[typecode]

    let generate_event_property (property:Model.EventPropertyDefinition) = fsharp_parameter (identifier property.Identifier) (clitype property.Type)

    let generate_event_properties properties = properties |> Seq.map generate_event_property

    let generate_function_parameter (property:Model.EventPropertyDefinition) = fsharp_parameter (underscore_identifier property.Identifier) (clitype property.Type)

    let generate_function_parameters properties = properties |> Seq.map generate_function_parameter

    let generate_typed_identifier entity =
        seq {
            yield sprintf "type %s = %s of Guid with" entity entity             
            yield sprintf "    member this.Id = match this with | (%s id) -> id" entity
            yield sprintf "    override this.ToString () = \"%s@[\"+(this.Id.ToString())+\"]\"" entity
            yield sprintf "let new_%s () = %s (Guid.NewGuid())" entity entity
            yield ""
            yield ""
        }
        

    let generate_typed_identifiers entities =
        entities |> Seq.collect(entity_identifier >> generate_typed_identifier)

    let record_factory e_id ({Model.EventPropertyDefinition.Identifier = p_id ; Model.EventPropertyDefinition.Type = tc}::rem) =
        (identifier e_id)+"."+(identifier p_id)+"="+(underscore_identifier p_id) + (String.Join("", (rem |> Seq.map (fun {Identifier = id ; Type = tc} -> "; "+(identifier id)+"="+(underscore_identifier id)))))

    let generate_event (event:Model.EventDefinition) =
        seq {
            yield (fsharp_record (identifier event.Identifier) (generate_event_properties event.Properties)) + " with"
            yield sprintf "    override this.ToString() = "
            yield sprintf "        let data = %s " (event.Properties |> Seq.map (fun p -> "\"; '"+p.Identifier+"':'\"+this."+(identifier p.Identifier)+".ToString()+\"'\"") |> join "+")
            yield sprintf "        sprintf \"%s%%s\" data" event.Identifier
            yield "/// parmeters: "+(event.Properties |> Seq.map (fun p -> p.Identifier) |> (fun s -> String.Join(", ", s)))
            yield sprintf "let %s %s : System.Object = {%s} :> System.Object" (underscore_identifier ("event_"+event.Identifier)) (fsharp_parameter_declaration_list (generate_function_parameters event.Properties)) (record_factory event.Identifier event.Properties)
            yield ""
            yield ""
        }
        

    let events_module (modulename,events) =
        events 
            |> Seq.collect generate_event
            //|> fsharp_module modulename

    let generate_events (events:Model.EventDefinition seq) : string seq =
        events 
            |> Seq.groupBy (fun x->x.Module) 
            |> Seq.collect events_module
    


let specs = System.IO.File.ReadAllText(@"Specifications.md").Split([|'\r';'\n'|], StringSplitOptions.RemoveEmptyEntries) |> Seq.toList

let entities = Parser.parse_entities specs |> List.rev
let events = Parser.parse_events specs

let output = 
    CodeGen_FS.fsharp_module "Generated" <|
        CodeGen_FS.fsharp_module "Domain" (
            seq { 
                yield "open System"
                yield ""
                yield! CodeGen.generate_typed_identifiers entities
                yield ""
                yield! CodeGen.generate_events events
            }) |> Seq.toList

System.IO.File.WriteAllLines("Model.gen.fs", ["// WARNING: GENERATED CODE";"// *Never* manually modify the code in this file";"namespace Eventsourcing_fs_Demo"]@output)
