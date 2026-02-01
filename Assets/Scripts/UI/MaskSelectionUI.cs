using UnityEngine;
using UnityEngine.UI;

public class MaskSelectionUI : MonoBehaviour
{
    public Sprite greenSprite;
    public Sprite redSprite;
    public Sprite blueSprite;

    public PlayerController playerController;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = greenSprite; // Default to green mask
    }

    private void Start()
    {
        //playerController.OnMaskChanged += UpdateMaskUI;
    }

    private void UpdateMaskUI(MaskType maskType)
    {
        switch (maskType)
        {
            case MaskType.Green:
                image.sprite = greenSprite;
                break;
            case MaskType.Red:
                image.sprite = redSprite;
                break;
            case MaskType.Blue:
                image.sprite = blueSprite;
                break;
        }
    }
}
