using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Sessions;

public sealed class RunSessionCoordinator
{
    private CodexSessionState? _currentSession;

    public CodexSessionState? CurrentSession => _currentSession;

    public void BeginRun(string runId)
    {
        if (_currentSession is not null && string.Equals(_currentSession.RunId, runId, StringComparison.Ordinal))
        {
            return;
        }

        _currentSession = null;
    }

    public void Restore(CodexSessionState session)
    {
        _currentSession = session;
    }

    public void Update(string runId, string sessionId)
    {
        var now = DateTimeOffset.UtcNow;
        _currentSession = _currentSession is { RunId: var existingRunId } && string.Equals(existingRunId, runId, StringComparison.Ordinal)
            ? _currentSession with { SessionId = sessionId, UpdatedAt = now }
            : new CodexSessionState(runId, sessionId, now, now);
    }

    public bool MatchesRun(string? runId)
    {
        return _currentSession is not null && string.Equals(_currentSession.RunId, runId, StringComparison.Ordinal);
    }

    public void Reset()
    {
        _currentSession = null;
    }
}
