using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BalatroOnline.Game
{
    public class OpponentSlot : MonoBehaviour
    {
        public TextMeshProUGUI nicknameText; // 인스펙터에서 연결
        private string currentUserId;

        public void SetUser(string userId, string nickname)
        {
            Debug.Log("SetUser "+userId+" "+nickname);
            currentUserId = userId;
            if (nicknameText != null)
            {
                nicknameText.text = string.IsNullOrEmpty(nickname) ? userId : nickname;
                Debug.Log("SetUser2 "+userId+" "+nickname);
            }
            gameObject.SetActive(true); // 슬롯 전체 활성화

            Debug.Log("SetUser3 "+userId+" "+nickname);
        }

        public void ClearSlot()
        {
            currentUserId = null;
            if (nicknameText != null)
            {
                nicknameText.text = "";
            }
            gameObject.SetActive(false); // 슬롯 전체 비활성화
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(currentUserId);
        }

        public string GetUserId()
        {
            return currentUserId;
        }
    }
}