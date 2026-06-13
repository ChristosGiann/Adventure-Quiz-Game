using UnityEngine;

public class PersistentScore : MonoBehaviour
{
    public static PersistentScore Instance;

    public int finalScore;

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

    public void Clear()
    {
        Destroy(gameObject);
    }
}
