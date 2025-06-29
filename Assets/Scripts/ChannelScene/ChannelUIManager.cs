using UnityEngine;
using UnityEngine.UI;
using BalatroOnline.Common;

namespace BalatroOnline.Channel
{
    /// <summary>
    /// 로비 씬 전용 UI를 관리하는 매니저
    /// </summary>
    public class ChannelUIManager : MonoBehaviour
    {
        public static ChannelUIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // TODO: 로비 UI 관리 (버튼, 리스트 등)

        public void OnClickChannel()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }        

        public void OnClickBack()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
        }               
    }
} 