﻿using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using dotnetCampus.Ipc.Context;
using dotnetCampus.Ipc.Messages;
using dotnetCampus.Ipc.Pipes;
using Newtonsoft.Json;

namespace dotnetCampus.Ipc.IpcRouteds.DirectRouteds;

public class JsonIpcDirectRoutedClientProxy : IpcDirectRoutedClientProxyBase
{
    public JsonIpcDirectRoutedClientProxy(PeerProxy peerProxy)
    {
        _peerProxy = peerProxy;
        IpcContext = peerProxy.IpcContext;
    }

    private readonly PeerProxy _peerProxy;
    private IpcContext? IpcContext { get; }
    private JsonSerializer? _jsonSerializer;
    private JsonSerializer JsonSerializer => _jsonSerializer ??= JsonSerializer.CreateDefault();

    /// <summary>
    /// 不带参数的通知服务端
    /// </summary>
    /// <param name="routedPath"></param>
    /// <returns></returns>
    public Task NotifyAsync(string routedPath)
        => NotifyAsync(routedPath, JsonIpcDirectRoutedParameterlessType.Instance);

    public Task NotifyAsync<T>(string routedPath, T obj) where T : class
    {
        IpcMessage ipcMessage = BuildMessage(routedPath, obj);
        return _peerProxy.NotifyAsync(ipcMessage);
    }

    public Task<TResponse?> GetResponseAsync<TResponse>(string routedPath) where TResponse : class
        => GetResponseAsync<TResponse>(routedPath, JsonIpcDirectRoutedParameterlessType.Instance);

    public async Task<TResponse?> GetResponseAsync<TResponse>(string routedPath, object obj) where TResponse : class
    {
        IpcMessage ipcMessage = BuildMessage(routedPath, obj);
        var responseMessage = await _peerProxy.GetResponseAsync(ipcMessage);

        using var memoryStream = new MemoryStream(responseMessage.Body.Buffer, responseMessage.Body.Start, responseMessage.Body.Length, writable: false);
        using StreamReader reader = new StreamReader
        (
            memoryStream,
#if !NETCOREAPP
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 2048,
#endif
            leaveOpen: true
        );
        JsonReader jsonReader = new JsonTextReader(reader);
        return JsonSerializer.Deserialize<TResponse>(jsonReader);
    }

    private IpcMessage BuildMessage(string routedPath, object obj)
    {
        using var memoryStream = new MemoryStream();
        WriteHeader(memoryStream, routedPath);

        using (var textWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true, bufferSize: 2048))
        {
            JsonSerializer.Serialize(textWriter, obj);
        }

        return ToIpcMessage(memoryStream, $"Message {routedPath}");
    }

    protected override ulong BusinessHeader => (ulong) KnownMessageHeaders.JsonIpcDirectRoutedMessageHeader;
}
