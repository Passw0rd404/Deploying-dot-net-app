using System.Net;
using System.Text;
using System.Text.Json;

var s3Facts = new string[]
{
    "v2 Scale storage resources to meet fluctuating needs with 99.999999999% (11 9s) of data durability.",
    "v2 Store data across Amazon S3 storage classes to reduce costs without upfront investment or hardware refresh cycles.",
    "v2 Protect your data with unmatched security, compliance, and audit capabilities.",
    "v2 Easily manage data at any scale with robust access controls, flexible replication tools, and organization-wide visibility.",
    "v2 Run big data analytics, artificial intelligence (AI), machine learning (ML), and high performance computing (HPC) applications.",
    "v2 Meet Recovery Time Objectives (RTO), Recovery Point Objectives (RPO), and compliance requirements with S3's robust replication features."
};

var startTime = DateTime.UtcNow;
using var cts = new CancellationTokenSource();

// Handle Ctrl+C gracefully
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\nShutting down server...");
};

Console.WriteLine("Starting HTTP listener on port 8002...");

using var listener = new HttpListener();
listener.Prefixes.Add("http://*:8002/");

try
{
    listener.Start();
    Console.WriteLine("Server started. Press Ctrl+C to stop.");

    while (!cts.Token.IsCancellationRequested)
    {
        try
        {
            var context = await listener.GetContextAsync().WaitAsync(cts.Token);
            
            // Handle request asynchronously without blocking
            _ = Task.Run(() => HandleRequestAsync(context), cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting request: {ex.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Server error: {ex.Message}");
}
finally
{
    listener.Close();
    Console.WriteLine("Server stopped.");
}

async Task HandleRequestAsync(HttpListenerContext context)
{
    try
    {
        using var response = context.Response;
        
        // Add health check endpoint
        if (context.Request.Url?.AbsolutePath == "/health")
        {
            await RespondWithHealthCheckAsync(response);
            return;
        }
        
        // Serve random S3 fact
        response.StatusCode = (int)HttpStatusCode.OK;
        response.StatusDescription = "Status OK";
        response.ContentType = "text/plain; charset=utf-8";
        
        var factIndex = Random.Shared.Next(0, s3Facts.Length);
        var factText = s3Facts[factIndex];
        
        Console.WriteLine($"Serving fact #{factIndex}: {factText}");
        
        var responseText = $"{DateTime.Now.TimeOfDay} - {factText}";
        var buffer = Encoding.UTF8.GetBytes(responseText);
        
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in request handler: {ex.Message}");
    }
}

async Task RespondWithHealthCheckAsync(HttpListenerResponse response)
{
    try
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "application/json";
        
        var uptime = DateTime.UtcNow - startTime;
        var healthData = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            uptime = uptime.ToString(@"dd\.hh\:mm\:ss"),
            total_facts = s3Facts.Length
        };
        
        var jsonResponse = JsonSerializer.Serialize(healthData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        var buffer = Encoding.UTF8.GetBytes(jsonResponse);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
        
        Console.WriteLine("Health check requested");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Health check error: {ex.Message}");
    }
}