using UnityEngine;

public class SpecialPlatform : MonoBehaviour
{
    public float respawnDelay = 5f;
    private float respawnTimer = 0f;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (!boxCollider.enabled)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                boxCollider.enabled = true;
            }
        }
    }

    public void Break()
    {
        boxCollider.enabled = false;
        respawnTimer = respawnDelay;
    }
}
