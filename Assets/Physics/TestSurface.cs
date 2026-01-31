using UnityEngine;

public class TestSurface : MonoBehaviour, IWalkableSurface
{
    [field: SerializeField] public float Slipperiness { get; private set; } = 50f;

    [field: SerializeField] public float MaxSpeed { get; private set; } = 5f;
}
