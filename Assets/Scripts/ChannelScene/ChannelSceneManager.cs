using UnityEngine;
using UnityEngine.SceneManagement;
using BalatroOnline.Common;

namespace BalatroOnline.Channel
{
    /// <summary>
    /// 로비 씬의 전체 흐름(씬 전환 등)을 관리하는 매니저
    /// </summary>
    public class ChannelSceneManager : MonoBehaviour
    {
        public static ChannelSceneManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void OnClickEnterGame()
        {
            // TODO: 게임 씬으로 전환
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
} 