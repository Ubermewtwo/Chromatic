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
    public Sprite GreenAttackSprite;
    public List<Sprite> RedAttackTransformationSprites;
    public float RedAttackTransformationFrameRate;
    public List<Sprite> BlueAttackSprites;
    public float BlueAttackFrameRate;

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
            case PlayerState.GreenAttack:
                return GreenAttackSprite;
            case PlayerState.RedAttack:
                int redAttackIndex = (int)(time * RedAttackTransformationFrameRate);
                if (redAttackIndex >= RedAttackTransformationSprites.Count)
                    redAttackIndex = RedAttackTransformationSprites.Count - 1;
                return RedAttackTransformationSprites[redAttackIndex];
            case PlayerState.BlueAttack:
                int blueAttackIndex = (int)(time * BlueAttackFrameRate) % BlueAttackSprites.Count;
                return BlueAttackSprites[blueAttackIndex];
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
        Grab,
        GreenAttack,
        RedAttack,
        BlueAttack
    }
}
