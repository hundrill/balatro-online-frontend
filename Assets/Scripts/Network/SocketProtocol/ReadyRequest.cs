using System;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class ReadyRequest : BaseSocket
{
    public override string EventName => "ReadyRequest";
    public string roomId;

    public ReadyRequest(string roomId)
    {
        this.roomId = roomId;
    }
}