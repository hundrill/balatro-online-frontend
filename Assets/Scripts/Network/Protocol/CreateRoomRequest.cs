namespace BalatroOnline.Network.Protocol
{
    [System.Serializable]
    public class CreateRoomRequest
    {
        public string name;
        public int maxPlayers;
    }
}