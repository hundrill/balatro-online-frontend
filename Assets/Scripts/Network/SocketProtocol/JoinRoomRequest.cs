using System;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class JoinRoomRequest : BaseSocket
{
    public override string EventName => "JoinRoomRequest";

    public string roomId;
    public string userId;

    public JoinRoomRequest(string roomId, string userId)
    {
        this.roomId = roomId;
        this.userId = userId;
    }
}