using System.Buffers;
using System.Text.Json;

namespace Web4.WebSockets;

record struct JsonRpc(string Method, int? ID)
{
    public static JsonRpc Error = new("error", null);

    public static JsonRpc Parse(ReadOnlySequence<byte> sequence)
    {
        using var perf = Debug.PerfCheck("JsonRpc.Parse"); // TODO: Remove PerfCheck

        string? method = null;
        int? id = null;
        try
        {
            var reader = new Utf8JsonReader(sequence);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals("method"))
                    {
                        reader.Read();
                        ReadOnlySpan<byte> value = reader.HasValueSequence
                            ? reader.ValueSequence.ToArray() // TODO: Allocates, but is rare?  Confirm.
                            : reader.ValueSpan;
                        method = Keymaker.GetKeyIfCached(value);
                    }
                    else if (reader.ValueTextEquals("params"))
                    {
                        reader.Skip();
                    }
                    else if (reader.ValueTextEquals("id"))
                    {
                        reader.Read();
                        if (reader.TryGetInt32(out int i))
                            id = i;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return method is not null ? new(method, id) : Error;
    }

    public T GetNextPositionalParam<T>()
    {
        return default(T);
    }
}
