using System;
using System.Collections.Generic;
using BalatroOnline.Game;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class HandPlayResultResponse : BaseSocket
{
    [Serializable]
    public class Card
    {
        public CardType suit;
        public int rank;
    }

    [Serializable]
    public class RoundResult
    {
        public List<Card> hand;
        public int score;
        public int silverChipGain;
        public int goldChipGain;
        public int finalSilverChips;
        public int finalGoldChips;
        public int finalFunds;
        // 추가 필드
        public int remainingDiscards;
        public int remainingDeck;
        public int remainingSevens;
    }

    [Serializable]
    public class ShopCard
    {
        public string id;
        public string name;
        public string description;
        public int price;
        public int sprite;
        public int basevalue;
        public float increase;
        public float decrease;
        public string timing_after_scoring;
        public string timing_hand_play;
        public int maxvalue;
        public string type;
    }

    public override string EventName => EventNameConst;
    public const string EventNameConst = "HandPlayResultResponse";

    [SerializeField] private Dictionary<string, RoundResult> _roundResult;
    [SerializeField] private List<ShopCard> _shopCards;
    [SerializeField] private Dictionary<string, List<object>> _ownedCards;
    [SerializeField] private int _round;

    // 프로퍼티로 접근
    public Dictionary<string, RoundResult> roundResult => _roundResult;
    public List<ShopCard> shopCards => _shopCards;
    public Dictionary<string, List<object>> ownedCards => _ownedCards;
    public int round => _round;

    public static HandPlayResultResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new HandPlayResultResponse();

                // success와 message는 BaseSocket에서 처리
                if (dict.TryGetValue("success", out var successObj))
                    response.success = Convert.ToBoolean(successObj);
                if (dict.TryGetValue("message", out var messageObj))
                    response.message = messageObj.ToString();

                // round 파싱
                if (dict.TryGetValue("round", out var roundObj))
                    response._round = Convert.ToInt32(roundObj);

                // roundResult 파싱
                if (dict.TryGetValue("roundResult", out var roundResultObj))
                {
                    response._roundResult = new Dictionary<string, RoundResult>();

                    if (roundResultObj is Dictionary<string, object> roundResultDict)
                    {
                        foreach (var kvp in roundResultDict)
                        {
                            string userId = kvp.Key;
                            if (kvp.Value is Dictionary<string, object> userResultDict)
                            {
                                var roundResult = new RoundResult();

                                // hand 파싱
                                if (userResultDict.TryGetValue("hand", out var handObj))
                                {
                                    roundResult.hand = new List<Card>();
                                    if (handObj is List<object> handList)
                                    {
                                        foreach (var cardObj in handList)
                                        {
                                            if (cardObj is Dictionary<string, object> cardDict)
                                            {
                                                var card = new Card
                                                {
                                                    suit = cardDict.TryGetValue("suit", out var suitObj) ? CardData.StringToCardType(suitObj.ToString()) : CardType.Clubs,
                                                    rank = cardDict.TryGetValue("rank", out var rankObj) ? Convert.ToInt32(rankObj) : 0
                                                };
                                                roundResult.hand.Add(card);
                                            }
                                        }
                                    }
                                    else if (handObj is object[] handArray)
                                    {
                                        foreach (var cardObj in handArray)
                                        {
                                            if (cardObj is Dictionary<string, object> cardDict)
                                            {
                                                var card = new Card
                                                {
                                                    suit = cardDict.TryGetValue("suit", out var suitObj) ? CardData.StringToCardType(suitObj.ToString()) : CardType.Clubs,
                                                    rank = cardDict.TryGetValue("rank", out var rankObj) ? Convert.ToInt32(rankObj) : 0
                                                };
                                                roundResult.hand.Add(card);
                                            }
                                        }
                                    }
                                }

                                // 기타 필드들 파싱
                                if (userResultDict.TryGetValue("score", out var scoreObj))
                                    roundResult.score = Convert.ToInt32(scoreObj);
                                if (userResultDict.TryGetValue("silverChipGain", out var silverGainObj))
                                    roundResult.silverChipGain = Convert.ToInt32(silverGainObj);
                                if (userResultDict.TryGetValue("goldChipGain", out var goldGainObj))
                                    roundResult.goldChipGain = Convert.ToInt32(goldGainObj);
                                if (userResultDict.TryGetValue("finalSilverChips", out var finalSilverObj))
                                    roundResult.finalSilverChips = Convert.ToInt32(finalSilverObj);
                                if (userResultDict.TryGetValue("finalGoldChips", out var finalGoldObj))
                                    roundResult.finalGoldChips = Convert.ToInt32(finalGoldObj);
                                if (userResultDict.TryGetValue("finalFunds", out var finalFundsObj))
                                    roundResult.finalFunds = Convert.ToInt32(finalFundsObj);
                                // 추가 필드 파싱
                                if (userResultDict.TryGetValue("remainingDiscards", out var remainingDiscardsObj))
                                    roundResult.remainingDiscards = Convert.ToInt32(remainingDiscardsObj);
                                if (userResultDict.TryGetValue("remainingDeck", out var remainingDeckObj))
                                    roundResult.remainingDeck = Convert.ToInt32(remainingDeckObj);
                                if (userResultDict.TryGetValue("remainingSevens", out var remainingSevensObj))
                                    roundResult.remainingSevens = Convert.ToInt32(remainingSevensObj);

                                response._roundResult[userId] = roundResult;
                            }
                        }
                    }
                }

                // shopCards 파싱
                if (dict.TryGetValue("shopCards", out var shopCardsObj))
                {
                    response._shopCards = new List<ShopCard>();

                    if (shopCardsObj is List<object> shopCardsList)
                    {
                        foreach (var cardObj in shopCardsList)
                        {
                            if (cardObj is Dictionary<string, object> cardDict)
                            {
                                var shopCard = new ShopCard
                                {
                                    id = cardDict.TryGetValue("id", out var idObj) ? idObj.ToString() : "",
                                    name = cardDict.TryGetValue("name", out var nameObj) ? nameObj.ToString() : "",
                                    description = cardDict.TryGetValue("description", out var descObj) ? descObj.ToString() : "",
                                    price = cardDict.TryGetValue("price", out var priceObj) ? Convert.ToInt32(priceObj) : 0,
                                    sprite = cardDict.TryGetValue("sprite", out var spriteObj) ? Convert.ToInt32(spriteObj) : 0,
                                    basevalue = cardDict.TryGetValue("basevalue", out var basevalueObj) ? Convert.ToInt32(basevalueObj) : 0,
                                    increase = cardDict.TryGetValue("increase", out var increaseObj) ? Convert.ToSingle(increaseObj) : 0f,
                                    decrease = cardDict.TryGetValue("decrease", out var decreaseObj) ? Convert.ToSingle(decreaseObj) : 0f,
                                    timing_after_scoring = cardDict.TryGetValue("timing_after_scoring", out var timingAfterObj) ? timingAfterObj.ToString() : "",
                                    timing_hand_play = cardDict.TryGetValue("timing_hand_play", out var timingHandObj) ? timingHandObj.ToString() : "",
                                    maxvalue = cardDict.TryGetValue("maxvalue", out var maxvalueObj) ? Convert.ToInt32(maxvalueObj) : 0,
                                    type = cardDict.TryGetValue("type", out var typeObj) ? typeObj.ToString() : ""
                                };
                                response._shopCards.Add(shopCard);
                            }
                        }
                    }
                    else if (shopCardsObj is object[] shopCardsArray)
                    {
                        foreach (var cardObj in shopCardsArray)
                        {
                            if (cardObj is Dictionary<string, object> cardDict)
                            {
                                var shopCard = new ShopCard
                                {
                                    id = cardDict.TryGetValue("id", out var idObj) ? idObj.ToString() : "",
                                    name = cardDict.TryGetValue("name", out var nameObj) ? nameObj.ToString() : "",
                                    description = cardDict.TryGetValue("description", out var descObj) ? descObj.ToString() : "",
                                    price = cardDict.TryGetValue("price", out var priceObj) ? Convert.ToInt32(priceObj) : 0,
                                    sprite = cardDict.TryGetValue("sprite", out var spriteObj) ? Convert.ToInt32(spriteObj) : 0,
                                    basevalue = cardDict.TryGetValue("basevalue", out var basevalueObj) ? Convert.ToInt32(basevalueObj) : 0,
                                    increase = cardDict.TryGetValue("increase", out var increaseObj) ? Convert.ToSingle(increaseObj) : 0f,
                                    decrease = cardDict.TryGetValue("decrease", out var decreaseObj) ? Convert.ToSingle(decreaseObj) : 0f,
                                    timing_after_scoring = cardDict.TryGetValue("timing_after_scoring", out var timingAfterObj) ? timingAfterObj.ToString() : "",
                                    timing_hand_play = cardDict.TryGetValue("timing_hand_play", out var timingHandObj) ? timingHandObj.ToString() : "",
                                    maxvalue = cardDict.TryGetValue("maxvalue", out var maxvalueObj) ? Convert.ToInt32(maxvalueObj) : 0,
                                    type = cardDict.TryGetValue("type", out var typeObj) ? typeObj.ToString() : ""
                                };
                                response._shopCards.Add(shopCard);
                            }
                        }
                    }
                }

                // ownedCards 파싱
                if (dict.TryGetValue("ownedCards", out var ownedCardsObj))
                {
                    response._ownedCards = new Dictionary<string, List<object>>();

                    if (ownedCardsObj is Dictionary<string, object> ownedCardsDict)
                    {
                        foreach (var kvp in ownedCardsDict)
                        {
                            string userId = kvp.Key;
                            if (kvp.Value is List<object> cardsList)
                            {
                                response._ownedCards[userId] = cardsList;
                            }
                            else if (kvp.Value is object[] cardsArray)
                            {
                                response._ownedCards[userId] = new List<object>(cardsArray);
                            }
                        }
                    }
                }

                Debug.Log($"[HandPlayResultResponse] FromPayload 성공 - round: {response._round}, roundResult count: {response._roundResult?.Count ?? 0}, shopCards count: {response._shopCards?.Count ?? 0}");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HandPlayResultResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}