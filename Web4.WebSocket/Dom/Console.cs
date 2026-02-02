using Web4.Dom;

namespace Web4.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
// TODO: Ignore that.  Make it an `internal struct` like LazyEvent?
internal partial class WebSocketTransport : IConsole
{
    // TODO: Tests still needed for each one.
    void IConsole.Assert(bool assertion, string message)
        => jsonRpc.WriteNotification("console.assert", assertion ? "true" : "false", message);
    void IConsole.Assert(bool assertion, string message, params Span<string> substitutions)
        => jsonRpc.WriteNotification("console.assert", assertion ? "true" : "false", message);
    void IConsole.Assert(bool assertion, object value)
        => jsonRpc.WriteNotification("console.assert", assertion ? "true" : "false", value);
    void IConsole.Assert(bool assertion, params Span<object> values)
        => jsonRpc.WriteNotification("console.assert", [assertion ? "true" : "false", ..values]);
    void IConsole.Clear()
        => jsonRpc.WriteNotification("console.clear");
    void IConsole.Count()
        => jsonRpc.WriteNotification("console.count");
    void IConsole.Count(object label)
        => jsonRpc.WriteNotification("console.count", label);
    void IConsole.CountReset()
        => jsonRpc.WriteNotification("console.reset");
    void IConsole.CountReset(object label)
        => jsonRpc.WriteNotification("console.reset", label);
    void IConsole.Debug(string message)
        => jsonRpc.WriteNotification("console.debug", message);
    void IConsole.Debug(string message, params Span<string> substitutions)
        => jsonRpc.WriteNotification("console.debug", message, substitutions);
    void IConsole.Debug(object value)
        => jsonRpc.WriteNotification("console.debug", value);
    void IConsole.Debug(params Span<object> values)
        => jsonRpc.WriteNotification("console.debug", values);
    void IConsole.Dir(object obj)
        => jsonRpc.WriteNotification("console.dir", obj);
    void IConsole.Dir(object obj, IConsole.DirOptions options)
        => throw new NotImplementedException();
    void IConsole.Error(string message)
        => jsonRpc.WriteNotification("console.error", message);
    void IConsole.Error(string message, params Span<string> substitutions)
        => jsonRpc.WriteNotification("console.error", message, substitutions);
    void IConsole.Error(object value)
        => jsonRpc.WriteNotification("console.error", value);
    void IConsole.Error(object value, params Span<object> values)
        => jsonRpc.WriteNotification("console.error", [value, ..values]);
    void IConsole.Group()
        => jsonRpc.WriteNotification("console.group");
    void IConsole.Group(object label)
        => jsonRpc.WriteNotification("console.group", label);
    void IConsole.GroupCollapsed()
        => jsonRpc.WriteNotification("console.groupCollapsed");
    void IConsole.GroupCollapsed(object label)
        => jsonRpc.WriteNotification("console.groupCollapsed", label);
    void IConsole.GroupCollapsed(string message, params Span<string> substitutions)
        => jsonRpc.WriteNotification("console.groupCollapsed", message, substitutions);
    void IConsole.GroupEnd()
        => jsonRpc.WriteNotification("console.groupEnd");
    void IConsole.Info(string message)
        => jsonRpc.WriteNotification("console.info", message);
    void IConsole.Info(string message, params Span<string> substitutions)
        => jsonRpc.WriteNotification("console.info", message, substitutions);
    void IConsole.Info(object value)
        => jsonRpc.WriteNotification("console.info", value);
    void IConsole.Info(object value, params Span<object> values)
        => jsonRpc.WriteNotification("console.info", [value, ..values]);
    void IConsole.Log(string message)
        => jsonRpc.WriteNotification("console.log", message);
    void IConsole.Log(string message, params Span<string> substitutions)
        => jsonRpc.WriteNotification("console.log", message, substitutions);
    void IConsole.Log(object value)
        => jsonRpc.WriteNotification("console.log", value);
    void IConsole.Log(object value, params Span<object> values)
        => jsonRpc.WriteNotification("console.log", [value, ..values]);
    void IConsole.Table(object obj)
        => jsonRpc.WriteNotification("console.table", obj);
    void IConsole.Table(object obj, string[] columns)
        => jsonRpc.WriteNotification("console.table", obj, columns);
    void IConsole.Time()
        => jsonRpc.WriteNotification("console.time");
    void IConsole.Time(object label)
        => jsonRpc.WriteNotification("console.time", label);
    void IConsole.TimeEnd()
        => jsonRpc.WriteNotification("console.timeEnd");
    void IConsole.TimeEnd(object label)
        => jsonRpc.WriteNotification("console.timeEnd", label);
    void IConsole.TimeLog()
        => jsonRpc.WriteNotification("console.timeLog");
    void IConsole.TimeLog(object label)
        => jsonRpc.WriteNotification("console.timeLog", label);
    void IConsole.TimeLog(object label, params Span<object> values)
        => jsonRpc.WriteNotification("console.timeLog", [label, ..values]);
    void IConsole.Trace()
        => jsonRpc.WriteNotification("console.trace");
    void IConsole.Trace(params Span<object> objects)
        => jsonRpc.WriteNotification("console.trace", objects);
    void IConsole.Warn(string message)
        => jsonRpc.WriteNotification("console.warn", message);
    void IConsole.Warn(string message, params Span<string> substitutions)
        => jsonRpc.WriteNotification("console.warn", message, substitutions);
    void IConsole.Warn(object value)
        => jsonRpc.WriteNotification("console.warn", value);
    void IConsole.Warn(object value, params Span<object> values)
        => jsonRpc.WriteNotification("console.warn", [value, ..values]);
}