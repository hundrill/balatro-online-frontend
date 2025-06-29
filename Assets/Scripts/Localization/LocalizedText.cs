using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BalatroOnline.Localization
{
    public class LocalizedText : MonoBehaviour
    {
        public string key;

        void Start()
        {
            string localized = LocalizationManager.GetText(key);
            var text = GetComponent<Text>();
            if (text != null) text.text = localized;
            var tmp = GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = localized;
        }
    }
} 