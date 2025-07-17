using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using BalatroOnline.Localization;
using System.Collections;

namespace BalatroOnline.Common
{
    public class MessageDialogManager : MonoBehaviour
    {
        public static MessageDialogManager Instance;
        public GameObject dialogPrefab;
        private GameObject dialogInstance;
        private TextMeshProUGUI messageText;
        private Button okButton;
        private Action onOkCallback;
        private TextMeshProUGUI timeoutText;
        private float timeoutRemaining;
        private bool isTimeoutMode;
        private CanvasGroup canvasGroup;
        private Coroutine fadeOutCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Show(string message, Action onOk = null, float timeoutSeconds = 0)
        {
            // 이전 페이드아웃 코루틴이 실행 중이면 취소
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }

            if (dialogInstance == null)
            {
                var canvas = FindFirstObjectByType<Canvas>();
                dialogInstance = Instantiate(dialogPrefab, canvas.transform);
                dialogInstance.SetActive(true);
            }
            else
            {
                dialogInstance.SetActive(true);
            }

            // CanvasGroup 연결 또는 추가
            if (canvasGroup == null)
            {
                canvasGroup = dialogInstance.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = dialogInstance.AddComponent<CanvasGroup>();
            }
            // 즉시 표시 (페이드인 없이 바로)
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (messageText == null)
                messageText = dialogInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (okButton == null)
            {
                var buttons = dialogInstance.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                if (buttons.Length > 0) okButton = buttons[0];
            }
            if (messageText == null)
                messageText = dialogInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);

            // Timeout 텍스트 찾기
            if (timeoutText == null)
            {
                var tmps = dialogInstance.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var tmp in tmps)
                {
                    if (tmp.gameObject.name.ToLower().Contains("timeout"))
                    {
                        timeoutText = tmp;
                        break;
                    }
                }
            }

            if (messageText != null) messageText.text = LocalizationManager.GetText(message);
            onOkCallback = onOk;

            if (timeoutSeconds > 0)
            {
                isTimeoutMode = true;
                timeoutRemaining = timeoutSeconds;
                if (okButton != null) okButton.gameObject.SetActive(false);
                if (timeoutText != null) timeoutText.gameObject.SetActive(true);
            }
            else
            {
                isTimeoutMode = false;
                if (okButton != null)
                {
                    okButton.gameObject.SetActive(true);
                    okButton.onClick.RemoveAllListeners();
                    okButton.onClick.AddListener(OnOkClicked);
                }
                if (timeoutText != null) timeoutText.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            if (dialogInstance != null)
                fadeOutCoroutine = StartCoroutine(FadeOut());
            isTimeoutMode = false;
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 4f; // 0.25초 정도
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null || dialogInstance == null) yield break;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 4f;
                // 파괴 체크
                if (canvasGroup == null || dialogInstance == null || canvasGroup.Equals(null) || dialogInstance.Equals(null))
                    yield break;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            if (canvasGroup != null && dialogInstance != null && !canvasGroup.Equals(null) && !dialogInstance.Equals(null))
            {
                canvasGroup.alpha = 0f;
                dialogInstance.SetActive(false);
            }
            fadeOutCoroutine = null;
        }

        private void OnOkClicked()
        {
            Hide();
            onOkCallback?.Invoke();
        }

        private void Update()
        {
            if (isTimeoutMode && dialogInstance != null && dialogInstance.activeSelf)
            {
                timeoutRemaining -= Time.unscaledDeltaTime;
                if (timeoutText != null)
                {
                    int seconds = Mathf.CeilToInt(timeoutRemaining);
                    timeoutText.text = $"{seconds}";
                }
                if (timeoutRemaining <= 0f)
                {
                    Hide();
                    onOkCallback?.Invoke();
                }
            }
        }

        public bool IsDialogActive()
        {
            return dialogInstance != null && dialogInstance.activeSelf;
        }

        public IEnumerator ShowAndWait(string message, Action onOk = null, float timeoutSeconds = 0, float preDelay = 0f, float postDelay = 0f)
        {
            if (TestConfig.Instance.isTestMode){
                if (timeoutSeconds > 0.5f) timeoutSeconds = 0.5f;
                if (preDelay > 0.5f) preDelay = 0.5f;
                if (postDelay > 0.5f) postDelay = 0.5f;
            }

            if (preDelay > 0f)
                yield return new UnityEngine.WaitForSeconds(preDelay);
            Show(message, onOk, timeoutSeconds);
            while (IsDialogActive())
                yield return null;
            if (postDelay > 0f)
                yield return new UnityEngine.WaitForSeconds(postDelay);
        }
    }
} 