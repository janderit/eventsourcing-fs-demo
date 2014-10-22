namespace Eventsourcing_fs_Demo.WebApplication

module Logistics =

        open Eventsourcing_fs_Demo
        open Layout
        open Generated
        open Model
        open Domain
        open Projections
        open Common
        open System
        open CommandHandling
        open QueryHandling
        open API

        let dispatch_goods_form_handler (fielddata:(string*string) list) (checked_handle:obj -> string) =
            let order = Order <| new Guid(form_data fielddata "order_id")
            let product = Product <| new Guid(form_data fielddata "product_id")
            let quantity = Int32.Parse(form_data fielddata "quantity")
            let command = 
                {
                    ``dispatch goods``.``order id`` = order
                    ``product id`` = product
                    quantity = quantity
                }
            checked_handle command 

        let dispatch_goods_form (context:WebContext) (order:Order_info) remaining =
            
            let instock = context.Readmodels.items_in_stock order.Product
            let shippable = List.min [instock;remaining]
            
            if (shippable>0)
            then
             let fields = 
                 [
                     id_field "order_id" order.Id.Id
                     id_field "product_id" order.Product.Id
                     number_field "quantity" shippable "Quantity"
                 ] |> collect
            
             let url = "/logistics/dispatch_goods"
             [
                 "Dispatch good: "
                 form url "confirm !" fields
             ] |> collect
            else
             "-not stocked-"

        let goods_received_form_handler (fielddata:(string*string) list) (checked_handle:obj -> string) =
            let supplier = Domain.Supplier <| new Guid(form_data fielddata "supplier_id")
            let product = Domain.Product <| new Guid(form_data fielddata "product_id")
            let purchaseorder = Domain.Purchaseorder <| new Guid(form_data fielddata "purchase_order_id")
            let quantity = Int32.Parse(form_data fielddata "quantity")
            let command = {``register arrival of goods``.``purchaseorder id``=purchaseorder; ``supplier id``=supplier; ``product id``=product; quantity=quantity}
            checked_handle command 

        let goods_received_form (context:WebContext) (po:Purchase_order_info) =
            let fields = 
                [
                    id_field "product_id" po.Product.Id
                    hidden_field "quantity" (po.Quantity.ToString())
                    id_field "purchase_order_id" po.Id.Id
                    id_field "supplier_id" po.Supplier.Id
                ] |> collect
            
            let url = "/logistics/goods_received"
            [
                "Register goods received: "
                form url "confirm !" fields
            ] |> collect

        let purchase_order_detail_view (context:WebContext) (po:Purchase_order_info)  = 
            let texts = context.Readmodels.entity_list_readmodel ()
            let state = context.Readmodels.purchase_orders_states_readmodel po.Id
            [
                state.ToString()
                texts.[po.Product.Id]
                texts.[po.Supplier.Id]
                po.Quantity.ToString()
                goods_received_form context po
            ] |> datarow

        let purchase_orders_view (context:WebContext)  = 
            let orders = context.Readmodels.open_purchase_orders_readmodel ()            
            [
                section_title "Open purchase orders"
                orders |> Seq.map (purchase_order_detail_view context) |> datatable
            ] |> collect 

        let order_detail_view (context:WebContext) (o:Order_info)  = 
            let texts = context.Readmodels.entity_list_readmodel ()
            let remaining = context.Readmodels.remaining_quantity_in_order o.Id
            [
                o.Ordernumber.ToString()
                texts.[o.Product.Id]
                remaining.ToString()
                o.ShippingAddress
                dispatch_goods_form context o remaining
            ] |> datarow


        let orders_view (context:WebContext)  = 
            let orders = context.Readmodels.open_orders_readmodel ()            
            [
                section_title "Open orders"
                orders |> Seq.map (order_detail_view context) |> Seq.toList |> (datatable2 (datarow ["order #";"product";"quantity to ship";"address";""]))
            ] |> collect 
        

        let product_detail_view (context:WebContext) (product,info) = 
            let instock = context.Readmodels.items_in_stock product
            let allocated = context.Readmodels.items_allocated_to_orders product
            let available = context.Readmodels.available_items product
            let inbound = context.Readmodels.items_in_purchase product
            [
                info
                instock.ToString()
                inbound.ToString()
                allocated.ToString()
                available.ToString()
            ] |> datarow

        let depot_view (context:WebContext) = 
            let available_products = context.Readmodels.product_list_readmodel
            available_products() |> List.map (product_detail_view context) |> datatable2 (["Product";"in stock";"inbound";"allocated";"available"] |> headerrow)
        
        let logistics_home (context:WebContext) = 
            [ 
                depot_view context
                orders_view context
                purchase_orders_view context 
            ] |> collect |> page (context.Company+ " - Depot") context.Menu "logistics"

