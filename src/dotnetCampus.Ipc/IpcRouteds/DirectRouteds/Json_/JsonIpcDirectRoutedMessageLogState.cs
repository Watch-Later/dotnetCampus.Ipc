﻿using System;
using System.IO;

namespace dotnetCampus.Ipc.IpcRouteds.DirectRouteds;

public readonly struct JsonIpcDirectRoutedMessageLogState
{
    public JsonIpcDirectRoutedMessageLogState(string routedPath, string localPeerName, string remotePeerName, JsonIpcDirectRoutedLogStateMessageType messageType,
        MemoryStream stream)
    {
        RoutedPath = routedPath;
        LocalPeerName = localPeerName;
        RemotePeerName = remotePeerName;
        MessageType = messageType;
        Stream = stream;
    }

    public string RoutedPath { get; }
    public string LocalPeerName { get; }
    public string RemotePeerName { get; }
    public JsonIpcDirectRoutedLogStateMessageType MessageType { get; }
    private MemoryStream Stream { get; }

    public string GetJsonText()
    {
        var position = Stream.Position;
        var streamReader = new StreamReader(Stream);
        var jsonText = streamReader.ReadToEnd();
        Stream.Position = position;
        return jsonText;
    }

    /// <summary>
    /// 格式化
    /// </summary>
    /// <param name="state"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static string Format(JsonIpcDirectRoutedMessageLogState state, Exception? exception)
    {
        var action = state.MessageType switch
        {
            JsonIpcDirectRoutedLogStateMessageType.Notify => "Notify",
            JsonIpcDirectRoutedLogStateMessageType.Request => "Request",
            _ => string.Empty
        };

        return $"[JsonIpcDirectRouted][{action}] {state.RoutedPath} from {state.RemotePeerName} To {state.LocalPeerName} : {state.GetJsonText()}";
    }
}
