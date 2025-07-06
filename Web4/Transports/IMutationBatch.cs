namespace Web4.Transports;

public interface IMutationBatch
{
    void UpdateValue(string key, ref Keyhole before, ref Keyhole after);
    void UpdateAttribute(string key, Span<Keyhole> before, Span<Keyhole> after);
    void UpdatePartial(string key, Span<Keyhole> before, Span<Keyhole> after);
    void AddPartial(string key, int index, Span<Keyhole> partial);
    void RemovePartial(string key, int index, Span<Keyhole> partial);
    void MovePartial(string key, int from, int to);
    void Commit();
}