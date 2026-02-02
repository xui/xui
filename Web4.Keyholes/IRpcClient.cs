using Web4.Dom;

namespace Web4.Keyholes;

public interface IRpcClient
{
    void SetText(byte[] key, ref Keyhole keyhole);
    void SetAttribute(byte[] key, ref Keyhole keyhole);
    void SetAttribute(byte[] key, Span<Keyhole> keyholes);
    void SetNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, int>? viewTransitionNameNew = null, ValueTuple<string, int>? viewTransitionNameOld = null);
    void SetNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, byte[]> viewTransitionName);
    void PushNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, byte[] newKey);
    void PushNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, byte[] newKey, ValueTuple<string, int> viewTransitionName);
    void PopNode(byte[] key);
    void PopNode(byte[] key, ValueTuple<string, int> viewTransitionName);
}
