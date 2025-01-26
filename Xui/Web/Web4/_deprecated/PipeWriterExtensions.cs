using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Web4;
using Web4.Composers;

namespace Web4.HttpX;

public static class PipeWriterExtensions
{





    public static void Write(this IBufferWriter<byte> writer, ref Keyhole keyhole)
    {
        // keyhole.Write(writer);
    }

    public static void Write(this PipeWriter writer, IEnumerable<Memory<Keyhole>> deltas)
    {
        foreach (var delta in deltas)
        {
        //     ref var keyhole = ref delta.Span[0];

        //     if (delta.Length == 1)
        //     {
        //         writer.WriteStringLiteral("slot");
        //         writer.Write(keyhole.Id);
        //         writer.WriteStringLiteral(".nodeValue='");
        //         writer.Write(ref keyhole);
        //         writer.WriteStringLiteral("';");
        //     }
        //     else
        //     {
        //         writer.WriteStringLiteral("replaceNode(slot");
        //         writer.Write(keyhole.Id);
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
            //         writer.Write(keyhole.Id);
            //         writer.WriteStringLiteral(".nodeValue=`");
            //         writer.Write(ref keyhole);
            //         writer.WriteStringLiteral("`;");
            //         break;
            //     case DeltaType.Attribute:
            //         writer.WriteStringLiteral("slot");
            //         writer.Write(keyhole.Id);
            //         writer.WriteStringLiteral(".setAttribute(`");
            //         writer.Write(keyhole.Attribute);
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
            //         writer.Write(keyhole.Id);
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

    public static void Write(this PipeWriter writer, Span<Keyhole> span)
    {
        // bool hackProbablyAnAttributeNext = false;

        // for (int i = 0; i < span.Length; i++)
        // {
        //     ref var keyhole = ref span[i];

        //     switch (keyhole.Type)
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
        //                 writer.Write(ref keyhole);
        //             }
        //             else
        //             {
        //                 writer.WriteStringLiteral("<!-- -->");
        //                 writer.Write(ref keyhole);
        //                 writer.WriteStringLiteral("<script>r(\"slot");
        //                 writer.Write(keyhole.Id);
        //                 writer.WriteStringLiteral("\")</script>");
        //             }
        //             break;
        //         case FormatType.View:
        //         case FormatType.HtmlString:
        //             // Only render extras for HtmlString's trailing sentinel, ignore for the leading sentinel.
        //             if (keyhole.Id > keyhole.Integer)
        //             {
        //                 writer.WriteStringLiteral("<script>r(\"slot");
        //                 writer.Write(keyhole.Id);
        //                 writer.WriteStringLiteral("\")</script>");
        //             }
        //             break;
        //         case FormatType.Action:
        //         case FormatType.ActionAsync:
        //             writer.WriteStringLiteral("h(");
        //             writer.Write(keyhole.Id);
        //             writer.WriteStringLiteral(")");
        //             break;
        //         case FormatType.ActionEvent:
        //         case FormatType.ActionEventAsync:
        //             writer.WriteStringLiteral("h(");
        //             writer.Write(keyhole.Id);
        //             writer.WriteStringLiteral(",event)");
        //             break;
        //         default:
        //             writer.Write(ref keyhole);
        //             break;
        //     }

        //     if (keyhole.Type == FormatType.StringLiteral && keyhole.String?[^1] == '"')
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