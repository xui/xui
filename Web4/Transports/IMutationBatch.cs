namespace Web4.Transports;

public interface IMutationBatch : IDisposable
{
    void SetTextNode(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole);
    void SetAttribute(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole);
    void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes);
    void ReplaceElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes);
    void AddElement(string key, int index, Span<Keyhole> keyholes);
    void RemoveElement(string key, int index, Span<Keyhole> keyholes);
    void MoveElement(string key, int from, int to);
    void Commit();
}