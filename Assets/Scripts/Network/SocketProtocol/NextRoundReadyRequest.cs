using System;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class NextRoundReadyRequest : BaseSocket
{
    public override string EventName => "NextRoundReadyRequest";
    public string roomId;

    public NextRoundReadyRequest(string roomId)
    {
        this.roomId = roomId;
    }
}