using UnityEngine;

public class TestConfig : MonoBehaviour
{
    public static TestConfig Instance { get; private set; }

    [Header("테스트 모드 (배포 전 false로!)")]
    public bool isTestMode = true;

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