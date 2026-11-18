// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Severity classification for log output. Values follow the conventional ordering from
///     most-verbose (<see cref="Trace" />) to no-output (<see cref="None" />).
/// </summary>
public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical,
    None
}