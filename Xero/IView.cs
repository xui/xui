namespace Xero;

public abstract partial class UI<T>
{
    public interface IView
    {
        View Render();
    }
}