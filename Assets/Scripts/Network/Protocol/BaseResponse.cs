namespace BalatroOnline.Network.Protocol
{
    [System.Serializable]
    public class BaseResponse
    {
        public bool success;
        public int code;
        public string message;
    }
} 