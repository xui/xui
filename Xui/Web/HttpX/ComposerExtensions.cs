using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX;

public static class ComposerExtensions
{
    // This strange gymnastics is required because InterpolatedStringHandlerArgument
    // must have at least one arg before it in order for the compiler to pick it up right.  
    // Extension methods help create the illusion of a simple composer.Compose($"...").
    public static void Compose(
        this BaseComposer composer, 
        [InterpolatedStringHandlerArgument("composer")] Html html)
    {
    }
}