using System.Collections.Generic;
using UnityEngine;

namespace BalatroOnline.Game
{
    public class OpponentSlot : MonoBehaviour
    {
        public Transform[] handPositions; // 상대방 핸드 위치들
        public GameObject cardBackPrefab; // 카드 뒷면 프리팹
        private List<GameObject> cardBacks = new List<GameObject>();

        public void SetCardCount(int count)
        {
            // 기존 카드 뒷면 제거
            foreach (var go in cardBacks)
                Destroy(go);
            cardBacks.Clear();
            // count만큼 뒷면 카드 생성
            for (int i = 0; i < count && i < handPositions.Length; i++)
            {
                var go = Instantiate(cardBackPrefab, handPositions[i].position, Quaternion.identity, handPositions[i]);
                cardBacks.Add(go);
            }
        }
    }
} 