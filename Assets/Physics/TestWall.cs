using UnityEngine;

public class TestWall : MonoBehaviour, IGrabbableSurface
{
    [field: SerializeField] public bool Grabbable { get; private set; } = true;
}
