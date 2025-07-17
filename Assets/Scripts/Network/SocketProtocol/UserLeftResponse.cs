using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class UserLeftResponse : BaseSocket
{
    public override string EventName => EventNameConst;
    public const string EventNameConst = "UserLeftResponse";

    public static UserLeftResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new UserLeftResponse();
                
                // BaseSocket 필드들 설정
                response.SetBaseFields(dict);

                Debug.Log($"[UserLeftResponse] FromPayload 성공");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UserLeftResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}