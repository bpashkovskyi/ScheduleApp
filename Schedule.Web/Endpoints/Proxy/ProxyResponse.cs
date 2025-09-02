namespace Schedule.Web.Endpoints.Proxy;

public sealed class ProxyResponse
{
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
}
