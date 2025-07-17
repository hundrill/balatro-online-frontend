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
        public void OnReceiveCardDeal(List<CardData> myCards, List<string> opponentIds)
        {
            if (myPlayer != null){
                myPlayer.ReceiveInitialCards(myCards);

            }

            // 상대방 카드 딜링 (유저 할당 없이 카드만)
            if (cardDealer != null && opponentIds != null)
            {
                for (int i = 0; i < opponentIds.Count && i < 3; i++)
                {
                    var oppSlot = opponentSlots[i];
                    if (oppSlot != null)
                        oppSlot.ClearHandCards(); // 혹시 남아있을 수 있으니 초기화
                    for (int j = 0; j < 8; j++)
                    {
                        Card card = cardDealer.DealOpponentCard(i, j);
                        if (oppSlot != null && card != null)
                            oppSlot.handCards.Add(card);
                    }

                }
            }
        }
    }
}