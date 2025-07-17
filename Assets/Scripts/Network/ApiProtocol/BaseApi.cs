namespace BalatroOnline.Network.Protocol
{
    [System.Serializable]
    public class BaseApi
    {
        public bool success;
        public int code;
        public string message;

        public string ToJson()
        {
            return UnityEngine.JsonUtility.ToJson(this);
        }

        public static T FromJson<T>(string json) where T : BaseApi
        {
            return UnityEngine.JsonUtility.FromJson<T>(json);
        }
    }
} 