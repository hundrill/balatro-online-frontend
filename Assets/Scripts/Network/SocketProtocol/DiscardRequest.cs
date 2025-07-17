using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class DiscardRequest : BaseSocket
{
    public override string EventName => "DiscardRequest";
    public string roomId;
    public List<CardDto> cards;

    public DiscardRequest(string roomId, List<CardDto> cards)
    {
        this.roomId = roomId;
        this.cards = cards;
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