using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace BalatroOnline.Game
{
    /// <summary>
    /// 칩 타입을 정의하는 enum
    /// </summary>
    public enum ChipType
    {
        Silver,
        Gold
    }

    /// <summary>
    /// 칩 이동 애니메이션을 관리하는 컴포넌트
    /// </summary>
    public class ChipController : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float moveDuration = 0.8f;
        public float randomOffsetRange = 20f; // 목표 위치 주변 랜덤 범위
        public Ease moveEase = Ease.OutQuad;
        
        [Header("Seed Chip Effects")]
        public Color silverSeedChipColor = Color.red; // 실버 시드 칩 색상
        public Color goldSeedChipColor = Color.yellow; // 골드 시드 칩 색상
        public float seedChipScale = 1.2f; // 시드 칩 크기
        public float seedChipGlowDuration = 0.3f; // 시드 칩 글로우 효과 지속시간
        
        private Vector3 startPosition;
        private bool isAnimating = false;
        private SpriteRenderer spriteRenderer;
        private UnityEngine.UI.Image imageComponent;
        private Color originalColor;
        private Vector3 originalScale;
        private int originalSortingOrder; // 원래 정렬 순서 저장

        private void Awake()
        {
            startPosition = transform.position;
            spriteRenderer = GetComponent<SpriteRenderer>();
            imageComponent = GetComponent<UnityEngine.UI.Image>();
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                originalScale = transform.localScale;
                originalSortingOrder = spriteRenderer.sortingOrder;
            }
            else if (imageComponent != null)
            {
                originalColor = imageComponent.color;
                originalScale = transform.localScale;
                originalSortingOrder = imageComponent.transform.GetSiblingIndex();
            }
        }

        /// <summary>
        /// 칩을 목표 위치로 직선 이동시킵니다.
        /// </summary>
        /// <param name="targetPosition">목표 위치</param>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        public void MoveToTarget(Vector3 targetPosition, System.Action onComplete = null)
        {
            if (isAnimating) return;
            
            isAnimating = true;
            
            // 애니메이션 시작 시 레이어를 맨 위로 설정
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 999; // 맨 위 레이어
            }
            else if (imageComponent != null)
            {
                imageComponent.transform.SetAsLastSibling(); // UI에서 맨 위로
            }
            
            // 목표 위치에 랜덤 오프셋 추가
            Vector3 randomOffset = new Vector3(
                Random.Range(-randomOffsetRange, randomOffsetRange),
                Random.Range(-randomOffsetRange, randomOffsetRange),
                0f
            );
            Vector3 finalTarget = targetPosition + randomOffset;
            
            // 직선 이동 애니메이션
            transform.DOMove(finalTarget, moveDuration)
                .SetEase(moveEase)
                .OnComplete(() => {
                    // 이동 완료 후 페이드아웃
                    FadeOutAndStack(onComplete);
                });
        }
        
        /// <summary>
        /// 칩을 페이드아웃시키고 콜백을 호출합니다. (파괴하지 않음)
        /// </summary>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        public void FadeOutAndStack(System.Action onComplete = null)
        {
            if (spriteRenderer != null)
            {
                // 알파값을 0으로 페이드아웃
                spriteRenderer.DOFade(0f, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        isAnimating = false;
                        // 애니메이션 완료 시 원래 레이어로 복원
                        spriteRenderer.sortingOrder = originalSortingOrder;
                        // 칩을 비활성화하여 보이지 않게 함
                        gameObject.SetActive(false);
                        onComplete?.Invoke();
                    });
            }
            else if (imageComponent != null)
            {
                // Image 컴포넌트의 알파값을 0으로 페이드아웃
                imageComponent.DOFade(0f, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        isAnimating = false;
                        // 애니메이션 완료 시 원래 레이어로 복원
                        imageComponent.transform.SetSiblingIndex(originalSortingOrder);
                        // 칩을 비활성화하여 보이지 않게 함
                        gameObject.SetActive(false);
                        onComplete?.Invoke();
                    });
            }
            else
            {
                isAnimating = false;
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 칩을 페이드인(알파값 1로) 애니메이션
        /// </summary>
        public void FadeIn()
        {
            if (spriteRenderer != null)
            {
                // 칩을 활성화하고 알파값을 0으로 시작
                gameObject.SetActive(true);
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
                
                // 알파값을 1로 페이드인
                spriteRenderer.DOFade(1f, 0.3f)
                    .SetEase(Ease.OutQuad);
            }
            else if (imageComponent != null)
            {
                // 칩을 활성화하고 알파값을 0으로 시작
                gameObject.SetActive(true);
                imageComponent.color = new Color(imageComponent.color.r, imageComponent.color.g, imageComponent.color.b, 0f);
                
                // 알파값을 1로 페이드인
                imageComponent.DOFade(1f, 0.3f)
                    .SetEase(Ease.OutQuad);
            }
        }

        /// <summary>
        /// 칩을 페이드아웃 후 파괴합니다.
        /// </summary>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        public void FadeOutAndDestroy(System.Action onComplete = null)
        {
            if (spriteRenderer != null)
            {
                // 알파값을 0으로 페이드아웃
                spriteRenderer.DOFade(0f, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        isAnimating = false;
                        // 애니메이션 완료 시 원래 레이어로 복원
                        spriteRenderer.sortingOrder = originalSortingOrder;
                        // 칩을 파괴
                        Destroy(gameObject);
                        onComplete?.Invoke();
                    });
            }
            else if (imageComponent != null)
            {
                // Image 컴포넌트의 알파값을 0으로 페이드아웃
                imageComponent.DOFade(0f, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        isAnimating = false;
                        // 애니메이션 완료 시 원래 레이어로 복원
                        imageComponent.transform.SetSiblingIndex(originalSortingOrder);
                        // 칩을 파괴
                        Destroy(gameObject);
                        onComplete?.Invoke();
                    });
            }
            else
            {
                isAnimating = false;
                Destroy(gameObject);
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 칩을 아바타 위치에서 칩 쌓이는 위치로 이동시킵니다.
        /// </summary>
        /// <param name="avatarPosition">아바타 위치</param>
        /// <param name="chipStackPosition">칩 쌓이는 위치</param>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        public void MoveFromAvatarToStack(Vector3 avatarPosition, Vector3 chipStackPosition, System.Action onComplete = null)
        {
            startPosition = avatarPosition;
            transform.position = avatarPosition;
            
            // 아바타 위치의 레이어 정보를 가져와서 칩의 원래 레이어로 설정
            SetAvatarLayerOrder(avatarPosition);
            
            MoveToTarget(chipStackPosition, onComplete);
        }

        /// <summary>
        /// 칩을 스택 위치에서 슬롯 위치로 이동시킵니다.
        /// </summary>
        /// <param name="stackPosition">스택 위치</param>
        /// <param name="slotPosition">슬롯 위치</param>
        /// <param name="onComplete">완료 시 호출할 콜백</param>
        public void MoveFromStackToSlot(Vector3 stackPosition, Vector3 slotPosition, System.Action onComplete = null)
        {
            startPosition = stackPosition;
            transform.position = stackPosition;
            
            // 스택 위치의 레이어 정보를 가져와서 칩의 원래 레이어로 설정
            SetAvatarLayerOrder(stackPosition);
            
            MoveToTarget(slotPosition, () => {
                // 이동 완료 후 페이드아웃 후 파괴
                FadeOutAndDestroy(onComplete);
            });
        }
        
        /// <summary>
        /// 아바타 위치의 레이어 순서를 가져와서 칩의 원래 레이어로 설정합니다.
        /// </summary>
        /// <param name="avatarPosition">아바타 위치</param>
        private void SetAvatarLayerOrder(Vector3 avatarPosition)
        {
            // 아바타 위치에서 가장 가까운 SpriteRenderer를 찾아서 레이어 순서를 가져옴
            Collider2D[] colliders = Physics2D.OverlapPointAll(avatarPosition);
            foreach (var collider in colliders)
            {
                SpriteRenderer avatarSprite = collider.GetComponent<SpriteRenderer>();
                if (avatarSprite != null)
                {
                    originalSortingOrder = avatarSprite.sortingOrder;
                    break;
                }
            }
        }

        /// <summary>
        /// 시드 칩 효과를 적용합니다.
        /// </summary>
        /// <param name="chipType">칩 타입 (실버 또는 골드)</param>
        public void ApplySeedChipEffect(ChipType chipType)
        {
            if (spriteRenderer != null)
            {
                // 칩 타입에 따른 색상 선택
                Color effectColor = chipType == ChipType.Silver ? silverSeedChipColor : goldSeedChipColor;
                
                // 색상 변경
                spriteRenderer.color = effectColor;
                
                // 크기 변경
                transform.localScale = originalScale * seedChipScale;
                
                // 글로우 효과 (색상 페이드)
                spriteRenderer.DOColor(originalColor, seedChipGlowDuration)
                    .SetEase(Ease.OutQuad);
                
                // 크기 복원
                transform.DOScale(originalScale, seedChipGlowDuration)
                    .SetEase(Ease.OutQuad);
            }
            else if (imageComponent != null)
            {
                // 칩 타입에 따른 색상 선택
                Color effectColor = chipType == ChipType.Silver ? silverSeedChipColor : goldSeedChipColor;
                
                // 색상 변경
                imageComponent.color = effectColor;
                
                // 크기 변경
                transform.localScale = originalScale * seedChipScale;
                
                // 글로우 효과 (색상 페이드)
                imageComponent.DOColor(originalColor, seedChipGlowDuration)
                    .SetEase(Ease.OutQuad);
                
                // 크기 복원
                transform.DOScale(originalScale, seedChipGlowDuration)
                    .SetEase(Ease.OutQuad);
            }
        }

        /// <summary>
        /// 애니메이션을 즉시 중단합니다.
        /// </summary>
        public void StopAnimation()
        {
            if (isAnimating)
            {
                DOTween.Kill(transform);
                isAnimating = false;
            }
        }

        /// <summary>
        /// 칩을 원래 위치로 되돌립니다.
        /// </summary>
        public void ResetToStart()
        {
            StopAnimation();
            transform.position = startPosition;
        }

        private void OnDestroy()
        {
            // 컴포넌트가 파괴될 때 애니메이션 정리
            DOTween.Kill(transform);
        }
    }
} 