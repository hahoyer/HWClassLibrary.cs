// ReSharper disable CheckNamespace
namespace hw.DebugFormatter;

/// <summary>
///     Severity classification for log output. Values follow the conventional ordering from
///     most-verbose (<see cref="Trace" />) to no-output (<see cref="None" />).
/// </summary>
public enum LogLevel
{
    /// <summary>Most detailed diagnostic information; expected to be filtered out in production.</summary>
    Trace
    /// <summary>Information useful while developing or troubleshooting.</summary>
    , Debug
    /// <summary>Routine operational messages.</summary>
    , Information
    /// <summary>Recoverable irregularity worth attention.</summary>
    , Warning
    /// <summary>A failure in the current operation.</summary>
    , Error
    /// <summary>A failure that compromises the whole application.</summary>
    , Critical
    /// <summary>Suppresses output entirely.</summary>
    , None
}