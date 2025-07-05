using UnityEngine;
using BalatroOnline.Game;
using System.Collections.Generic;

namespace BalatroOnline.Common
{
    /// <summary>
    /// 게임 전체를 관리하는 싱글톤 매니저
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public CardDealer cardDealer; // 카드 딜러 참조
        public MySlot myPlayer; // 내 플레이어 참조
        public List<OpponentSlot> opponentSlots; // 상대방 슬롯들 (Inspector에서 할당)

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 인게임 씬에서만 사용하므로 제거
        }

        private void Start()
        {
            // 게임 시작 시 자동 딜링 제거 (아무 동작 없음)
        }

        // TODO: 전체 게임 상태, 데이터 관리 등


        // 서버에서 카드 분배 메시지 수신 시 호출
        public void OnReceiveCardDeal(List<CardData> myCards, List<int> opponentCardCounts)
        {
            if (myPlayer != null)
                myPlayer.ReceiveInitialCards(myCards);

            /*
            for (int i = 0; i < opponentSlots.Count; i++)
            {
                if (i < opponentCardCounts.Count)
                    opponentSlots[i].SetCardCount(opponentCardCounts[i]);
                else
                    opponentSlots[i].SetCardCount(0);
            }
            */
        }
    }
}