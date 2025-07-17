using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class NextRoundReadyResponse : BaseSocket
{
    public override string EventName => EventNameConst;
    public const string EventNameConst = "NextRoundReadyResponse";

    public string userId;

    public static NextRoundReadyResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new NextRoundReadyResponse();
                
                // BaseSocket 필드들 설정
                response.SetBaseFields(dict);
                
                // NextRoundReadyResponse 필드들 설정
                if (dict.TryGetValue("userId", out var userIdObj))
                    response.userId = userIdObj?.ToString() ?? "";

                Debug.Log($"[NextRoundReadyResponse] FromPayload 성공 - userId: {response.userId}");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NextRoundReadyResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}