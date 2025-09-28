using Web4.Core.DOM;

namespace Web4.WebSockets;

public partial class WebSocketTransport : IApp
{
    void IApp.SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setTextNode"),
            param: ref newKeyhole
        );
    }

    void IApp.SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setAttribute"),
            param: ref newKeyhole
        );
    }

    void IApp.SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", key, "setAttribute"),
            newKeyholes,
            includeSentinels: false
        );
    }

    void IApp.SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", key, "setElement"),
            newKeyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    void IApp.AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", priorKey, "addElement"),
            param1: key,
            param2: keyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    void IApp.RemoveElement(string key, string? transition)
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
