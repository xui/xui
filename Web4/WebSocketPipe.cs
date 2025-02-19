using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Web4;

public class WebSocketPipe : IDuplexPipe, IDisposable
{
    private static readonly ConcurrentDictionary<string, WebSocketPipe> pipeMap = [];
    const int receiveBufferSize = 1024;
    private readonly WebSocket webSocket;
    readonly Pipe inputPipe = new();
    readonly PipeWriter pipeWriter;
    public PipeReader Input => inputPipe.Reader;
    public PipeWriter Output => pipeWriter;
    public WebSocketState State => webSocket.State;
    public string Key { get; init; }

    public static async Task<WebSocketPipe> Upgrade(HttpContext httpContext)
    {
        // TODO: Switch to header.
        var key = httpContext.Connection.Id;

        var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
        var pipe = new WebSocketPipe(key, webSocket);

        pipeMap[key] = pipe;

        return pipe;
    }

    public static WebSocketPipe? Get(string key) => pipeMap.GetValueOrDefault(key);

    private WebSocketPipe(string key, WebSocket webSocket)
    {
        this.Key = key;
        this.webSocket = webSocket;
        pipeWriter = PipeWriter.Create(new WebSocketStream(webSocket));
    }

    public async Task RunAsync(CancellationToken cancellation = default)
    {
        while (webSocket.State == WebSocketState.Open && !cancellation.IsCancellationRequested)
        {
            try
            {
                while (true)
                {
                    var memory = inputPipe.Writer.GetMemory(receiveBufferSize);
                    var message = await webSocket.ReceiveAsync(memory, cancellation);
                    inputPipe.Writer.Advance(message.Count);

                    if (message.Count == 0 ||
                        message.EndOfMessage ||
                        cancellation.IsCancellationRequested ||
                        message.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }

                var result = await inputPipe.Writer.FlushAsync(cancellation);
                if (result.IsCompleted)
                    break;
            } catch (Exception ex) when (
                ex is OperationCanceledException ||
                ex is WebSocketException ||
                ex is InvalidOperationException
            ) {
                break;
            }
        }

        await CompleteAsync(webSocket.CloseStatus, webSocket.CloseStatusDescription);
    }

    public async Task CompleteAsync(WebSocketCloseStatus? closeStatus = null, string? closeStatusDescription = null)
    {
        await inputPipe.Writer.CompleteAsync();
        await inputPipe.Reader.CompleteAsync();
        await CloseAsync(closeStatus ?? WebSocketCloseStatus.NormalClosure, closeStatusDescription ?? "");
    }

    async Task CloseAsync(WebSocketCloseStatus closeStatus, string closeStatusDescription)
    {
        var state = webSocket.State;
        if (state == WebSocketState.Closed || state == WebSocketState.CloseSent || state == WebSocketState.Aborted)
            return;

        var closeTask = webSocket is ClientWebSocket ?
            webSocket.CloseAsync(closeStatus, closeStatusDescription, default) :
            webSocket.CloseOutputAsync(closeStatus, closeStatusDescription, default);

        await Task.WhenAny(closeTask, Task.Delay(TimeSpan.FromMilliseconds(250)));
    }

    public void Dispose()
    {
        webSocket.Dispose();
        pipeMap.Remove(Key, out WebSocketPipe? value);
    }
}

class WebSocketStream(WebSocket webSocket) : Stream
{
    public override bool CanRead => throw new NotImplementedException();
    public override bool CanSeek => throw new NotImplementedException();
    public override bool CanWrite => throw new NotImplementedException();
    public override long Length => throw new NotImplementedException();
    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override void Flush() => throw new NotImplementedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
    public override void SetLength(long value) => throw new NotImplementedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        => webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);

    public override async Task FlushAsync(CancellationToken cancellationToken)
        => await webSocket.SendAsync(
            new ReadOnlyMemory<byte>([]), // TODO: Find out why it's not flushing.
            WebSocketMessageType.Text,
            true,
            cancellationToken
        );
}