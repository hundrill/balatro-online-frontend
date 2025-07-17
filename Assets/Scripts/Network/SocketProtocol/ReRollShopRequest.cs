using System;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class ReRollShopRequest : BaseSocket
{
    public override string EventName => "ReRollShopRequest";
    public string roomId;

    public ReRollShopRequest(string roomId)
    {
        this.roomId = roomId;
    }
}