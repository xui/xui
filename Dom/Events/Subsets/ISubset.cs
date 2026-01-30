namespace Web4.Events.Subsets;

public interface ISubset
{
    void StopPropagation();
    void StopImmediatePropagation();
}