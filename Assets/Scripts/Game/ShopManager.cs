using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using BalatroOnline.InGame;

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
    public Transform[] ownedJokerPositions; // 구매한 조커 카드 위치들 (JokerPos0~4)

    private List<CardJoker> shopJokers = new List<CardJoker>();
    private CardJoker currentlySelectedCard; // 현재 선택된 카드
    
    // 구매한 조커 카드들 관리
    private List<CardJoker> ownedJokers = new List<CardJoker>();
    private List<ServerCardData> ownedJokerData = new List<ServerCardData>();

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
            cj.SetData(card.id, card.type, card.price, sprite, card.name, card.description,
                (clickedCard) =>
                {
                    OnCardClicked(clickedCard);
                }
            );
            shopJokers.Add(cj);
        }
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
        ShowJokerInfo(clickedCard.cardName, clickedCard.cardDesc);
        
        // Buy 버튼 활성화
        SetBuyButtonState(true);
        
        Debug.Log($"[ShopManager] 카드 선택됨: {clickedCard.cardName} (가격: ${clickedCard.price})");
    }

    // Buy 버튼 클릭 시 호출되는 메서드
    private void OnBuyButtonClicked()
    {
        if (currentlySelectedCard == null)
        {
            Debug.LogWarning("[ShopManager] 선택된 카드가 없습니다.");
            return;
        }

        Debug.Log($"[ShopManager] 구매 시도: {currentlySelectedCard.cardName} (가격: ${currentlySelectedCard.price})");
        
        // 서버에 구매 요청 전송
        var roomId = BalatroOnline.Common.SessionManager.Instance.CurrentRoomId;
        var buyData = new Dictionary<string, object>
        {
            { "roomId", roomId },
            { "cardId", currentlySelectedCard.id },
            { "cardType", currentlySelectedCard.type },
            { "price", currentlySelectedCard.price }
        };
        
        SocketManager.Instance.EmitToServer("buyCard", buyData);
        
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
            if (shopJokers[i].id == cardId)
            {
                cardToRemove = shopJokers[i];
                break;
            }
        }
        
        if (cardToRemove != null)
        {
            Debug.Log($"[ShopManager] 카드 제거: {cardToRemove.cardName} ({cardId})");
            
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

    // 구매한 조커 카드 추가
    public void AddOwnedJoker(ServerCardData cardData)
    {
        Debug.Log($"[ShopManager] 구매한 조커 카드 추가: {cardData.name} ({cardData.id})");
        
        // 데이터 저장
        ownedJokerData.Add(cardData);
        
        // UI에 표시
        ShowOwnedJokers();
    }

    // 구매한 조커 카드들 UI에 표시
    public void ShowOwnedJokers()
    {
        Debug.Log($"[ShopManager] 구매한 조커 카드들 표시 시작. 총 {ownedJokerData.Count}개");
        
        // 기존 UI 오브젝트들 제거
        foreach (var joker in ownedJokers)
        {
            if (joker != null)
                Destroy(joker.gameObject);
        }
        ownedJokers.Clear();

        // 구매한 조커 카드들을 UI에 표시
        for (int i = 0; i < ownedJokerData.Count && i < ownedJokerPositions.Length; i++)
        {
            var cardData = ownedJokerData[i];
            var position = ownedJokerPositions[i];
            
            if (position != null)
            {
                // 조커 카드 오브젝트 생성
                GameObject jokerObj = Instantiate(cardJokerPrefab, position);
                var cardJoker = jokerObj.GetComponent<CardJoker>();
                
                // 스프라이트 설정
                Sprite sprite = null;
                if (cardData.sprite >= 0 && cardData.sprite < SpriteManager.Instance.jokerSprites.Count)
                {
                    sprite = SpriteManager.Instance.jokerSprites[cardData.sprite];
                }
                
                // 카드 데이터 설정 (가격은 표시하지 않음)
                cardJoker.SetData(cardData.id, cardData.type, 0, sprite, cardData.name, cardData.description);
                
                // 가격 텍스트 숨기기 (소유한 카드이므로)
                if (cardJoker.priceText != null)
                {
                    cardJoker.priceText.gameObject.SetActive(false);
                }
                
                // 클릭 이벤트 연결 (정보 표시용)
                cardJoker.SetData(cardData.id, cardData.type, 0, sprite, cardData.name, cardData.description,
                    (clickedCard) =>
                    {
                        ShowJokerInfo(clickedCard.cardName, clickedCard.cardDesc);
                    }
                );
                
                ownedJokers.Add(cardJoker);
                
                Debug.Log($"[ShopManager] 조커 카드 UI 생성: {cardData.name} at position {i}");
            }
        }
        
        Debug.Log($"[ShopManager] 구매한 조커 카드들 표시 완료. 총 {ownedJokers.Count}개");
    }

    // 구매한 조커 카드 데이터 반환
    public List<ServerCardData> GetOwnedJokers()
    {
        return new List<ServerCardData>(ownedJokerData);
    }

    // 구매한 조커 카드 개수 반환
    public int GetOwnedJokerCount()
    {
        return ownedJokerData.Count;
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