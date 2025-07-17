using UnityEngine;
using System.Collections.Generic;
using BalatroOnline.Game;

namespace BalatroOnline.Game
{
    /// <summary>
    /// 칩 애니메이션을 관리하는 매니저
    /// </summary>
    public class ChipAnimationManager : MonoBehaviour
    {
        public static ChipAnimationManager Instance { get; private set; }

        [Header("Chip Settings")]
        public GameObject chipPrefab;
        public Transform chipStackPosition;

        [Header("Chip Stack Settings")]
        public float chipStackSpacing = 0.1f; // 칩 간격
        public int maxChipsInStack = 20; // 스택당 최대 칩 개수

        [Header("Slot References")]
        public MySlot mySlot;
        public OpponentSlot[] opponentSlots = new OpponentSlot[3];

        [Header("Animation Settings")]
        public int maxConcurrentChips = 5;
        public float delayBetweenChips = 0.1f;

        private Queue<ChipController> chipPool = new Queue<ChipController>();
        private List<ChipController> activeChips = new List<ChipController>();
        private List<ChipController> landedChips = new List<ChipController>(); // 도착한 칩들을 저장
        private List<ChipController> stackedChips = new List<ChipController>(); // 스택에 쌓인 칩들

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 시드 칩을 아바타에서 칩 스택으로 이동시킵니다 (라운드 시작 시).
        /// </summary>
        /// <param name="userId">칩을 차감할 사용자 ID</param>
        /// <param name="silverSeedChipAmount">실버 시드 칩 수량</param>
        /// <param name="goldSeedChipAmount">골드 시드 칩 수량</param>
        /// <param name="onComplete">모든 칩 이동 완료 시 호출할 콜백</param>
        public void DeductSeedChip(string userId, int silverSeedChipAmount, int goldSeedChipAmount, System.Action onComplete = null)
        {
            Debug.Log($"[ChipAnimationManager] 시드 시작: 사용자 {userId}, 실버 {silverSeedChipAmount}개, 골드 {goldSeedChipAmount}개");

            // 사용자 ID로 슬롯 찾기
            Transform targetAvatarPosition = FindAvatarPositionByUserId(userId);
            if (targetAvatarPosition != null)
            {

                // 시드 칩 차감 시 시각적 효과 (예: 화면 깜빡임, 사운드 등)
                StartCoroutine(DeductSeedChipCoroutine(targetAvatarPosition, silverSeedChipAmount, goldSeedChipAmount, onComplete));
            }
        }

        private System.Collections.IEnumerator DeductSeedChipCoroutine(Transform avatarPosition, int silverSeedChipAmount, int goldSeedChipAmount, System.Action onComplete)
        {
            // 시드 칩 차감 시작 메시지 표시 (선택사항)
            // MessageDialogManager.Instance.Show($"시드 칩 {silverSeedChipAmount + goldSeedChipAmount}개를 차감합니다...", null, 1f);

            yield return new WaitForSeconds(0.5f); // 잠시 대기

            // 칩 이동 애니메이션 실행 (시드 칩 차감 플래그 설정)
            StartCoroutine(MoveChipsCoroutineWithSeedEffect(avatarPosition, silverSeedChipAmount, goldSeedChipAmount, onComplete));
        }

