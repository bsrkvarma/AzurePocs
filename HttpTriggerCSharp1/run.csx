using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, IAsyncCollector<Order> orderItem)
{
    log.Info("New order placed.");

    // Get request body
    var order = await req.Content.ReadAsAsync<Order>();    
    if(order!=null)
        await orderItem.AddAsync(order);
    return order == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, $"{order.CustomerName} ordered {order.Product}");
}

public class Order
{
    public string Product { get; set;}
    public string CustomerName { get; set;}
    public string Rate { get; set;}
    public string Phone { get; set;}
}
