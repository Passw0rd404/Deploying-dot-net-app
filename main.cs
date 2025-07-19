using System;
using System.Net;
using System.Text;

var facts = new[] { "AWS S3 has 11 9s of durability", "S3 scales automatically", "S3 supports multiple storage classes" };

using var listener = new HttpListener();
listener.Prefixes.Add("http://*:8002/");
listener.Start();

Console.WriteLine("Server running on port 8002");

while (true)
{
    var context = await listener.GetContextAsync();
    var response = context.Response;
    
    if (context.Request.Url?.AbsolutePath == "/health")
    {
        var healthText = "{\"status\":\"ok\"}";
        var healthBytes = Encoding.UTF8.GetBytes(healthText);
        response.ContentType = "application/json";
        await response.OutputStream.WriteAsync(healthBytes);
    }
    else
    {
        var fact = facts[Random.Shared.Next(facts.Length)];
        var responseText = $"{DateTime.Now:HH:mm:ss} - {fact}";
        var bytes = Encoding.UTF8.GetBytes(responseText);
        response.ContentType = "text/plain";
        await response.OutputStream.WriteAsync(bytes);
    }
    
    response.Close();
}