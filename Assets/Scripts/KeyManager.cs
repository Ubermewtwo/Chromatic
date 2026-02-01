using UnityEngine;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    public bool HasGreenKey = false;
    public bool HasRedKey = false;
    public bool HasBlueKey = false;

    public void Awake()
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

    public void ObtainKey(MaskType type)
    {
        switch (type)
        {
            case MaskType.Green:
                HasGreenKey = true;
                break;
            case MaskType.Red:
                HasRedKey = true;
                break;
            case MaskType.Blue:
                HasBlueKey = true;
                break;
        }
    }
}
