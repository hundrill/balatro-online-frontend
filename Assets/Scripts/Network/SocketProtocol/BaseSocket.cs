using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BalatroOnline.Network.SocketProtocol
{
    [Serializable]
    public abstract class BaseSocket
    {
        public bool success;
        public int code;
        public string message;

        public abstract string EventName { get; }

        // 공통 필드들을 설정하는 메서드
        protected void SetBaseFields(Dictionary<string, object> dict)
        {
            if (dict.TryGetValue("success", out var successObj))
                success = Convert.ToBoolean(successObj);
            if (dict.TryGetValue("code", out var codeObj))
                code = Convert.ToInt32(codeObj);
            if (dict.TryGetValue("message", out var messageObj))
                message = messageObj?.ToString() ?? "";
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        // 모든 public 필드를 Dictionary로 변환
        public virtual Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                dict[field.Name] = field.GetValue(this);
            }
            return dict;
        }

        public static T FromJson<T>(string json) where T : BaseSocket
        {
            return JsonUtility.FromJson<T>(json);
        }

        // eventName만 읽어서 확인하는 함수
        public static string GetEventNameFromJson(string json)
        {
            try
            {
                // JSON에서 eventName 필드만 추출
                var jsonData = JsonUtility.FromJson<Dictionary<string, object>>(json);
                if (jsonData != null && jsonData.ContainsKey("eventName"))
                {
                    return jsonData["eventName"].ToString();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BaseSocket] GetEventNameFromJson 오류: {e.Message}");
            }
            return null;
        }

        // object data에서 eventName 추출하는 함수
        public static string GetEventNameFromData(object data)
        {
            try
            {
                // data가 Dictionary<string, object>인 경우 직접 처리
                if (data is Dictionary<string, object> dict)
                {
                    if (dict.ContainsKey("eventName"))
                    {
                        string eventName = dict["eventName"].ToString();
                        Debug.Log($"[BaseSocket] GetEventNameFromData - eventName: {eventName}");
                        return eventName;
                    }
                }

                // JSON 문자열로 변환 시도
                string jsonString = JsonUtility.ToJson(data);
                Debug.Log($"[BaseSocket] GetEventNameFromData - JSON: {jsonString}");

                // 간단한 문자열 파싱으로 eventName 추출
                if (jsonString.Contains("\"eventName\":"))
                {
                    int startIndex = jsonString.IndexOf("\"eventName\":") + 12;
                    int endIndex = jsonString.IndexOf("\"", startIndex + 1);
                    if (endIndex > startIndex)
                    {
                        string eventName = jsonString.Substring(startIndex, endIndex - startIndex);
                        Debug.Log($"[BaseSocket] GetEventNameFromData - eventName: {eventName}");
                        return eventName;
                    }
                }

                Debug.LogWarning("[BaseSocket] GetEventNameFromData - eventName 필드를 찾을 수 없음");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BaseSocket] GetEventNameFromData 오류: {e.Message}");
            }
            return null;
        }
    }
}