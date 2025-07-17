using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class SellCardResponse : BaseSocket
{
    public override string EventName => EventNameConst;
    public const string EventNameConst = "SellCardResponse";

    public string soldCardName;

    public static SellCardResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new SellCardResponse();
                
                // BaseSocket 필드들 설정
                response.SetBaseFields(dict);
                
                // SellCardResponse 필드들 설정
                if (dict.TryGetValue("soldCardName", out var soldCardNameObj))
                    response.soldCardName = soldCardNameObj?.ToString() ?? "";

                Debug.Log($"[SellCardResponse] FromPayload 성공 - soldCardName: {response.soldCardName}");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SellCardResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}