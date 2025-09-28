using Web4.Core.DOM;

namespace Web4.WebSockets;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
public partial class WebSocketTransport : IKeyholes
{
    void IKeyholes.SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setTextNode"),
            param: ref newKeyhole
        );
    }

    void IKeyholes.SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setAttribute"),
            param: ref newKeyhole
        );
    }

    void IKeyholes.SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", key, "setAttribute"),
            newKeyholes,
            includeSentinels: false
        );
    }

    void IKeyholes.SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", key, "setElement"),
            newKeyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    void IKeyholes.AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", priorKey, "addElement"),
            param1: key,
            param2: keyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    void IKeyholes.RemoveElement(string key, string? transition)
    {
        if (transition is null)
            BatchWriter.WriteNotification(
                method: ("app.keyholes", key, "removeElement")
            );
        else
            BatchWriter.WriteNotification(
                method: ("app.keyholes", key, "removeElement"),
                param: transition
            );
    }
}
