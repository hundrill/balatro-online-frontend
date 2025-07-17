using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class CardPurchasedResponse : BaseSocket
{
    public override string EventName => EventNameConst;
    public const string EventNameConst = "CardPurchasedResponse";

    public string userId;
    public string cardId;
    public string cardType;
    public int price;
    public string cardName;
    public string cardDescription;
    public int cardSprite;

    public static CardPurchasedResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new CardPurchasedResponse();
                
                // BaseSocket 필드들 설정
                response.SetBaseFields(dict);
                
                // CardPurchasedResponse 필드들 설정
                if (dict.TryGetValue("userId", out var userIdObj))
                    response.userId = userIdObj?.ToString() ?? "";
                if (dict.TryGetValue("cardId", out var cardIdObj))
                    response.cardId = cardIdObj?.ToString() ?? "";
                if (dict.TryGetValue("cardType", out var cardTypeObj))
                    response.cardType = cardTypeObj?.ToString() ?? "";
                if (dict.TryGetValue("price", out var priceObj))
                    response.price = Convert.ToInt32(priceObj);
                if (dict.TryGetValue("cardName", out var cardNameObj))
                    response.cardName = cardNameObj?.ToString() ?? "";
                if (dict.TryGetValue("cardDescription", out var cardDescriptionObj))
                    response.cardDescription = cardDescriptionObj?.ToString() ?? "";
                if (dict.TryGetValue("cardSprite", out var cardSpriteObj))
                    response.cardSprite = Convert.ToInt32(cardSpriteObj);

                Debug.Log($"[CardPurchasedResponse] FromPayload 성공 - userId: {response.userId}, cardName: {response.cardName}");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CardPurchasedResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}