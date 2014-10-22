// WARNING: GENERATED CODE
// *Never* manually modify the code in this file
namespace Eventsourcing_fs_Demo
module Generated =
    module Domain =
        open System
        
        type Supplier = Supplier of Guid with
            member this.Id = match this with | (Supplier id) -> id
            override this.ToString () = "Supplier@["+(this.Id.ToString())+"]"
        let new_Supplier () = Supplier (Guid.NewGuid())
        
        
        type Product = Product of Guid with
            member this.Id = match this with | (Product id) -> id
            override this.ToString () = "Product@["+(this.Id.ToString())+"]"
        let new_Product () = Product (Guid.NewGuid())
        
        
        type Purchaseorder = Purchaseorder of Guid with
            member this.Id = match this with | (Purchaseorder id) -> id
            override this.ToString () = "Purchaseorder@["+(this.Id.ToString())+"]"
        let new_Purchaseorder () = Purchaseorder (Guid.NewGuid())
        
        
        type Order = Order of Guid with
            member this.Id = match this with | (Order id) -> id
            override this.ToString () = "Order@["+(this.Id.ToString())+"]"
        let new_Order () = Order (Guid.NewGuid())
        
        
        
        type ``Supplier account opened`` = {``supplier id``: Supplier;designation: String} with
            override this.ToString() = 
                let data = "; 'supplier id':'"+this.``supplier id``.ToString()+"'"+"; 'designation':'"+this.designation.ToString()+"'" 
                sprintf "Supplier account opened%s" data
        /// parmeters: supplier id, designation
        let event_Supplier_account_opened (supplier_id: Supplier) (designation: String) : System.Object = {``Supplier account opened``.``supplier id``=supplier_id; designation=designation} :> System.Object
        
        
        type ``Supplier designation changed`` = {``supplier id``: Supplier;designation: String} with
            override this.ToString() = 
                let data = "; 'supplier id':'"+this.``supplier id``.ToString()+"'"+"; 'designation':'"+this.designation.ToString()+"'" 
                sprintf "Supplier designation changed%s" data
        /// parmeters: supplier id, designation
        let event_Supplier_designation_changed (supplier_id: Supplier) (designation: String) : System.Object = {``Supplier designation changed``.``supplier id``=supplier_id; designation=designation} :> System.Object
        
        
        type ``Supplier account closed`` = {``supplier id``: Supplier} with
            override this.ToString() = 
                let data = "; 'supplier id':'"+this.``supplier id``.ToString()+"'" 
                sprintf "Supplier account closed%s" data
        /// parmeters: supplier id
        let event_Supplier_account_closed (supplier_id: Supplier) : System.Object = {``Supplier account closed``.``supplier id``=supplier_id} :> System.Object
        
        
        type ``Product listed`` = {``product id``: Product;designation: String} with
            override this.ToString() = 
                let data = "; 'product id':'"+this.``product id``.ToString()+"'"+"; 'designation':'"+this.designation.ToString()+"'" 
                sprintf "Product listed%s" data
        /// parmeters: product id, designation
        let event_Product_listed (product_id: Product) (designation: String) : System.Object = {``Product listed``.``product id``=product_id; designation=designation} :> System.Object
        
        
        type ``Product designation changed`` = {``product id``: Product;designation: String} with
            override this.ToString() = 
                let data = "; 'product id':'"+this.``product id``.ToString()+"'"+"; 'designation':'"+this.designation.ToString()+"'" 
                sprintf "Product designation changed%s" data
        /// parmeters: product id, designation
        let event_Product_designation_changed (product_id: Product) (designation: String) : System.Object = {``Product designation changed``.``product id``=product_id; designation=designation} :> System.Object
        
        
        type ``Product deprecated`` = {``product id``: Product} with
            override this.ToString() = 
                let data = "; 'product id':'"+this.``product id``.ToString()+"'" 
                sprintf "Product deprecated%s" data
        /// parmeters: product id
        let event_Product_deprecated (product_id: Product) : System.Object = {``Product deprecated``.``product id``=product_id} :> System.Object
        
        
        type ``Received product availability annoucement`` = {``supplier id``: Supplier;``product id``: Product;``quantity available``: Int32;``price per quantity``: Common.Currency} with
            override this.ToString() = 
                let data = "; 'supplier id':'"+this.``supplier id``.ToString()+"'"+"; 'product id':'"+this.``product id``.ToString()+"'"+"; 'quantity available':'"+this.``quantity available``.ToString()+"'"+"; 'price per quantity':'"+this.``price per quantity``.ToString()+"'" 
                sprintf "Received product availability annoucement%s" data
        /// parmeters: supplier id, product id, quantity available, price per quantity
        let event_Received_product_availability_annoucement (supplier_id: Supplier) (product_id: Product) (quantity_available: Int32) (price_per_quantity: Common.Currency) : System.Object = {``Received product availability annoucement``.``supplier id``=supplier_id; ``product id``=product_id; ``quantity available``=quantity_available; ``price per quantity``=price_per_quantity} :> System.Object
        
        
        type ``Received product delisting annoucement`` = {``supplier id``: Supplier;``product id``: Product} with
            override this.ToString() = 
                let data = "; 'supplier id':'"+this.``supplier id``.ToString()+"'"+"; 'product id':'"+this.``product id``.ToString()+"'" 
                sprintf "Received product delisting annoucement%s" data
        /// parmeters: supplier id, product id
        let event_Received_product_delisting_annoucement (supplier_id: Supplier) (product_id: Product) : System.Object = {``Received product delisting annoucement``.``supplier id``=supplier_id; ``product id``=product_id} :> System.Object
        
        
        type ``Purchase order submitted to supplier`` = {``purchaseorder id``: Purchaseorder;``supplier id``: Supplier;``product id``: Product;quantity: Int32} with
            override this.ToString() = 
                let data = "; 'purchaseorder id':'"+this.``purchaseorder id``.ToString()+"'"+"; 'supplier id':'"+this.``supplier id``.ToString()+"'"+"; 'product id':'"+this.``product id``.ToString()+"'"+"; 'quantity':'"+this.quantity.ToString()+"'" 
                sprintf "Purchase order submitted to supplier%s" data
        /// parmeters: purchaseorder id, supplier id, product id, quantity
        let event_Purchase_order_submitted_to_supplier (purchaseorder_id: Purchaseorder) (supplier_id: Supplier) (product_id: Product) (quantity: Int32) : System.Object = {``Purchase order submitted to supplier``.``purchaseorder id``=purchaseorder_id; ``supplier id``=supplier_id; ``product id``=product_id; quantity=quantity} :> System.Object
        
        
        type ``Purchase order confirmed by supplier`` = {``purchaseorder id``: Purchaseorder;``supplier id``: Supplier;``product id``: Product;``quantity shipped``: Int32;``price per quantity``: Common.Currency} with
            override this.ToString() = 
                let data = "; 'purchaseorder id':'"+this.``purchaseorder id``.ToString()+"'"+"; 'supplier id':'"+this.``supplier id``.ToString()+"'"+"; 'product id':'"+this.``product id``.ToString()+"'"+"; 'quantity shipped':'"+this.``quantity shipped``.ToString()+"'"+"; 'price per quantity':'"+this.``price per quantity``.ToString()+"'" 
                sprintf "Purchase order confirmed by supplier%s" data
        /// parmeters: purchaseorder id, supplier id, product id, quantity shipped, price per quantity
        let event_Purchase_order_confirmed_by_supplier (purchaseorder_id: Purchaseorder) (supplier_id: Supplier) (product_id: Product) (quantity_shipped: Int32) (price_per_quantity: Common.Currency) : System.Object = {``Purchase order confirmed by supplier``.``purchaseorder id``=purchaseorder_id; ``supplier id``=supplier_id; ``product id``=product_id; ``quantity shipped``=quantity_shipped; ``price per quantity``=price_per_quantity} :> System.Object
        
        
        type ``Purchase order rejected by supplier`` = {``purchaseorder id``: Purchaseorder;``supplier id``: Supplier} with
            override this.ToString() = 
                let data = "; 'purchaseorder id':'"+this.``purchaseorder id``.ToString()+"'"+"; 'supplier id':'"+this.``supplier id``.ToString()+"'" 
                sprintf "Purchase order rejected by supplier%s" data
        /// parmeters: purchaseorder id, supplier id
        let event_Purchase_order_rejected_by_supplier (purchaseorder_id: Purchaseorder) (supplier_id: Supplier) : System.Object = {``Purchase order rejected by supplier``.``purchaseorder id``=purchaseorder_id; ``supplier id``=supplier_id} :> System.Object
        
        
        type ``Supplier fulfilled purchase order`` = {``supplier id``: Supplier;``purchaseorder id``: Purchaseorder} with
            override this.ToString() = 
                let data = "; 'supplier id':'"+this.``supplier id``.ToString()+"'"+"; 'purchaseorder id':'"+this.``purchaseorder id``.ToString()+"'" 
                sprintf "Supplier fulfilled purchase order%s" data
        /// parmeters: supplier id, purchaseorder id
        let event_Supplier_fulfilled_purchase_order (supplier_id: Supplier) (purchaseorder_id: Purchaseorder) : System.Object = {``Supplier fulfilled purchase order``.``supplier id``=supplier_id; ``purchaseorder id``=purchaseorder_id} :> System.Object
        
        
        type ``Goods received in depot`` = {``product id``: Product;quantity: Int32;``purchaseorder id``: Purchaseorder;``supplier id``: Supplier} with
            override this.ToString() = 
                let data = "; 'product id':'"+this.``product id``.ToString()+"'"+"; 'quantity':'"+this.quantity.ToString()+"'"+"; 'purchaseorder id':'"+this.``purchaseorder id``.ToString()+"'"+"; 'supplier id':'"+this.``supplier id``.ToString()+"'" 
                sprintf "Goods received in depot%s" data
        /// parmeters: product id, quantity, purchaseorder id, supplier id
        let event_Goods_received_in_depot (product_id: Product) (quantity: Int32) (purchaseorder_id: Purchaseorder) (supplier_id: Supplier) : System.Object = {``Goods received in depot``.``product id``=product_id; quantity=quantity; ``purchaseorder id``=purchaseorder_id; ``supplier id``=supplier_id} :> System.Object
        
        
        type ``Goods dispatched to customer`` = {``order id``: Order;``product id``: Product;quantity: Int32;``shipping address``: String;``shipping expenses``: Common.Currency} with
            override this.ToString() = 
                let data = "; 'order id':'"+this.``order id``.ToString()+"'"+"; 'product id':'"+this.``product id``.ToString()+"'"+"; 'quantity':'"+this.quantity.ToString()+"'"+"; 'shipping address':'"+this.``shipping address``.ToString()+"'"+"; 'shipping expenses':'"+this.``shipping expenses``.ToString()+"'" 
                sprintf "Goods dispatched to customer%s" data
        /// parmeters: order id, product id, quantity, shipping address, shipping expenses
        let event_Goods_dispatched_to_customer (order_id: Order) (product_id: Product) (quantity: Int32) (shipping_address: String) (shipping_expenses: Common.Currency) : System.Object = {``Goods dispatched to customer``.``order id``=order_id; ``product id``=product_id; quantity=quantity; ``shipping address``=shipping_address; ``shipping expenses``=shipping_expenses} :> System.Object
        
        
        type ``Customer order accepted`` = {``order id``: Order;``order number``: Int32;``shipping address``: String;``product id``: Product;quantity: Int32;``net price``: Common.Currency;``shipping cost``: Common.Currency} with
            override this.ToString() = 
                let data = "; 'order id':'"+this.``order id``.ToString()+"'"+"; 'order number':'"+this.``order number``.ToString()+"'"+"; 'shipping address':'"+this.``shipping address``.ToString()+"'"+"; 'product id':'"+this.``product id``.ToString()+"'"+"; 'quantity':'"+this.quantity.ToString()+"'"+"; 'net price':'"+this.``net price``.ToString()+"'"+"; 'shipping cost':'"+this.``shipping cost``.ToString()+"'" 
                sprintf "Customer order accepted%s" data
        /// parmeters: order id, order number, shipping address, product id, quantity, net price, shipping cost
        let event_Customer_order_accepted (order_id: Order) (order_number: Int32) (shipping_address: String) (product_id: Product) (quantity: Int32) (net_price: Common.Currency) (shipping_cost: Common.Currency) : System.Object = {``Customer order accepted``.``order id``=order_id; ``order number``=order_number; ``shipping address``=shipping_address; ``product id``=product_id; quantity=quantity; ``net price``=net_price; ``shipping cost``=shipping_cost} :> System.Object
        
        
        type ``Customer order fulfilled`` = {``order id``: Order} with
            override this.ToString() = 
                let data = "; 'order id':'"+this.``order id``.ToString()+"'" 
                sprintf "Customer order fulfilled%s" data
        /// parmeters: order id
        let event_Customer_order_fulfilled (order_id: Order) : System.Object = {``Customer order fulfilled``.``order id``=order_id} :> System.Object
        
        
        type ``Order number allocated`` = {``order number``: Int32;``order id``: Order} with
            override this.ToString() = 
                let data = "; 'order number':'"+this.``order number``.ToString()+"'"+"; 'order id':'"+this.``order id``.ToString()+"'" 
                sprintf "Order number allocated%s" data
        /// parmeters: order number, order id
        let event_Order_number_allocated (order_number: Int32) (order_id: Order) : System.Object = {``Order number allocated``.``order number``=order_number; ``order id``=order_id} :> System.Object
        
        
    

