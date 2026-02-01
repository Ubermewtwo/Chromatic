using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskSelectionUI : MonoBehaviour
{
    public Sprite greenSprite;
    public Sprite redSprite;
    public Sprite blueSprite;

    public List<Sprite> greenMaskSprites;
    public List<Sprite> redMaskSprites;
    public List<Sprite> blueMaskSprites;

    public Image maskImage;

    private MaskType currentMaskType;

    public PlayerController playerController;
    public PlayerHealth playerHealth;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = greenSprite; // Default to green mask
        currentMaskType = MaskType.Green;
    }

    private void Start()
    {
        playerController.OnMaskChanged.AddListener(UpdateMaskUI);
        playerHealth.OnHealthChanged.AddListener(_ => UpdateMaskUI(currentMaskType));
    }

    private void OnDisable()
    {
        playerController.OnMaskChanged.RemoveListener(UpdateMaskUI);
        playerHealth.OnHealthChanged.RemoveListener(_ => UpdateMaskUI(currentMaskType));
    }

    private void UpdateMaskUI(MaskType maskType)
    {
        currentMaskType = maskType;
        int maskIndex = 3 - playerHealth.currentHealth;

        switch (currentMaskType)
        {
            case MaskType.Green:
                image.sprite = greenSprite;
                maskImage.sprite = greenMaskSprites[maskIndex];
                break;
            case MaskType.Red:
                image.sprite = redSprite;
                maskImage.sprite = redMaskSprites[maskIndex];
                break;
            case MaskType.Blue:
                image.sprite = blueSprite;
                maskImage.sprite = blueMaskSprites[maskIndex];
                break;
        }
    }
}
