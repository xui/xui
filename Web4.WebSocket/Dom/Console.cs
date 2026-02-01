using Web4.Dom;

namespace Web4.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
// TODO: Ignore that.  Make it an `internal struct` like LazyEvent?
internal partial class WebSocketTransport : IConsole
{
    // TODO: Tests still needed for each one.
    void IConsole.Assert(bool assertion, string message)
        => Output.WriteNotification("console.assert", assertion ? "true" : "false", message);
    void IConsole.Assert(bool assertion, string message, params Span<string> substitutions)
        => Output.WriteNotification("console.assert", assertion ? "true" : "false", message);
    void IConsole.Assert(bool assertion, object value)
        => Output.WriteNotification("console.assert", assertion ? "true" : "false", value);
    void IConsole.Assert(bool assertion, params Span<object> values)
        => Output.WriteNotification("console.assert", [assertion ? "true" : "false", ..values]);
    void IConsole.Clear()
        => Output.WriteNotification("console.clear");
    void IConsole.Count()
        => Output.WriteNotification("console.count");
    void IConsole.Count(object label)
        => Output.WriteNotification("console.count", label);
    void IConsole.CountReset()
        => Output.WriteNotification("console.reset");
    void IConsole.CountReset(object label)
        => Output.WriteNotification("console.reset", label);
    void IConsole.Debug(string message)
        => Output.WriteNotification("console.debug", message);
    void IConsole.Debug(string message, params Span<string> substitutions)
        => Output.WriteNotification("console.debug", message, substitutions);
    void IConsole.Debug(object value)
        => Output.WriteNotification("console.debug", value);
    void IConsole.Debug(params Span<object> values)
        => Output.WriteNotification("console.debug", values);
    void IConsole.Dir(object obj)
        => Output.WriteNotification("console.dir", obj);
    void IConsole.Dir(object obj, IConsole.DirOptions options)
        => throw new NotImplementedException();
    void IConsole.Error(string message)
        => Output.WriteNotification("console.error", message);
    void IConsole.Error(string message, params Span<string> substitutions)
        => Output.WriteNotification("console.error", message, substitutions);
    void IConsole.Error(object value)
        => Output.WriteNotification("console.error", value);
    void IConsole.Error(object value, params Span<object> values)
        => Output.WriteNotification("console.error", [value, ..values]);
    void IConsole.Group()
        => Output.WriteNotification("console.group");
    void IConsole.Group(object label)
        => Output.WriteNotification("console.group", label);
    void IConsole.GroupCollapsed()
        => Output.WriteNotification("console.groupCollapsed");
    void IConsole.GroupCollapsed(object label)
        => Output.WriteNotification("console.groupCollapsed", label);
    void IConsole.GroupCollapsed(string message, params Span<string> substitutions)
        => Output.WriteNotification("console.groupCollapsed", message, substitutions);
    void IConsole.GroupEnd()
        => Output.WriteNotification("console.groupEnd");
    void IConsole.Info(string message)
        => Output.WriteNotification("console.info", message);
    void IConsole.Info(string message, params Span<string> substitutions)
        => Output.WriteNotification("console.info", message, substitutions);
    void IConsole.Info(object value)
        => Output.WriteNotification("console.info", value);
    void IConsole.Info(object value, params Span<object> values)
        => Output.WriteNotification("console.info", [value, ..values]);
    void IConsole.Log(string message)
        => Output.WriteNotification("console.log", message);
    void IConsole.Log(string message, params Span<string> substitutions)
        => Output.WriteNotification("console.log", message, substitutions);
    void IConsole.Log(object value)
        => Output.WriteNotification("console.log", value);
    void IConsole.Log(object value, params Span<object> values)
        => Output.WriteNotification("console.log", [value, ..values]);
    void IConsole.Table(object obj)
        => Output.WriteNotification("console.table", obj);
    void IConsole.Table(object obj, string[] columns)
        => Output.WriteNotification("console.table", obj, columns);
    void IConsole.Time()
        => Output.WriteNotification("console.time");
    void IConsole.Time(object label)
        => Output.WriteNotification("console.time", label);
    void IConsole.TimeEnd()
        => Output.WriteNotification("console.timeEnd");
    void IConsole.TimeEnd(object label)
        => Output.WriteNotification("console.timeEnd", label);
    void IConsole.TimeLog()
        => Output.WriteNotification("console.timeLog");
    void IConsole.TimeLog(object label)
        => Output.WriteNotification("console.timeLog", label);
    void IConsole.TimeLog(object label, params Span<object> values)
        => Output.WriteNotification("console.timeLog", [label, ..values]);
    void IConsole.Trace()
        => Output.WriteNotification("console.trace");
    void IConsole.Trace(params Span<object> objects)
        => Output.WriteNotification("console.trace", objects);
    void IConsole.Warn(string message)
        => Output.WriteNotification("console.warn", message);
    void IConsole.Warn(string message, params Span<string> substitutions)
        => Output.WriteNotification("console.warn", message, substitutions);
    void IConsole.Warn(object value)
        => Output.WriteNotification("console.warn", value);
    void IConsole.Warn(object value, params Span<object> values)
        => Output.WriteNotification("console.warn", [value, ..values]);
}