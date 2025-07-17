using System;
using System.Collections.Generic;
using System.Reflection;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class LoginResponse : BaseSocket
{
    public const string EventNameConst = "LoginResponse";
    public override string EventName => EventNameConst;

    public string email;
    public string nickname;
    public int silverChip;
    public int goldChip;
    public string createdAt;

    // payload를 받아서 LoginResponse로 파싱하는 정적 함수
    public static LoginResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new LoginResponse();
                
                // BaseSocket 필드들 설정
                response.SetBaseFields(dict);
                
                // LoginResponse 필드들 설정
                if (dict.TryGetValue("email", out var emailObj))
                    response.email = emailObj?.ToString() ?? "";
                if (dict.TryGetValue("nickname", out var nicknameObj))
                    response.nickname = nicknameObj?.ToString() ?? "";
                if (dict.TryGetValue("silverChip", out var silverChipObj))
                    response.silverChip = Convert.ToInt32(silverChipObj);
                if (dict.TryGetValue("goldChip", out var goldChipObj))
                    response.goldChip = Convert.ToInt32(goldChipObj);
                if (dict.TryGetValue("createdAt", out var createdAtObj))
                    response.createdAt = createdAtObj?.ToString() ?? "";

                Debug.Log($"[LoginResponse] FromPayload 성공 - email: {response.email}, nickname: {response.nickname}");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LoginResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}