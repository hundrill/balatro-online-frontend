using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using BalatroOnline.InGame;
using BalatroOnline.Common;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public Transform[] shopCardSlots; // 인스펙터에서 5개 슬롯 오브젝트 할당
    public GameObject cardJokerPrefab; // CardJoker 프리팹
    public GameObject shopPanel; // 샵 전체 패널 오브젝트(인스펙터 연결)

    // === 조커 정보 패널 ===
    public GameObject jokerInfoPanel; // Shop/JokerInfo
    public TextMeshProUGUI jokerNameText; // Shop/JokerInfo/Name
    public TextMeshProUGUI jokerInfoText; // Shop/JokerInfo/Info

    // === Buy 버튼 ===
    public Button buyButton; // Buy 버튼 (인스펙터에서 연결)

    // === 구매한 조커 카드 표시용 ===
    // public Transform[] ownedJokerPositions; // 구매한 조커 카드 위치들 (JokerPos0~4)

    private List<CardJoker> shopJokers = new List<CardJoker>();
    private CardJoker currentlySelectedCard; // 현재 선택된 카드

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        // Buy 버튼 이벤트 연결
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        // 초기 상태: Buy 버튼 비활성화
        SetBuyButtonState(false);
    }

    // 서버에서 받은 shopCards 리스트로 샵 UI 갱신
    public void ShowShop(List<ServerCardData> shopCards)
    {
        // 샵 패널 오픈
        if (shopPanel != null) shopPanel.SetActive(true);

        // 기존 카드 오브젝트 제거
        foreach (var c in shopJokers) Destroy(c.gameObject);
        shopJokers.Clear();

        // 선택 상태 초기화
        currentlySelectedCard = null;
        SetBuyButtonState(false);

        for (int i = 0; i < shopCards.Count && i < shopCardSlots.Length; i++)
        {
            var card = shopCards[i];
            var go = Instantiate(cardJokerPrefab, shopPanel.transform);
            go.transform.position = shopCardSlots[i].position;
            var cj = go.GetComponent<CardJoker>();
            Sprite sprite = null;
            if (card.type == "joker")
            {
                if (card.sprite >= 0 && card.sprite < SpriteManager.Instance.jokerSprites.Count)
                    sprite = SpriteManager.Instance.jokerSprites[card.sprite];
            }
            else if (card.type == "planet")
            {
                if (card.sprite >= 0 && card.sprite < SpriteManager.Instance.planetSprites.Count)
                    sprite = SpriteManager.Instance.planetSprites[card.sprite];
            }
            else if (card.type == "tarot")
            {
                if (card.sprite >= 0 && card.sprite < SpriteManager.Instance.tarotSprites.Count)
                    sprite = SpriteManager.Instance.tarotSprites[card.sprite];
            }
            cj.SetData(card.id, sprite, (clickedCard) =>
            {
                OnCardClicked(clickedCard);
            });
            shopJokers.Add(cj);
        }
    }

    // 다시뽑기 카드로 샵 UI 갱신
    public void ShowReRollCards(List<ServerCardData> reRollCards)
    {
        Debug.Log($"[ShopManager] 다시뽑기 카드 {reRollCards.Count}장으로 샵 갱신");

        // 기존 카드 오브젝트 제거
        foreach (var c in shopJokers) Destroy(c.gameObject);
        shopJokers.Clear();

        // 선택 상태 초기화
        currentlySelectedCard = null;
        SetBuyButtonState(false);

        for (int i = 0; i < reRollCards.Count && i < shopCardSlots.Length; i++)
        {
            var card = reRollCards[i];
            var go = Instantiate(cardJokerPrefab, shopPanel.transform);
            go.transform.position = shopCardSlots[i].position;
            var cj = go.GetComponent<CardJoker>();
            Sprite sprite = null;
            if (card.type == "joker")
            {
                if (card.sprite >= 0 && card.sprite < SpriteManager.Instance.jokerSprites.Count)
                    sprite = SpriteManager.Instance.jokerSprites[card.sprite];
            }
            else if (card.type == "planet")
            {
                if (card.sprite >= 0 && card.sprite < SpriteManager.Instance.planetSprites.Count)
                    sprite = SpriteManager.Instance.planetSprites[card.sprite];
            }
            else if (card.type == "tarot")
            {
                if (card.sprite >= 0 && card.sprite < SpriteManager.Instance.tarotSprites.Count)
                    sprite = SpriteManager.Instance.tarotSprites[card.sprite];
            }
            cj.SetData(card.id, sprite, (clickedCard) =>
            {
                OnCardClicked(clickedCard);
            });
            shopJokers.Add(cj);
        }

        // 다시뽑기 완료 메시지 표시
        MessageDialogManager.Instance.Show("새로운 조커 카드가 준비되었습니다!", null, 2f);
    }

    // 카드 클릭 시 호출되는 메서드
    private void OnCardClicked(CardJoker clickedCard)
    {
        // 이전 선택된 카드의 선택 상태 해제
        if (currentlySelectedCard != null)
        {
            currentlySelectedCard.SetSelected(false);
        }

        // 새로 클릭된 카드를 선택 상태로 설정
        currentlySelectedCard = clickedCard;
        currentlySelectedCard.SetSelected(true);

        // 정보 패널 표시
        ShowJokerInfo(clickedCard.jokerCard.name, clickedCard.jokerCard.GetDescription());

        // Buy 버튼 활성화
        SetBuyButtonState(true);

        Debug.Log($"[ShopManager] 카드 선택됨: {clickedCard.jokerCard.name} (가격: ${clickedCard.jokerCard.price})");
    }

    // Buy 버튼 클릭 시 호출되는 메서드
    private void OnBuyButtonClicked()
    {
        if (currentlySelectedCard == null)
        {
            Debug.LogWarning("[ShopManager] 선택된 카드가 없습니다.");
            return;
        }

        Debug.Log($"[ShopManager] 구매 시도: {currentlySelectedCard.jokerCard.name} (가격: ${currentlySelectedCard.jokerCard.price})");

        // 서버에 구매 요청 전송
        var roomId = BalatroOnline.Common.SessionManager.Instance.CurrentRoomId;
        // var buyData = new Dictionary<string, object>
        // {
        //     { "roomId", roomId },
        //     { "cardId", currentlySelectedCard.jokerCard.id },
        //     { "cardType", "joker" },
        //     { "price", currentlySelectedCard.jokerCard.price }
        // };

        // SocketManager.Instance.EmitToServer("buyCard", buyData);
        SocketManager.Instance.EmitToServer(new BuyCardRequest(roomId, currentlySelectedCard.jokerCard.id, "joker", currentlySelectedCard.jokerCard.price));

        // 구매 후 선택 상태 초기화
        currentlySelectedCard.SetSelected(false);
        currentlySelectedCard = null;
        SetBuyButtonState(false);
        HideJokerInfo();
    }

    // Buy 버튼 상태 설정
    private void SetBuyButtonState(bool interactable)
    {
        if (buyButton != null)
        {
            buyButton.interactable = interactable;
        }
    }

    // 조커 카드 클릭 시 정보 패널 표시
    public void ShowJokerInfo(string name, string desc)
    {
        if (jokerInfoPanel != null) jokerInfoPanel.SetActive(true);
        if (jokerNameText != null) jokerNameText.text = name;
        if (jokerInfoText != null) jokerInfoText.text = desc;
    }

    public void HideJokerInfo()
    {
        if (jokerInfoPanel != null) jokerInfoPanel.SetActive(false);
        if (jokerNameText != null) jokerNameText.text = "";
        if (jokerInfoText != null) jokerInfoText.text = "";
    }

    // 샵 패널 닫기
    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);

        // 선택 상태 초기화
        if (currentlySelectedCard != null)
        {
            currentlySelectedCard.SetSelected(false);
            currentlySelectedCard = null;
        }
        SetBuyButtonState(false);
        HideJokerInfo();
    }

    // 구매된 카드를 UI에서 제거
    public void RemovePurchasedCard(string cardId)
    {
        Debug.Log($"[ShopManager] 구매된 카드 제거 시도: {cardId}");

        // shopJokers 리스트에서 해당 카드 찾기
        CardJoker cardToRemove = null;
        for (int i = 0; i < shopJokers.Count; i++)
        {
            if (shopJokers[i].jokerCard.id == cardId)
            {
                cardToRemove = shopJokers[i];
                break;
            }
        }

        if (cardToRemove != null)
        {
            Debug.Log($"[ShopManager] 카드 제거: {cardToRemove.jokerCard.name} ({cardId})");

            // 선택된 카드였다면 선택 상태 해제
            if (currentlySelectedCard == cardToRemove)
            {
                currentlySelectedCard = null;
                SetBuyButtonState(false);
                HideJokerInfo();
            }

            // UI에서 카드 오브젝트 제거
            shopJokers.Remove(cardToRemove);
            Destroy(cardToRemove.gameObject);

            Debug.Log($"[ShopManager] 카드 제거 완료. 남은 카드 수: {shopJokers.Count}");
        }
        else
        {
            Debug.LogWarning($"[ShopManager] 제거할 카드를 찾을 수 없음: {cardId}");
        }
    }
}

// 서버에서 내려주는 카드 데이터 구조 예시
[System.Serializable]
public class ServerCardData
{
    public string id;
    public string type; // "joker", "planet", "tarot"
    public int price;
    public int sprite; // 서버에서 내려주는 스프라이트 인덱스
    public string name;
    public string description;
}