using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using BalatroOnline.Game;
using UnityEngine;

[Serializable]
public class DiscardResponse : BaseSocket
{
    [Serializable]
    public class Card
    {
        public CardType suit;
        public int rank;
    }

    public override string EventName => EventNameConst;
    public const string EventNameConst = "DiscardResponse";

    [SerializeField] private List<Card> _newHand;
    [SerializeField] private List<Card> _discarded;
    [SerializeField] private int _remainingDiscards;

    // 프로퍼티로 접근
    public List<Card> newHand => _newHand;
    public List<Card> discarded => _discarded;
    public int remainingDiscards => _remainingDiscards;

    public static DiscardResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new DiscardResponse();

                // success와 message는 BaseSocket에서 처리
                if (dict.TryGetValue("success", out var successObj))
                    response.success = Convert.ToBoolean(successObj);
                if (dict.TryGetValue("message", out var messageObj))
                    response.message = messageObj.ToString();

                // remainingDiscards 파싱
                if (dict.TryGetValue("remainingDiscards", out var remainingObj))
                    response._remainingDiscards = Convert.ToInt32(remainingObj);

                // newHand 파싱
                if (dict.TryGetValue("newHand", out var newHandObj))
                {
                    response._newHand = new List<Card>();

                    if (newHandObj is List<object> newHandList)
                    {
                        foreach (var cardObj in newHandList)
                        {
                            if (cardObj is Dictionary<string, object> cardDict)
                            {
                                var card = new Card
                                {
                                    suit = cardDict.TryGetValue("suit", out var suitObj) ? CardData.StringToCardType(suitObj.ToString()) : CardType.Clubs,
                                    rank = cardDict.TryGetValue("rank", out var rankObj) ? Convert.ToInt32(rankObj) : 0
                                };
                                response._newHand.Add(card);
                            }
                        }
                    }
                    else if (newHandObj is object[] newHandArray)
                    {
                        foreach (var cardObj in newHandArray)
                        {
                            if (cardObj is Dictionary<string, object> cardDict)
                            {
                                var card = new Card
                                {
                                    suit = cardDict.TryGetValue("suit", out var suitObj) ? CardData.StringToCardType(suitObj.ToString()) : CardType.Clubs,
                                    rank = cardDict.TryGetValue("rank", out var rankObj) ? Convert.ToInt32(rankObj) : 0
                                };
                                response._newHand.Add(card);
                            }
                        }
                    }
                }

                // discarded 파싱
                if (dict.TryGetValue("discarded", out var discardedObj))
                {
                    response._discarded = new List<Card>();

                    if (discardedObj is List<object> discardedList)
                    {
                        foreach (var cardObj in discardedList)
                        {
                            if (cardObj is Dictionary<string, object> cardDict)
                            {
                                var card = new Card
                                {
                                    suit = cardDict.TryGetValue("suit", out var suitObj) ? CardData.StringToCardType(suitObj.ToString()) : CardType.Clubs,
                                    rank = cardDict.TryGetValue("rank", out var rankObj) ? Convert.ToInt32(rankObj) : 0
                                };
                                response._discarded.Add(card);
                            }
                        }
                    }
                    else if (discardedObj is object[] discardedArray)
                    {
                        foreach (var cardObj in discardedArray)
                        {
                            if (cardObj is Dictionary<string, object> cardDict)
                            {
                                var card = new Card
                                {
                                    suit = cardDict.TryGetValue("suit", out var suitObj) ? CardData.StringToCardType(suitObj.ToString()) : CardType.Clubs,
                                    rank = cardDict.TryGetValue("rank", out var rankObj) ? Convert.ToInt32(rankObj) : 0
                                };
                                response._discarded.Add(card);
                            }
                        }
                    }
                }

                Debug.Log($"[DiscardResponse] FromPayload 성공 - newHand count: {response._newHand?.Count ?? 0}, discarded count: {response._discarded?.Count ?? 0}, remainingDiscards: {response._remainingDiscards}");
                if (response._newHand != null)
                {
                    foreach (var card in response._newHand)
                    {
                        Debug.Log($"[DiscardResponse] NewHand Card: {card.suit} {card.rank}");
                    }
                }
                if (response._discarded != null)
                {
                    foreach (var card in response._discarded)
                    {
                        Debug.Log($"[DiscardResponse] Discarded Card: {card.suit} {card.rank}");
                    }
                }
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DiscardResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}