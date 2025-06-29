using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using BalatroOnline.Localization;

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

        public void Show(string message, Action onOk = null)
        {
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

            if (messageText == null)
                messageText = dialogInstance.GetComponentInChildren<TextMeshProUGUI>(true);
            if (okButton == null)
            {
                var buttons = dialogInstance.GetComponentsInChildren<Button>(true);
                if (buttons.Length > 0) okButton = buttons[0];
            }
            if (messageText == null)
                messageText = dialogInstance.GetComponentInChildren<TextMeshProUGUI>(true);

            if (messageText != null) messageText.text = LocalizationManager.GetText(message);
            onOkCallback = onOk;
            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(OnOkClicked);
            }
        }

        public void Hide()
        {
            if (dialogInstance != null)
                dialogInstance.SetActive(false);
        }

        private void OnOkClicked()
        {
            Hide();
            onOkCallback?.Invoke();
        }
    }
} 