        private System.Collections.IEnumerator MoveChipsCoroutineWithSeedEffect(Transform avatarPosition, int silverChipAmount, int goldChipAmount, System.Action onComplete)
        {
            int chipsMoved = 0;
            int totalChips = silverChipAmount + goldChipAmount;

            // 실버 칩 먼저 처리
            for (int i = 0; i < silverChipAmount; i++)
            {
                // 동시에 움직이는 칩 수 제한
                while (activeChips.Count >= maxConcurrentChips)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                // 칩 생성 또는 풀에서 가져오기
                ChipController chip = GetChipFromPool();
                if (chip != null)
                {
                    activeChips.Add(chip);

                    // 실버 시드 칩 효과 적용
                    chip.ApplySeedChipEffect(ChipType.Silver);

                    // 칩 이동 시작
                    chip.MoveFromAvatarToStack(
                        avatarPosition.position,
                        chipStackPosition.position,
                        () =>
                        {
                            Debug.Log($"[ChipAnimationManager] 실버 시드 칩 이동 완료: 칩 {chip.gameObject.name}");
                            // 이동 완료 시 활성 칩 목록에서 제거
                            activeChips.Remove(chip);
                            // 칩을 페이드아웃 후 스택에 추가 및 페이드인
                            chip.FadeOutAndStack(() =>
                            {

                                Debug.Log($"[ChipAnimationManager] 실버 시드 칩 이동 완료 1111: 칩 {chip.gameObject.name}");

                                AddChipToStack(chip);
                                chip.FadeIn();
                            });
                            chipsMoved++;
                            // 모든 칩 이동 완료 시 콜백 호출
                            if (chipsMoved >= totalChips)
                            {
                                onComplete?.Invoke();
                            }
                        }
                    );
                }

                yield return new WaitForSeconds(delayBetweenChips);
            }

            // 골드 칩 처리
            for (int i = 0; i < goldChipAmount; i++)
            {
                // 동시에 움직이는 칩 수 제한
                while (activeChips.Count >= maxConcurrentChips)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                // 칩 생성 또는 풀에서 가져오기
                ChipController chip = GetChipFromPool();
                if (chip != null)
                {
                    activeChips.Add(chip);

                    // 골드 시드 칩 효과 적용
                    chip.ApplySeedChipEffect(ChipType.Gold);

                    // 칩 이동 시작
                    chip.MoveFromAvatarToStack(
                        avatarPosition.position,
                        chipStackPosition.position,
                        () =>
                        {
                            // 이동 완료 시 활성 칩 목록에서 제거
                            activeChips.Remove(chip);
                            // 칩을 페이드아웃 후 스택에 추가 및 페이드인
                            chip.FadeOutAndStack(() =>
                            {
                                AddChipToStack(chip);
                                chip.FadeIn();
                            });
                            chipsMoved++;

                            // 모든 칩 이동 완료 시 콜백 호출
                            if (chipsMoved >= totalChips)
                            {
                                onComplete?.Invoke();
                            }
                        }
                    );
                }

                yield return new WaitForSeconds(delayBetweenChips);
            }
        }

        // /// <summary>
        // /// 칩을 아바타에서 칩 스택으로 이동시킵니다.
        // /// </summary>
        // /// <param name="chipAmount">이동할 칩 수량</param>
        // /// <param name="onComplete">모든 칩 이동 완료 시 호출할 콜백</param>
        // public void MoveChipsFromAvatarToStack(int chipAmount, System.Action onComplete = null)
        // {
        //     StartCoroutine(MoveChipsCoroutine(chipAmount, onComplete));
        // }

        // private System.Collections.IEnumerator MoveChipsCoroutine(int chipAmount, System.Action onComplete)
        // {
        //     int chipsMoved = 0;

        //     for (int i = 0; i < chipAmount; i++)
        //     {
        //         // 동시에 움직이는 칩 수 제한
        //         while (activeChips.Count >= maxConcurrentChips)
        //         {
        //             yield return new WaitForSeconds(0.1f);
        //         }

        //         // 칩 생성 또는 풀에서 가져오기
        //         ChipController chip = GetChipFromPool();
        //         if (chip != null)
        //         {
        //             activeChips.Add(chip);

        //             // 칩 이동 시작
        //             chip.MoveFromAvatarToStack(
        //                 avatarPosition.position,
        //                 chipStackPosition.position,
        //                 () =>
        //                 {
        //                     // 이동 완료 시 활성 칩 목록에서 제거하고 도착한 칩 목록에 추가
        //                     activeChips.Remove(chip);
        //                     landedChips.Add(chip);
        //                     chipsMoved++;

        //                     // 모든 칩 이동 완료 시 콜백 호출
        //                     if (chipsMoved >= chipAmount)
        //                     {
        //                         onComplete?.Invoke();
        //                     }
        //                 }
        //             );
        //         }

