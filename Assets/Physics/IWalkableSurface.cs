using UnityEngine;

public interface IWalkableSurface
{
    public float Slipperiness { get; }
    public float MaxSpeed { get; }
}
