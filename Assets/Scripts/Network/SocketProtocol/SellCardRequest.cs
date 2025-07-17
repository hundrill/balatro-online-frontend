using System;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class SellCardRequest : BaseSocket
{
    public override string EventName => "SellCardRequest";
    public string roomId;
    public string cardId;
    public string cardType;
    public int price;

    public SellCardRequest(string roomId, string cardId, string cardType, int price)
    {
        this.roomId = roomId;
        this.cardId = cardId;
        this.cardType = cardType;
        this.price = price;
    }
}