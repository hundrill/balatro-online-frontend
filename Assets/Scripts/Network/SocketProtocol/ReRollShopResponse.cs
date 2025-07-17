using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class ReRollShopResponse : BaseSocket
{
    public override string EventName => EventNameConst;
    public const string EventNameConst = "ReRollShopResponse";

    public List<Card> cards;

    [Serializable]
    public class Card
    {
        public string id;
        public string name;
        public string description;
        public int price;
        public int sprite;
        public string type;
    }

    public static ReRollShopResponse FromPayload(object payload)
    {
        var response = new ReRollShopResponse();

        Debug.Log($"[ReRollShopResponse] FromPayload 호출 - payload 타입: {payload?.GetType()}");

        if (payload is Dictionary<string, object> payloadDict)
        {
            Debug.Log($"[ReRollShopResponse] payloadDict 키들: {string.Join(", ", payloadDict.Keys)}");

            // BaseSocket 필드들 설정
            if (payloadDict.TryGetValue("success", out var successObj))
                response.success = Convert.ToBoolean(successObj);
            if (payloadDict.TryGetValue("message", out var messageObj))
                response.message = messageObj?.ToString() ?? "";

            // cards 필드 처리
            if (payloadDict.TryGetValue("cards", out var cardsObj))
            {
                Debug.Log($"[ReRollShopResponse] cardsObj 타입: {cardsObj?.GetType()}, 값: {cardsObj}");

                List<object> cardsList = null;
                if (cardsObj is List<object> list)
                {
                    cardsList = list;
                }
                else if (cardsObj is object[] array)
                {
                    cardsList = new List<object>(array);
                }

                if (cardsList != null)
                {
                    Debug.Log($"[ReRollShopResponse] cardsList.Count: {cardsList.Count}");
                    response.cards = new List<Card>();
                    foreach (var cardObj in cardsList)
                    {
                        Debug.Log($"[ReRollShopResponse] cardObj 타입: {cardObj?.GetType()}");
                        if (cardObj is Dictionary<string, object> cardDict)
                        {
                            Debug.Log($"[ReRollShopResponse] cardDict 키들: {string.Join(", ", cardDict.Keys)}");
                            var card = new Card
                            {
                                id = cardDict.TryGetValue("id", out var id) ? id?.ToString() : "",
                                name = cardDict.TryGetValue("name", out var name) ? name?.ToString() : "",
                                description = cardDict.TryGetValue("description", out var description) ? description?.ToString() : "",
                                price = cardDict.TryGetValue("price", out var price) ? Convert.ToInt32(price) : 0,
                                sprite = cardDict.TryGetValue("sprite", out var sprite) ? Convert.ToInt32(sprite) : 0,
                                type = cardDict.TryGetValue("type", out var type) ? type?.ToString() : ""
                            };
                            response.cards.Add(card);
                            Debug.Log($"[ReRollShopResponse] 카드 추가됨: {card.id} - {card.name}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[ReRollShopResponse] cardsObj를 List<object>로 변환할 수 없음: {cardsObj?.GetType()}");
                }
            }
            else
            {
                Debug.LogWarning("[ReRollShopResponse] payloadDict에 'cards' 키가 없음");
            }
        }
        else
        {
            Debug.LogWarning($"[ReRollShopResponse] payload가 Dictionary<string, object>가 아님: {payload?.GetType()}");
        }

        Debug.Log($"[ReRollShopResponse] 최종 cards.Count: {response.cards?.Count ?? 0}");
        return response;
    }
}