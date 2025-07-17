using UnityEngine;
using BalatroOnline.Network;

namespace BalatroOnline.Login
{
    /// <summary>
    /// 로그인 씬의 전체 흐름을 관리하는 매니저
    /// </summary>
    public class LoginSceneManager : MonoBehaviour
    {
        public static LoginSceneManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {

        }

    }
}