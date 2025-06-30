using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalatroOnline.Network.Protocol;

namespace BalatroOnline.Lobby
{
    /// <summary>
    /// 방 리스트 아이템 컴포넌트
    /// </summary>
    public class RoomListItem : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI roomNameText;
        public TextMeshProUGUI playerCountText;
        public TextMeshProUGUI roomIdText;
        public Button joinButton;

        private string roomId;
        private System.Action<string> onJoinCallback;

        private void Awake()
        {
            if (joinButton != null)
            {
                joinButton.onClick.AddListener(OnJoinButtonClicked);
            }
        }

        public void SetRoomData(RoomData room, System.Action<string> joinCallback)
        {
            roomId = room.roomId;
            onJoinCallback = joinCallback;

            if (roomNameText != null)
            {
                roomNameText.text = room.name;
            }

            if (playerCountText != null)
            {
                playerCountText.text = $"{room.players}/{room.maxPlayers}";
            }

            if (roomIdText != null)
            {
                roomIdText.text = $"ID: {room.roomId.Substring(0, 8)}...";
            }

            // 방이 가득 찼는지 확인
            if (joinButton != null)
            {
                bool isFull = room.players >= room.maxPlayers;
                joinButton.interactable = !isFull;
                
                if (isFull)
                {
                    joinButton.GetComponentInChildren<TextMeshProUGUI>().text = "가득참";
                }
                else
                {
                    joinButton.GetComponentInChildren<TextMeshProUGUI>().text = "입장";
                }
            }
        }

        private void OnJoinButtonClicked()
        {
            onJoinCallback?.Invoke(roomId);
        }
    }
} 