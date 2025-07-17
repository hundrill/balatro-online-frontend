namespace BalatroOnline.Network.Protocol
{
    [System.Serializable]
    public class RoomListResponse : BaseApi
    {
        public RoomData[] rooms;
    }
} 