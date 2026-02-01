using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpritesData", menuName = "Scriptable Objects/EnemySpritesData")]
public class EnemySpritesData : ScriptableObject
{
    public List<Sprite> GreenSprites;
    public List<Sprite> RedSprites;
    public List<Sprite> BlueSprites;
    public float frameRate = 15f;

    public Sprite GetSprite(MaskType colorType, float time)
    {
        switch (colorType)
        {
            case MaskType.Green:
                int greenIndex = (int)(time * frameRate) % GreenSprites.Count;
                return GreenSprites[greenIndex];
            case MaskType.Red:
                int redIndex = (int)(time * frameRate) % RedSprites.Count;
                return RedSprites[redIndex];
            case MaskType.Blue:
                int blueIndex = (int)(time * frameRate) % BlueSprites.Count;
                return BlueSprites[blueIndex];
        }
        Debug.LogError("Invalid ColorType: " + colorType);
        return null;
    }
}
