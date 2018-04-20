using System;

public static void Run(Order orderItem, TraceWriter log)
{    
    log.Info($"Order picked from Queue: {orderItem.CustomerName} ordered {orderItem.Product}");
}


public class Order
{
    public string Product { get; set;}
    public string CustomerName { get; set;}
    public string Rate { get; set;}
    public string Phone { get; set;}
}
