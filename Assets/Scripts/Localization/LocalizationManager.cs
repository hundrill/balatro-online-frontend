using System.Collections.Generic;
using UnityEngine;

namespace BalatroOnline.Localization
{
    public static class LocalizationManager
    {
        static Dictionary<string, string> table;
        static string currentLang = "ko";
        private static List<LocalizedText> observers = new List<LocalizedText>();

        public static void Register(LocalizedText lt)
        {
            if (!observers.Contains(lt))
                observers.Add(lt);
        }
        public static void Unregister(LocalizedText lt)
        {
            observers.Remove(lt);
        }
        public static void NotifyAll()
        {
            foreach (var lt in observers)
                if (lt != null) lt.Refresh();
        }

        public static void Load(string lang)
        {
            currentLang = lang;
            TextAsset json = Resources.Load<TextAsset>($"Localization/{lang}");
            if (json != null)
                table = JsonUtility.FromJson<LocalizationTable>(json.text).ToDict();
            else
                table = new Dictionary<string, string>();
            NotifyAll(); // 언어 변경 시 자동 갱신
        }

        public static string GetText(string key)
        {
            if (table == null) Load(currentLang);
            return table != null && table.TryGetValue(key, out var value) ? value : key;
        }

        [System.Serializable]
        public class LocalizationTable
        {
            public List<Entry> entries;
            public Dictionary<string, string> ToDict()
            {
                var dict = new Dictionary<string, string>();
                foreach (var e in entries) dict[e.key] = e.value;
                return dict;
            }
        }
        [System.Serializable]
        public class Entry { public string key, value; }
    }
}