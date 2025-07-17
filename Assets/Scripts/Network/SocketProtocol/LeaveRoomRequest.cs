using System;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class LeaveRoomRequest : BaseSocket
{
    public override string EventName => "LeaveRoomRequest";

    public string roomId;

    public LeaveRoomRequest(string roomId)
    {
        this.roomId = roomId;
    }
}