        //         yield return new WaitForSeconds(delayBetweenChips);
        //     }
        //         }

        /// <summary>
        /// 칩 스택에서 특정 슬롯으로 칩을 이동시킵니다.
        /// </summary>
        /// <param name="userId">칩을 받을 사용자 ID</param>
        /// <param name="chipAmount">이동할 칩 수량</param>
        /// <param name="onComplete">모든 칩 이동 완료 시 호출할 콜백</param>
        public void MoveChipsFromStackToSlot(string userId, int chipAmount, System.Action onComplete = null)
        {
            Debug.Log($"[ChipAnimationManager] 칩 스택에서 슬롯으로 이동 시작: 사용자 {userId}, 칩 {chipAmount}개");

            // 사용자 ID로 슬롯 찾기
            Transform targetSlotPosition = FindAvatarPositionByUserId(userId);
            if (targetSlotPosition == null)
            {
                Debug.LogWarning($"[ChipAnimationManager] 사용자 {userId}의 슬롯 위치를 찾을 수 없습니다.");
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(MoveChipsFromStackToSlotCoroutine(targetSlotPosition, chipAmount, onComplete));
        }

        private System.Collections.IEnumerator MoveChipsFromStackToSlotCoroutine(Transform targetSlotPosition, int chipAmount, System.Action onComplete)
        {
            int chipsMoved = 0;

            // chipAmount가 0 이하이면 현재 스택에 있는 모든 칩을 가져옴
            int actualChipAmount = chipAmount;
            if (chipAmount <= 0)
            {
                actualChipAmount = stackedChips.Count;
                Debug.Log($"[ChipAnimationManager] 모든 칩 이동: {actualChipAmount}개");
            }

            for (int i = 0; i < actualChipAmount; i++)
            {
                // 동시에 움직이는 칩 수 제한
                while (activeChips.Count >= maxConcurrentChips)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                // 스택에서 칩 가져오기 (스택이 비어있으면 새로 생성)
                ChipController chip = GetChipFromStack();
                if (chip != null)
                {
                    activeChips.Add(chip);

                    // 칩 이동 시작 (스택에서 슬롯으로)
                    chip.MoveFromStackToSlot(
                        chipStackPosition.position,
                        targetSlotPosition.position,
                        () =>
                        {
                            // 이동 완료 시 활성 칩 목록에서 제거
                            activeChips.Remove(chip);
                            chipsMoved++;

                            // 모든 칩 이동 완료 시 콜백 호출
                            if (chipsMoved >= actualChipAmount)
                            {
                                onComplete?.Invoke();
                            }
                        }
                    );
                }

                yield return new WaitForSeconds(delayBetweenChips);
            }
        }

        /// <summary>
        /// 스택에서 칩을 가져옵니다. 스택이 비어있으면 새로 생성합니다.
        /// </summary>
        private ChipController GetChipFromStack()
        {
            // 스택에 칩이 있으면 가져오기
            if (stackedChips.Count > 0)
            {
                ChipController chip = stackedChips[stackedChips.Count - 1];
                stackedChips.RemoveAt(stackedChips.Count - 1);
                chip.gameObject.SetActive(true);
                return chip;
            }
            else
            {
                // 스택이 비어있으면 새로 생성
                return GetChipFromPool();
            }
        }

        /// <summary>
        /// 칩 풀에서 칩을 가져옵니다.
        /// </summary>
        private ChipController GetChipFromPool()
        {
            if (chipPool.Count > 0)
            {
                ChipController chip = chipPool.Dequeue();
                chip.gameObject.SetActive(true);
                return chip;
            }
            else if (chipPrefab != null)
            {
                // 새 칩 생성
                GameObject chipObj = Instantiate(chipPrefab, transform);
                ChipController chip = chipObj.GetComponent<ChipController>();
                if (chip == null)
                {
                    chip = chipObj.AddComponent<ChipController>();
                }
                return chip;
            }

            return null;
        }

        /// <summary>
        /// 칩을 풀로 반환합니다.
        /// </summary>
        private void ReturnChipToPool(ChipController chip)
        {
            if (chip != null)
            {
                chip.gameObject.SetActive(false);
                chipPool.Enqueue(chip);
            }
        }

        /// <summary>
        /// 사용자 ID로 아바타 위치를 찾습니다.
        /// </summary>
        /// <param name="userId">찾을 사용자 ID</param>
        /// <returns>아바타 위치 Transform, 찾지 못하면 null</returns>
        private Transform FindAvatarPositionByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            // MySlot에서 찾기
            if (mySlot != null && mySlot.GetUserId() == userId)
            {
                return mySlot.avatarPosition;
            }

            // OpponentSlot에서 찾기
            if (opponentSlots != null)
            {
                foreach (var slot in opponentSlots)
                {
                    if (slot != null && slot.GetUserId() == userId)
                    {
                        return slot.avatarPosition;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 칩을 스택에 추가합니다.
        /// </summary>
        /// <param name="chip">추가할 칩</param>
        private void AddChipToStack(ChipController chip)
        {
            if (chip == null || chipStackPosition == null) return;

            // 칩을 스택 위치의 자식으로 설정
            chip.transform.SetParent(chipStackPosition);

            // 스택에 쌓인 칩 개수에 따라 위치 계산
            int stackIndex = stackedChips.Count;
            Vector3 stackPosition = Vector3.up * (stackIndex * chipStackSpacing);

            // 칩을 스택 위치로 이동 (로컬 좌표 기준)
            chip.transform.localPosition = stackPosition;

            // 스택에 추가
            stackedChips.Add(chip);

            Debug.Log($"[ChipAnimationManager] 칩을 스택에 추가: 인덱스 {stackIndex}, 위치 {stackPosition}, 총 칩 수 {stackedChips.Count}");

            // 최대 개수 초과 시 오래된 칩 제거
            if (stackedChips.Count > maxChipsInStack)
            {
                var oldChip = stackedChips[0];
                stackedChips.RemoveAt(0);
                if (oldChip != null)
                {
                    Destroy(oldChip.gameObject);
                }

                // 나머지 칩들의 위치 재조정
                RearrangeChipStack();
            }
        }

        /// <summary>
        /// 칩 스택을 재정렬합니다.
        /// </summary>
        private void RearrangeChipStack()
        {
            for (int i = 0; i < stackedChips.Count; i++)
            {
                if (stackedChips[i] != null)
                {
                    Vector3 newPosition = Vector3.up * (i * chipStackSpacing);
                    stackedChips[i].transform.localPosition = newPosition;
                }
            }
        }

        /// <summary>
        /// 칩 스택을 클리어합니다.
        /// </summary>
        public void ClearChipStack()
        {
            foreach (var chip in stackedChips)
            {
                if (chip != null)
                {
                    Destroy(chip.gameObject);
                }
            }
            stackedChips.Clear();
        }

        /// <summary>
        /// 모든 활성 칩 애니메이션을 중단합니다.
        /// </summary>
        public void StopAllChipAnimations()
        {
            foreach (var chip in activeChips)
            {
                if (chip != null)
                {
                    chip.StopAnimation();
                }
            }
            activeChips.Clear();
        }

        /// <summary>
        /// 도착한 모든 칩들을 제거합니다.
        /// </summary>
        public void ClearLandedChips()
        {
            foreach (var chip in landedChips)
            {
                if (chip != null)
                {
                    Destroy(chip.gameObject);
                }
            }
            landedChips.Clear();
        }

        /// <summary>
        /// 칩 풀을 초기화합니다.
        /// </summary>
        public void ClearChipPool()
        {
            while (chipPool.Count > 0)
            {
                ChipController chip = chipPool.Dequeue();
                if (chip != null)
                {
                    Destroy(chip.gameObject);
                }
            }
        }

        /// <summary>
        /// 모든 칩을 제거합니다 (활성 + 도착한 칩).
        /// </summary>
        public void ClearAllChips()
        {
            StopAllChipAnimations();
            ClearLandedChips();
            ClearChipPool();
        }

        private void OnDestroy()
        {
            ClearAllChips();
        }
    }
}