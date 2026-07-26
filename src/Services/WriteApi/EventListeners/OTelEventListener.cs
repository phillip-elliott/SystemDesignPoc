using System.Diagnostics.Tracing;

// Temporary diagnostic listener to capture OpenTelemetry internal SDK logs
public class OTelEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name.StartsWith("OpenTelemetry"))
        {
            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventArgs)
    {
        var message = string.Format(eventArgs.Message ?? "", eventArgs.Payload?.ToArray() ?? Array.Empty<object>());
        Console.WriteLine($"[OTEL-INTERNAL] [{eventArgs.Level}] {eventArgs.EventSource.Name}: {message}");
    }
}