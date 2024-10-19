using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Xui.Web;

namespace Xui.Web.HttpX;

public static class PipeWriterExtensions
{
    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer, 
        [InterpolatedStringHandlerArgument("writer")] ref Html html,
        CancellationToken cancellationToken = default)
    {
        // When instantiating an Html object, the compiler generates 
        // lowered code (i.e. AppendLiteral and AppendFormatted) which
        // is where the bytes get written to the PipeWriter.

        return writer.FlushAsync(cancellationToken);
    }





    public static void Write(this IBufferWriter<byte> writer, ref Chunk chunk)
    {
        // chunk.Write(writer);
    }

    public static void Write(this PipeWriter writer, IEnumerable<Memory<Chunk>> deltas)
    {
        foreach (var delta in deltas)
        {
        //     ref var chunk = ref delta.Span[0];

        //     if (delta.Length == 1)
        //     {
        //         writer.WriteStringLiteral("slot");
        //         writer.Write(chunk.Id);
        //         writer.WriteStringLiteral(".nodeValue='");
        //         writer.Write(ref chunk);
        //         writer.WriteStringLiteral("';");
        //     }
        //     else
        //     {
        //         writer.WriteStringLiteral("replaceNode(slot");
        //         writer.Write(chunk.Id);
        //         writer.WriteStringLiteral(",`");
        //         var span = delta.Span;
        //         for (int i = 0; i <= delta.Length; i++)
        //         {
        //             var c = span[i];
        //             writer.Write(ref c);
        //         }
        //         writer.WriteStringLiteral("`);");
        //     }






            // switch (delta.Type)
            // {
            //     case DeltaType.Text:
            //         writer.WriteStringLiteral("slot");
            //         writer.Write(chunk.Id);
            //         writer.WriteStringLiteral(".nodeValue=`");
            //         writer.Write(ref chunk);
            //         writer.WriteStringLiteral("`;");
            //         break;
            //     case DeltaType.Attribute:
            //         writer.WriteStringLiteral("slot");
            //         writer.Write(chunk.Id);
            //         writer.WriteStringLiteral(".setAttribute(`");
            //         writer.Write(chunk.Attribute);
            //         writer.WriteStringLiteral("`,`");
            //         var spanAttr = delta.Span;
            //         for (int i = 0; i <= delta.Length; i++)
            //         {
            //             var c = spanAttr[i];
            //             writer.Write(ref c);
            //         }
            //         writer.WriteStringLiteral("`);");
            //         break;
            //     case DeltaType.Element:
            //         // TODO: This is incomplete.  The new partial 
            //         // should include the sentinels like below.
            //         // Will it be escaped properly?
            //         writer.WriteStringLiteral("replaceNode(slot");
            //         writer.Write(chunk.Id);
            //         writer.WriteStringLiteral(",`");
            //         var spanElem = delta.Span;
            //         for (int i = 0; i <= delta.Length; i++)
            //         {
            //             var c = spanElem[i];
            //             writer.Write(ref c);
            //         }
            //         writer.WriteStringLiteral("`);");
            //         break;
            // }
        }
    }

    public static void Write(this PipeWriter writer, Span<Chunk> span)
    {
        // bool hackProbablyAnAttributeNext = false;

        // for (int i = 0; i < span.Length; i++)
        // {
        //     ref var chunk = ref span[i];

        //     switch (chunk.Type)
        //     {
        //         case FormatType.Boolean:
        //         case FormatType.DateTime:
        //         case FormatType.TimeSpan:
        //         case FormatType.Integer:
        //         case FormatType.Long:
        //         case FormatType.Float:
        //         case FormatType.Double:
        //         case FormatType.Decimal:
        //         case FormatType.String:
        //             if (hackProbablyAnAttributeNext)
        //             {
        //                 writer.Write(ref chunk);
        //             }
        //             else
        //             {
        //                 writer.WriteStringLiteral("<!-- -->");
        //                 writer.Write(ref chunk);
        //                 writer.WriteStringLiteral("<script>r(\"slot");
        //                 writer.Write(chunk.Id);
        //                 writer.WriteStringLiteral("\")</script>");
        //             }
        //             break;
        //         case FormatType.View:
        //         case FormatType.HtmlString:
        //             // Only render extras for HtmlString's trailing sentinel, ignore for the leading sentinel.
        //             if (chunk.Id > chunk.Integer)
        //             {
        //                 writer.WriteStringLiteral("<script>r(\"slot");
        //                 writer.Write(chunk.Id);
        //                 writer.WriteStringLiteral("\")</script>");
        //             }
        //             break;
        //         case FormatType.Action:
        //         case FormatType.ActionAsync:
        //             writer.WriteStringLiteral("h(");
        //             writer.Write(chunk.Id);
        //             writer.WriteStringLiteral(")");
        //             break;
        //         case FormatType.ActionEvent:
        //         case FormatType.ActionEventAsync:
        //             writer.WriteStringLiteral("h(");
        //             writer.Write(chunk.Id);
        //             writer.WriteStringLiteral(",event)");
        //             break;
        //         default:
        //             writer.Write(ref chunk);
        //             break;
        //     }

        //     if (chunk.Type == FormatType.StringLiteral && chunk.String?[^1] == '"')
        //     {
        //         hackProbablyAnAttributeNext = true;
        //     }
        //     else
        //     {
        //         hackProbablyAnAttributeNext = false;
        //     }
        // }
    }
}