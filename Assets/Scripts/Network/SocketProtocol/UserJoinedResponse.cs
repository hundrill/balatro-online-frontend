using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class UserJoinedResponse : BaseSocket
{
    public override string EventName => EventNameConst;
    public const string EventNameConst = "UserJoinedResponse";

    public string userId;

    public static UserJoinedResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new UserJoinedResponse();
                
                // BaseSocket 필드들 설정
                response.SetBaseFields(dict);
                
                // UserJoinedResponse 필드들 설정
                if (dict.TryGetValue("userId", out var userIdObj))
                    response.userId = userIdObj?.ToString() ?? "";

                Debug.Log($"[UserJoinedResponse] FromPayload 성공 - userId: {response.userId}");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UserJoinedResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}