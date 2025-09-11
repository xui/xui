namespace Web4;

public interface IMutationBatch : IDisposable
{
    void SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole);
    void SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole);
    void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes);
    void SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition = null);
    void AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition = null);
    void RemoveElement(string key, string? transition = null);
    void Commit();
}