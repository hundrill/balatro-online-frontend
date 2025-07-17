using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class HandPlayReadyRequest : BaseSocket
{
    public override string EventName => "HandPlayReadyRequest";
    public string roomId;
    public List<CardDto> hand;

    public HandPlayReadyRequest(string roomId, List<CardDto> hand)
    {
        this.roomId = roomId;
        this.hand = hand;
    }

    [Serializable]
    public struct CardDto
    {
        public string suit;
        public int rank;

        public CardDto(string suit, int rank)
        {
            this.suit = suit;
            this.rank = rank;
        }
    }
}