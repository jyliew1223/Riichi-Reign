using UnityEngine;

public class DebugTimeControl : MonoBehaviour
{
    [SerializeField]
    public float m_turnTimeLoop = 1f;

    public static DebugTimeControl Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Update is called once per frame
    void Update() { }
}
