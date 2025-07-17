using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using BalatroOnline.Game;
using UnityEngine;

[Serializable]
public class StartGameResponse : BaseSocket
{
    [Serializable]
    public class Card
    {
        public CardType suit;
        public int rank;
    }

    [Serializable]
    public class Opponent
    {
        public string userId;
        public string nickname;
        public int silverChip;
        public int goldChip;
    }

    public override string EventName => EventNameConst;
    public const string EventNameConst = "StartGameResponse";

    [SerializeField] private List<Card> _myCards;
    [SerializeField] private List<Opponent> _opponents;
    [SerializeField] private int _round;
    [SerializeField] private int _silverSeedChip;
    [SerializeField] private int _goldSeedChip;
    [SerializeField] private Dictionary<string, int> _userFunds;

    // 프로퍼티로 접근
    public List<Card> myCards => _myCards;
    public List<Opponent> opponents => _opponents;
    public int round => _round;
    public int silverSeedChip => _silverSeedChip;
    public int goldSeedChip => _goldSeedChip;
    public Dictionary<string, int> userFunds => _userFunds;

    public static StartGameResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new StartGameResponse();

                // success와 message는 BaseSocket에서 처리
                if (dict.TryGetValue("success", out var successObj))
                    response.success = Convert.ToBoolean(successObj);
                if (dict.TryGetValue("message", out var messageObj))
                    response.message = messageObj.ToString();

                // round 파싱
                if (dict.TryGetValue("round", out var roundObj))
                    response._round = Convert.ToInt32(roundObj);

                // silverSeedChip 파싱
                if (dict.TryGetValue("silverSeedChip", out var silverObj))
                    response._silverSeedChip = Convert.ToInt32(silverObj);

                // goldSeedChip 파싱
                if (dict.TryGetValue("goldSeedChip", out var goldObj))
                    response._goldSeedChip = Convert.ToInt32(goldObj);

                // myCards 파싱
                if (dict.TryGetValue("myCards", out var myCardsObj))
                {
                    response._myCards = new List<Card>();

                    if (myCardsObj is List<object> myCardsList)
                    {
                        foreach (var cardObj in myCardsList)
                        {
                            if (cardObj is Dictionary<string, object> cardDict)
                            {
                                var card = new Card
                                {
                                    suit = cardDict.TryGetValue("suit", out var suitObj) ? CardData.StringToCardType(suitObj.ToString()) : CardType.Clubs,
                                    rank = cardDict.TryGetValue("rank", out var rankObj) ? Convert.ToInt32(rankObj) : 0
                                };
                                response._myCards.Add(card);
                            }
                        }
                    }
                    else if (myCardsObj is object[] myCardsArray)
                    {
                        foreach (var cardObj in myCardsArray)
                        {
                            if (cardObj is Dictionary<string, object> cardDict)
                            {
                                var card = new Card
                                {
                                    suit = cardDict.TryGetValue("suit", out var suitObj) ? CardData.StringToCardType(suitObj.ToString()) : CardType.Clubs,
                                    rank = cardDict.TryGetValue("rank", out var rankObj) ? Convert.ToInt32(rankObj) : 0
                                };
                                response._myCards.Add(card);
                            }
                        }
                    }
                }

                // opponents 파싱
                if (dict.TryGetValue("opponents", out var opponentsObj))
                {
                    response._opponents = new List<Opponent>();

                    if (opponentsObj is List<object> opponentsList)
                    {
                        foreach (var opponentObj in opponentsList)
                        {
                            if (opponentObj is Dictionary<string, object> opponentDict)
                            {
                                var opponent = new Opponent
                                {
                                    userId = opponentDict.TryGetValue("userId", out var uidObj) ? uidObj.ToString() : "",
                                    nickname = opponentDict.TryGetValue("nickname", out var nickObj) ? nickObj.ToString() : "",
                                    silverChip = opponentDict.TryGetValue("silverChip", out var silverChipObj) ? Convert.ToInt32(silverChipObj) : 0,
                                    goldChip = opponentDict.TryGetValue("goldChip", out var goldChipObj) ? Convert.ToInt32(goldChipObj) : 0
                                };
                                response._opponents.Add(opponent);
                            }
                        }
                    }
                    else if (opponentsObj is object[] opponentsArray)
                    {
                        foreach (var opponentObj in opponentsArray)
                        {
                            if (opponentObj is Dictionary<string, object> opponentDict)
                            {
                                var opponent = new Opponent
                                {
                                    userId = opponentDict.TryGetValue("userId", out var uidObj) ? uidObj.ToString() : "",
                                    nickname = opponentDict.TryGetValue("nickname", out var nickObj) ? nickObj.ToString() : "",
                                    silverChip = opponentDict.TryGetValue("silverChip", out var silverChipObj) ? Convert.ToInt32(silverChipObj) : 0,
                                    goldChip = opponentDict.TryGetValue("goldChip", out var goldChipObj) ? Convert.ToInt32(goldChipObj) : 0
                                };
                                response._opponents.Add(opponent);
                            }
                        }
                    }
                }

                // userFunds 파싱
                if (dict.TryGetValue("userFunds", out var userFundsObj))
                {
                    response._userFunds = new Dictionary<string, int>();

                    if (userFundsObj is Dictionary<string, object> userFundsDict)
                    {
                        foreach (var kvp in userFundsDict)
                        {
                            string userId = kvp.Key;
                            int funds = Convert.ToInt32(kvp.Value);
                            response._userFunds[userId] = funds;
                        }
                    }
                }

                Debug.Log($"[StartGameResponse] FromPayload 성공 - round: {response._round}, myCards count: {response._myCards?.Count ?? 0}, opponents count: {response._opponents?.Count ?? 0}");
                if (response._myCards != null)
                {
                    foreach (var card in response._myCards)
                    {
                        Debug.Log($"[StartGameResponse] Card: {card.suit} {card.rank}");
                    }
                }
                if (response._userFunds != null)
                {
                    foreach (var kvp in response._userFunds)
                    {
                        Debug.Log($"[StartGameResponse] UserFunds: {kvp.Key} = {kvp.Value}");
                    }
                }
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StartGameResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}