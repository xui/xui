namespace Web4.Transports;

public interface IMutationBatch : IDisposable
{
    void UpdateValue(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole);
    void UpdateAttribute(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole);
    void UpdateAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes);
    void UpdatePartial(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes);
    void AddPartial(string key, int index, Span<Keyhole> keyholes);
    void RemovePartial(string key, int index, Span<Keyhole> keyholes);
    void MovePartial(string key, int from, int to);
    void Commit();
}