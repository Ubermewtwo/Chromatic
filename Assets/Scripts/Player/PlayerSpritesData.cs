using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSpritesData", menuName = "Scriptable Objects/PlayerSpritesData")]
public class PlayerSpritesData : ScriptableObject
{
    public List<Sprite> IdleSprites;
    public float IdleFrameRate;
    public List<Sprite> WalkSprites;
    public float WalkFrameRate;
    public List<Sprite> JumpSprites;
    public float jumpFrameRate;
    public Sprite FallSprite;
    public Sprite GrabSprite;
    public List<Sprite> GreenAttackSprites;
    public float greenAttackFrameRate;
    public List<Sprite> RedAttackTransformationSprites;
    public float RedAttackTransformationFrameRate;
    public List<Sprite> BlueAttackSprites;
    public float BlueAttackFrameRate;

    public Sprite GetSprite(PlayerState state, float time, MaskType maskType)
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
                int jumpIndex = Mathf.Clamp((int)(time * jumpFrameRate) / JumpSprites.Count, 0, JumpSprites.Count - 1);
                return JumpSprites[jumpIndex];
            case PlayerState.Fall:
                return FallSprite;
            case PlayerState.Grab:
                return GrabSprite;
            case PlayerState.Attack:
                switch (maskType)
                {
                    case MaskType.Green:
                        int greenAttackIndex = (int)(time * greenAttackFrameRate);
                        if (greenAttackIndex >= GreenAttackSprites.Count)
                            greenAttackIndex = GreenAttackSprites.Count - 1;
                        return GreenAttackSprites[greenAttackIndex];
                    case MaskType.Red:
                        int redAttackIndex = (int)(time * RedAttackTransformationFrameRate);
                        if (redAttackIndex >= RedAttackTransformationSprites.Count)
                            redAttackIndex = RedAttackTransformationSprites.Count - 1;
                        return RedAttackTransformationSprites[redAttackIndex];
                    case MaskType.Blue:
                        int blueAttackIndex = (int)(time * BlueAttackFrameRate);
                        if (blueAttackIndex >= BlueAttackSprites.Count)
                            blueAttackIndex = BlueAttackSprites.Count - 1;
                        return BlueAttackSprites[blueAttackIndex];
                }
                break;
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
        Attack,
        Climb
    }
}
