using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace BalatroOnline.Game
{
    public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("카드 이미지 (자기 자신 Image 컴포넌트)")]
        public Image cardImage;

        private bool isSelected = false;
        private Vector3 originalWorldPosition;
        private RectTransform rectTransform;
        private const float SELECT_OFFSET_Y = 50f;
        private bool isDragging = false;
        private Canvas canvas;
        // 이동 코루틴 핸들러
        private Coroutine moveCoroutine = null;

        public System.Action<Card> OnMoveComplete;
        public MySlot myPlayer; // 외부에서 할당

        public bool isInteractable = true;
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }

        void Awake()
        {
            // cardImage가 할당되지 않았다면 자동으로 찾기
            if (cardImage == null)
            {
                cardImage = GetComponent<Image>();
                if (cardImage == null)
                {
                    Debug.LogError("Card에 Image 컴포넌트가 없습니다!", this);
                }
            }
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalWorldPosition = rectTransform.position;
            }
            // 상위에서 Canvas 찾기 (좌표 변환용)
            canvas = GetComponentInParent<Canvas>();
        }

        public bool IsSelected() => isSelected;
        public void Deselect()
        {
            if (isSelected && rectTransform != null)
            {
                rectTransform.position = new Vector3(rectTransform.position.x, originalWorldPosition.y, rectTransform.position.z);
                originalWorldPosition = rectTransform.position;
                isSelected = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable) return;
            if (isDragging) return; // 드래그 중에는 클릭 토글 무시
            if (rectTransform == null) return;
            bool wasSelected = isSelected;
            if (!isSelected)
            {
                if (myPlayer != null && !myPlayer.CanSelectMore())
                {
                    //Debug.Log("최대 5장까지만 선택할 수 있습니다.");
                    return;
                }
                originalWorldPosition = rectTransform.position;
                rectTransform.position = originalWorldPosition + new Vector3(0, SELECT_OFFSET_Y, 0);
                isSelected = true;
            }
            else
            {
                Deselect();
            }
            if (myPlayer != null)
                myPlayer.OnCardClicked(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isInteractable) return;
            isDragging = true;
            if (rectTransform != null)
            {
                originalWorldPosition = rectTransform.position;
            }
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isInteractable) return;
            if (rectTransform == null || canvas == null) return;
            Vector3 worldPoint;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out worldPoint))
            {
                rectTransform.position = worldPoint;
                var myPlayer = FindFirstObjectByType<MySlot>();
                if (myPlayer != null)
                {
                    myPlayer.OnCardDrag(this, eventData.position.x);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isInteractable) return;
            isDragging = false;
            var myPlayer = FindFirstObjectByType<MySlot>();
            if (myPlayer != null)
            {
                myPlayer.OnCardDragEnd(this);
            }
        }

        /// <summary>
        /// 카드 Sprite를 세팅합니다.
        /// </summary>
        public void SetCard(Sprite cardSprite)
        {
            if (cardImage == null)
            {
                Debug.LogError("Card image가 null입니다! Awake에서 할당을 확인하세요.", this);
                return;
            }
            cardImage.sprite = cardSprite;
        }

        /// <summary>
        /// 카드 뒷면 등으로 바꿀 때 사용
        /// </summary>
        public void SetBack(Sprite backSprite)
        {
            if (cardImage == null)
            {
                Debug.LogError("Card image가 null입니다! Awake에서 할당을 확인하세요.", this);
                return;
            }
            cardImage.sprite = backSprite;
        }

        // 카드가 지정 위치로 이동하는 메서드
        public void MoveToPosition(Vector3 pos, int? siblingIndex = null)
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

        public IEnumerator MoveToPositionCoroutine(Vector3 pos, float duration, int? siblingIndex = null)
        {
            //Debug.Log($"[Card] MoveToPositionCoroutine 진입: {gameObject.name}, target={pos}");
            if (rectTransform == null)
            {
                Debug.LogError($"[Card] rectTransform이 null입니다! {gameObject.name}");
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
            //Debug.Log($"[Card] MoveToPositionCoroutine 완료: {gameObject.name}, OnMoveComplete null? {OnMoveComplete == null}");
            OnMoveComplete?.Invoke(this);
        }

        // 카드 페이드아웃 후 파괴
        public void FadeOutAndDestroy(float duration = 0.5f)
        {
            StartCoroutine(FadeOutAndDestroyCoroutine(duration));
        }

        private IEnumerator FadeOutAndDestroyCoroutine(float duration)
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            float startAlpha = cg.alpha;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                cg.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            cg.alpha = 0f;
            Destroy(gameObject);
        }
    }
}