using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;
using System.Linq; // Added for FirstOrDefault
using System.Collections;
using BalatroOnline.Game; // Added for IEnumerator

public class CardJoker : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image cardImage;
    public TextMeshProUGUI priceText;

    // [HideInInspector] public string id;
    // [HideInInspector] public string type; // "joker", "planet", "tarot"
    // [HideInInspector] public int price;
    // [HideInInspector] public string cardName;
    // [HideInInspector] public string cardDesc;
    [HideInInspector] public JokerCard jokerCard; // 전체 조커 정보 참조

    private Action<CardJoker> onClick; // 외부 콜백
    private bool isSelected = false; // 선택 상태
    private bool isDragging = false; // 드래그 상태
    private Vector3 originalPosition; // 원래 위치
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector3 dragStartPosition; // 드래그 시작 위치
    private const float DRAG_THRESHOLD = 10f; // 드래그로 간주할 최소 거리
    private bool dragEnabled = false; // 드래그 기능 활성화 여부
    private MySlot mySlot; // MySlot 참조
    private Coroutine moveCoroutine = null; // 이동 코루틴 핸들러

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalPosition = rectTransform.position;
        }
        // 상위에서 Canvas 찾기 (좌표 변환용)
        canvas = GetComponentInParent<Canvas>();
    }

    // JokerCard 전체 정보를 받아서 세팅
    public void SetData(string id, Sprite sprite, Action<CardJoker> onClickCallback = null, bool enableDrag = false)
    {
        // id로 JokerManager에서 해당 카드 정보 찾기
        var card = JokerManager.Instance.allJokers.FirstOrDefault(j => j.id == id);
        this.jokerCard = card;
        // this.id = card != null ? card.id : id;
        // this.type = "joker";
        // this.price = card != null ? card.price : 0;
        // this.cardName = card != null ? card.name : "";
        // this.cardDesc = card != null ? card.description : "";
        this.onClick = onClickCallback;
        this.dragEnabled = enableDrag;
        if (priceText) priceText.text = string.Format("${0}", this.jokerCard.price);
        if (cardImage) cardImage.sprite = sprite;
        SetSelected(false);

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickShowInfo);
        }
    }

    // 샵에서 카드 클릭 시 외부 콜백 호출
    public void OnClickShowInfo()
    {
        if (isDragging) return; // 드래그 중에는 클릭 무시

        if (onClick != null)
            onClick(this);
    }

    // 선택 상태 설정
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // 선택된 카드는 약간 확대 효과 (선택사항)
        if (selected)
        {
            transform.localScale = Vector3.one * 1.1f;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    // 현재 선택 상태 반환
    public bool IsSelected()
    {
        return isSelected;
    }

    // IBeginDragHandler 구현
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!dragEnabled) return; // 드래그 기능이 비활성화되었으면 드래그 시작 무시
        isDragging = true;
        if (rectTransform != null)
        {
            originalPosition = rectTransform.position;
            dragStartPosition = rectTransform.position;
        }
        transform.SetAsLastSibling();
    }

    // IDragHandler 구현
    public void OnDrag(PointerEventData eventData)
    {
        if (!dragEnabled) return; // 드래그 기능이 비활성화되었으면 드래그 중 무시
        if (rectTransform == null || canvas == null) return;
        Vector3 worldPoint;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPoint))
        {
            rectTransform.position = worldPoint;

            // MySlot에 드래그 정보 전달
            if (mySlot != null)
            {
                mySlot.OnJokerDrag(this, eventData.position.x);
            }
        }
    }

    // IEndDragHandler 구현
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragEnabled) return; // 드래그 기능이 비활성화되었으면 드래그 종료 무시
        isDragging = false;

        // MySlot에 드래그 종료 알림
        if (mySlot != null)
        {
            mySlot.OnJokerDragEnd(this);
        }
        else
        {
            // 원래 위치로 돌아가기 (MySlot이 없는 경우)
            if (rectTransform != null)
            {
                rectTransform.position = originalPosition;
            }
        }
    }

    // IPointerClickHandler 구현
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return; // 드래그 중에는 클릭 무시

        // 드래그 기능이 활성화된 경우에만 드래그 거리 체크
        if (dragEnabled && rectTransform != null)
        {
            float dragDistance = Vector3.Distance(dragStartPosition, rectTransform.position);
            if (dragDistance > DRAG_THRESHOLD)
            {
                // 충분히 드래그했으면 클릭 이벤트 무시
                return;
            }
        }

        OnClickShowInfo();
    }

    // 카드가 지정 위치로 이동하는 메서드
    public void MoveToPosition(Vector3 pos)
    {
        if (rectTransform != null)
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            moveCoroutine = StartCoroutine(MoveToPositionCoroutine(pos, 0.15f));
        }
    }

    public IEnumerator MoveToPositionCoroutine(Vector3 pos, float duration)
    {
        if (rectTransform == null)
        {
            Debug.LogError($"[CardJoker] rectTransform이 null입니다! {gameObject.name}");
            yield break;
        }
        Vector3 start = rectTransform.position;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            rectTransform.position = Vector3.Lerp(start, pos, t);
            yield return null;
        }
        rectTransform.position = pos;
        moveCoroutine = null;
    }

    // MySlot 참조 설정
    public void SetMySlot(MySlot slot)
    {
        mySlot = slot;
    }

    // // 샵에서 선택/구매 등 UI 이벤트 연결
    // public void OnClickBuy()
    // {
    //     // ShopManager.Instance.BuyJoker(this);
    // }
}