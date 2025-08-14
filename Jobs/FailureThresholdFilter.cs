using System.Text;
using System.Text.Json;
using Hangfire.Server;

namespace UmbraCalendar.Jobs;


public class FailureThresholdFilter : IServerFilter
{
    private static int _failureCount = 0;
    private readonly IConfiguration _configuration;

    public FailureThresholdFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnPerforming(PerformingContext filterContext) { }

    public void OnPerformed(PerformedContext filterContext)
    {
        int.TryParse(_configuration["ExternalServices:ErrorThreshold"], out var errorThreshold);
        if (errorThreshold == 0)
        {
            errorThreshold = 25;
        }
        
        if (filterContext.Exception == null)
        {
            return;
        }
        
        _failureCount++;

        if (_failureCount <= errorThreshold)
        {
            return;
        }
        
        _ = NotifyAdmins(_failureCount, filterContext);
        _failureCount = 0; // reset counter after notification
    }

    private async Task NotifyAdmins(int count, PerformedContext filterContext)
    {
        var slackToken = _configuration["ExternalServices:SlackToken"];
        var slackChannel = _configuration["ExternalServices:SlackChannel"];
        
        var args = string.Join(",", filterContext.BackgroundJob.Job.Args
            .Where(o => o != null && !string.IsNullOrWhiteSpace(o.ToString()))
            .Select(o => o.ToString()));
        var messagePostfix = string.Empty;
        if (args.Length > 0)
        {
            messagePostfix = $" with args: `{args}`";
        }
        var method = filterContext.BackgroundJob.Job.Method.Name;
        var methodNamespace = filterContext.BackgroundJob.Job.Method.DeclaringType?.Namespace;
        var fullMethodName = $"{methodNamespace}.{method}";
        if (methodNamespace == string.Empty)
        {
            fullMethodName = method;
        }
        
        var message = $"UmbraCalendar (server `{filterContext.ServerId}`): {count} consecutive job failures detected. Threshold reached while working on `{fullMethodName}`{messagePostfix}.";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", slackToken);

        var payload = new
        {
            channel = slackChannel,
            text = message
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://slack.com/api/chat.postMessage", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine(responseBody);
    }
}