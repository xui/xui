using Web4.Core.DOM;

namespace Web4.WebSockets;

public interface IApp
{
    public void SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole);

    public void SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole);

    public void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes);

    public void SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition = null);

    public void AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition = null);

    public void RemoveElement(string key, string? transition = null);
}

public partial class WebSocketTransport : IApp
{
    public void SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setTextNode"),
            param: ref newKeyhole
        );
    }

    public void SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setAttribute"),
            param: ref newKeyhole
        );
    }

    public void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", key, "setAttribute"),
            newKeyholes,
            includeSentinels: false
        );
    }

    public void SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition = null)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", key, "setElement"),
            newKeyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    public void AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition = null)
    {
        BatchWriter.WriteNotification(
            method: ("app.keyholes", priorKey, "addElement"),
            param1: key,
            param2: keyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    public void RemoveElement(string key, string? transition = null)
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
