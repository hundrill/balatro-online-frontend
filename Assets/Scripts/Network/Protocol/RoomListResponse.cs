namespace BalatroOnline.Network.Protocol
{
    [System.Serializable]
    public class RoomListResponse : BaseResponse
    {
        public RoomData[] rooms;
    }
} 