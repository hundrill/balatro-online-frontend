using UnityEngine;

public class ServerConfig : MonoBehaviour
{
    public static ServerConfig Instance { get; private set; }

    [Header("서버 주소 (포트 포함)")]
    public string serverHost = "localhost";
    public int serverPort = 3000;

    public string GetHttpUrl()
    {
#if UNITY_EDITOR
        return $"http://localhost:{serverPort}";
#elif UNITY_ANDROID
        return $"http://10.0.2.2:{serverPort}";
#else
        return $"http://{serverHost}:{serverPort}";
#endif
    }

    public string GetWsUrl()
    {
#if UNITY_EDITOR
        return $"ws://localhost:{serverPort}";
#elif UNITY_ANDROID
        return $"ws://10.0.2.2:{serverPort}";
#else
        return $"ws://{serverHost}:{serverPort}";
#endif
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
} 