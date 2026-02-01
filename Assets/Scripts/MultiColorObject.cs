using Chromatic;
using System.Collections;
using UnityEngine;

public class MultiColorObject : MonoBehaviour
{
    public GameObject greenBehaviorObject;
    public GameObject redBehaviorObject;
    public GameObject blueBehaviorObject;

    //public SpriteRenderer greenSpriteRenderer;
    //public SpriteRenderer redSpriteRenderer;
    //public SpriteRenderer blueSpriteRenderer;

    public bool debugMoveUp = false;

    private void OnValidate()
    {
        if (debugMoveUp)
        {
            debugMoveUp = false;
            MoveUp(2f, 2f);
        }
    }


    private void Start()
    {
        MaskTransitionBehaviour.Instance.OnMaskTransition.AddListener(HandleMaskTransition);
    }

    private void OnDisable()
    {
        MaskTransitionBehaviour.Instance.OnMaskTransition.RemoveListener(HandleMaskTransition);
    }

    private void HandleMaskTransition(MaskType maskType)
    {
        //Debug.Log("MultiColorObject - HandleMaskTransition: " + maskType.ToString());

        switch (maskType)
        {
            case MaskType.Green:
                greenBehaviorObject.SetActive(true);
                redBehaviorObject.SetActive(false);
                blueBehaviorObject.SetActive(false);
                break;
            case MaskType.Red:
                greenBehaviorObject.SetActive(false);
                redBehaviorObject.SetActive(true);
                blueBehaviorObject.SetActive(false);
                break;
            case MaskType.Blue:
                greenBehaviorObject.SetActive(false);
                redBehaviorObject.SetActive(false);
                blueBehaviorObject.SetActive(true);
                break;
        }
    }

    public void MoveUp(float speed, float distance)
    {
        StopAllCoroutines();
        StartCoroutine(MoveUpCoroutine(speed, distance));
    }

    private IEnumerator MoveUpCoroutine(float speed, float distance)
    {
        float originalYScale = transform.localScale.y;

        while (transform.localScale.y - originalYScale < distance)
        {
            transform.localScale += Vector3.up * speed * Time.deltaTime;
            yield return null;
        }
    }
}
