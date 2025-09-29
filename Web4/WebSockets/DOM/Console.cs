using Web4.Core.DOM;namespace Web4.WebSockets;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
public partial class WebSocketTransport : IConsole
{
    // TODO: Tests still needed for each one.
    void IConsole.Assert(bool assertion, string message)
        => BatchWriter.WriteNotification("console.assert", assertion ? "true" : "false", message);
    void IConsole.Assert(bool assertion, string message, params Span<string> substitutions)
        => BatchWriter.WriteNotification("console.assert", assertion ? "true" : "false", message);
    void IConsole.Assert(bool assertion, object value)
        => BatchWriter.WriteNotification("console.assert", assertion ? "true" : "false", value);
    void IConsole.Assert(bool assertion, params Span<object> values)
        => BatchWriter.WriteNotification("console.assert", [assertion ? "true" : "false", ..values]);
    void IConsole.Clear()
        => BatchWriter.WriteNotification("console.clear");
    void IConsole.Count()
        => BatchWriter.WriteNotification("console.count");
    void IConsole.Count(object label)
        => BatchWriter.WriteNotification("console.count", label);
    void IConsole.CountReset()
        => BatchWriter.WriteNotification("console.reset");
    void IConsole.CountReset(object label)
        => BatchWriter.WriteNotification("console.reset", label);
    void IConsole.Debug(string message)
        => BatchWriter.WriteNotification("console.debug", message);
    void IConsole.Debug(string message, params Span<string> substitutions)
        => BatchWriter.WriteNotification("console.debug", message, substitutions);
    void IConsole.Debug(object value)
        => BatchWriter.WriteNotification("console.debug", value);
    void IConsole.Debug(params Span<object> values)
        => BatchWriter.WriteNotification("console.debug", values);
    void IConsole.Dir(object obj)
        => BatchWriter.WriteNotification("console.dir", obj);
    void IConsole.Dir(object obj, IConsole.DirOptions options)
        => throw new NotImplementedException();
    void IConsole.Error(string message)
        => BatchWriter.WriteNotification("console.error", message);
    void IConsole.Error(string message, params Span<string> substitutions)
        => BatchWriter.WriteNotification("console.error", message, substitutions);
    void IConsole.Error(object value)
        => BatchWriter.WriteNotification("console.error", value);
    void IConsole.Error(object value, params Span<object> values)
        => BatchWriter.WriteNotification("console.error", [value, ..values]);
    void IConsole.Group()
        => BatchWriter.WriteNotification("console.group");
    void IConsole.Group(object label)
        => BatchWriter.WriteNotification("console.group", label);
    void IConsole.GroupCollapsed()
        => BatchWriter.WriteNotification("console.groupCollapsed");
    void IConsole.GroupCollapsed(object label)
        => BatchWriter.WriteNotification("console.groupCollapsed", label);
    void IConsole.GroupEnd()
        => BatchWriter.WriteNotification("console.groupEnd");
    void IConsole.Info(string message)
        => BatchWriter.WriteNotification("console.info", message);
    void IConsole.Info(string message, params Span<string> substitutions)
        => BatchWriter.WriteNotification("console.info", message, substitutions);
    void IConsole.Info(object value)
        => BatchWriter.WriteNotification("console.info", value);
    void IConsole.Info(object value, params Span<object> values)
        => BatchWriter.WriteNotification("console.info", [value, ..values]);
    void IConsole.Log(string message)
        => BatchWriter.WriteNotification("console.log", message);
    void IConsole.Log(string message, params Span<string> substitutions)
        => BatchWriter.WriteNotification("console.log", message, substitutions);
    void IConsole.Log(object value)
        => BatchWriter.WriteNotification("console.log", value);
    void IConsole.Log(object value, params Span<object> values)
        => BatchWriter.WriteNotification("console.log", [value, ..values]);
    void IConsole.Table(object obj)
        => BatchWriter.WriteNotification("console.table", obj);
    void IConsole.Table(object obj, string[] columns)
        => BatchWriter.WriteNotification("console.table", obj, columns);
    void IConsole.Time()
        => BatchWriter.WriteNotification("console.time");
    void IConsole.Time(object label)
        => BatchWriter.WriteNotification("console.time", label);
    void IConsole.TimeEnd()
        => BatchWriter.WriteNotification("console.timeEnd");
    void IConsole.TimeEnd(object label)
        => BatchWriter.WriteNotification("console.timeEnd", label);
    void IConsole.TimeLog()
        => BatchWriter.WriteNotification("console.timeLog");
    void IConsole.TimeLog(object label)
        => BatchWriter.WriteNotification("console.timeLog", label);
    void IConsole.TimeLog(object label, params Span<object> values)
        => BatchWriter.WriteNotification("console.timeLog", [label, ..values]);
    void IConsole.Trace()
        => BatchWriter.WriteNotification("console.trace");
    void IConsole.Trace(params Span<object> objects)
        => BatchWriter.WriteNotification("console.trace", objects);
    void IConsole.Warn(string message)
        => BatchWriter.WriteNotification("console.warn", message);
    void IConsole.Warn(string message, params Span<string> substitutions)
        => BatchWriter.WriteNotification("console.warn", message, substitutions);
    void IConsole.Warn(object value)
        => BatchWriter.WriteNotification("console.warn", value);
    void IConsole.Warn(object value, params Span<object> values)
        => BatchWriter.WriteNotification("console.warn", [value, ..values]);
}