namespace OddsPushClient.Services;

public interface IHeartbeatMonitor
{
    void RecordHeartbeat();
    bool IsServiceAvailable();
    DateTime? GetLastHeartbeatTime();
}

public class HeartbeatMonitor : IHeartbeatMonitor
{
    private DateTime? _lastHeartbeat = DateTime.UtcNow;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(10);

    public void RecordHeartbeat()
    {
        _lastHeartbeat = DateTime.UtcNow;
    }

    public bool IsServiceAvailable()
    {
        if (!_lastHeartbeat.HasValue) return false;
        return (DateTime.UtcNow - _lastHeartbeat.Value) <= _timeout;
    }

    public DateTime? GetLastHeartbeatTime() => _lastHeartbeat;
}
