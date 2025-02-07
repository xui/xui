namespace Web4;

public delegate Html Slot();

public interface IView
{
    Html Render();
}
