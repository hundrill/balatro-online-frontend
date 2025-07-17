using System;
using System.Collections.Generic;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class ErrorResponse : BaseSocket
{
    public override string EventName => "ErrorResponse";

    public string message;

    public static ErrorResponse FromPayload(object payload)
    {
        try
        {
            if (payload is Dictionary<string, object> dict)
            {
                var response = new ErrorResponse();
                
                // BaseSocket 필드들 설정
                response.SetBaseFields(dict);

                Debug.Log($"[ErrorResponse] FromPayload 성공 - message: {response.message}");
                return response;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ErrorResponse] FromPayload 오류: {e.Message}");
        }
        return null;
    }
}