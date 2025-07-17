using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class HandPlayReadyResponse : BaseSocket
{
    public override string EventName => EventNameConst;
    public const string EventNameConst = "HandPlayReadyResponse";

    public string userId;

    public static HandPlayReadyResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new HandPlayReadyResponse();
                
                // BaseSocket 필드들 설정
                response.SetBaseFields(dict);
                
                // HandPlayReadyResponse 필드들 설정
                if (dict.TryGetValue("userId", out var userIdObj))
                    response.userId = userIdObj?.ToString() ?? "";

                Debug.Log($"[HandPlayReadyResponse] FromPayload 성공 - userId: {response.userId}");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HandPlayReadyResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}