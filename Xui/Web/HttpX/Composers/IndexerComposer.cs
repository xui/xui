using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class IndexerComposer(int slotId) : BaseComposer
{
    public Func<Event?, Task>? EventHandler { get; set; } = null;

    public override void AppendFormatted(Action a)
    {
        base.AppendFormatted(a);
    }

    public override void AppendFormatted(Action<Event> a)
    {
        base.AppendFormatted(a);
    }

    public override void AppendFormatted(Func<Task> f)
    {
        base.AppendFormatted(f);
    }

    public override void AppendFormatted(Func<Event, Task> f)
    {
        base.AppendFormatted(f);
    }
}