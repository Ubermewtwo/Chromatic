using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSpritesData", menuName = "Scriptable Objects/PlayerSpritesData")]
public class PlayerSpritesData : ScriptableObject
{
    public List<Sprite> IdleSprites;
    public float IdleFrameRate;
    public List<Sprite> WalkSprites;
    public float WalkFrameRate;
    public Sprite JumpSprite;
    public Sprite FallSprite;
    public Sprite GrabSprite;

    public Sprite GetSprite(PlayerState state, float time)
    {
        switch (state)
        {
            case PlayerState.Idle:
                int idleIndex = (int)(time * IdleFrameRate) % IdleSprites.Count;
                return IdleSprites[idleIndex];
            case PlayerState.Walk:
                int walkIndex = (int)(time * WalkFrameRate) % WalkSprites.Count;
                return WalkSprites[walkIndex];
            case PlayerState.Jump:
                return JumpSprite;
            case PlayerState.Fall:
                return FallSprite;
            case PlayerState.Grab:
                return GrabSprite;
        }

        Debug.LogError("Invalid PlayerState: " + state);
        return null;
    }

    public enum PlayerState
    {
        Idle,
        Walk,
        Jump,
        Fall,
        Grab
    }
}
