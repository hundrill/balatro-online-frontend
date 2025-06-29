namespace BalatroOnline.Network.Protocol
{
    [System.Serializable]
    public class CreateRoomResponse : BaseResponse
    {
        public string roomId;
        public RoomData room;
    }

    [System.Serializable]
    public class RoomData
    {
        public string roomId;
        public string name;
        public int maxPlayers;
        public int players;
        public string status;
        public long createdAt;
        public string ownerId;
    }
} 