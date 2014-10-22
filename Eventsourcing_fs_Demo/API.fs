namespace Eventsourcing_fs_Demo

module API =
    open System
    open Common
    open Generated
    open Domain

    // Commands
    type ``submit purchase order`` = {``purchaseorder id``: Purchaseorder;``supplier id``: Supplier;``product id``: Product;quantity: Int32}
    type ``register arrival of goods`` = {``purchaseorder id``: Purchaseorder;``supplier id``: Supplier;``product id``: Product;quantity: Int32}
    
    type ``evaluate order`` = {``order id``: Order; ``product id``: Product; quantity: Int32; ``shipping address``:string; ``net price``:Currency; shipping:Currency}
    type ``dispatch goods`` = {``order id``: Order; ``product id``: Product; quantity: Int32}


