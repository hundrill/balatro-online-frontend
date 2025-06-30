using UnityEngine;
using UnityEngine.UI;

namespace BalatroOnline.Game
{
    public class Card : MonoBehaviour
    {
        [Header("카드 이미지 (자기 자신 Image 컴포넌트)")]
        public Image cardImage;

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
    }
} 