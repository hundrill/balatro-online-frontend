using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BalatroOnline.Common
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TweenAlpha : MonoBehaviour
    {
        public float from = 1f;
        public float to = 0f;
        public float duration = 1f;
        public float startDelay = 0f;
        public bool destroyOnComplete = false;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = from;
        }

        private void Start()
        {
            if (!enabled) return;
            StartCoroutine(TweenRoutine());
        }

        private IEnumerator TweenRoutine()
        {
            if (startDelay > 0f)
                yield return new WaitForSeconds(startDelay);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = to;
            if (destroyOnComplete)
                Destroy(gameObject);
        }
    }
} 