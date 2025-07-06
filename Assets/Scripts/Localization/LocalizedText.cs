using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalatroOnline.Localization
{
    public class LocalizedText : MonoBehaviour
    {
        public string key;

        void OnEnable()
        {
            LocalizationManager.Register(this);
            Refresh();
        }
        void OnDisable()
        {
            LocalizationManager.Unregister(this);
        }

        void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            string localized = LocalizationManager.GetText(key);
            var text = GetComponent<Text>();
            if (text != null) text.text = localized;
            var tmp = GetComponent<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = localized;
        }
    }
} 