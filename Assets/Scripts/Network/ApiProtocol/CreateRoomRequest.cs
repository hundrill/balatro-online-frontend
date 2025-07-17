namespace BalatroOnline.Network.Protocol
{
    [System.Serializable]
    public class CreateRoomRequest : BaseApi
    {
        public string name;
        public int maxPlayers;
        public int silverSeedChip;
        public int goldSeedChip;
        public int silverBettingChip;
        public int goldBettingChip;
    }
}