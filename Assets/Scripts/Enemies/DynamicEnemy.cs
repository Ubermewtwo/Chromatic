using UnityEngine;

public class DynamicEnemy : MonoBehaviour
{
    public float speed = 2f;

    public bool movingRight = true;

    public BoxCollider2D rightWallCheck;
    public BoxCollider2D leftWallCheck;
    public BoxCollider2D rightEdgeCheck;
    public BoxCollider2D leftEdgeCheck;

    public LayerMask groundLayer;
    private Rigidbody2D rb;

    public EnemySpritesData enemySpritesData;

    public SpriteRenderer greenSpriteRenderer;
    public SpriteRenderer redSpriteRenderer;
    public SpriteRenderer blueSpriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Sprite greenSprite = enemySpritesData.GetSprite(MaskType.Green, Time.time);
        Sprite redSprite = enemySpritesData.GetSprite(MaskType.Red, Time.time);
        Sprite blueSprite = enemySpritesData.GetSprite(MaskType.Blue, Time.time);
        greenSpriteRenderer.sprite = greenSprite;
        redSpriteRenderer.sprite = redSprite;
        blueSpriteRenderer.sprite = blueSprite;
    }

    private void FixedUpdate()
    {
        // Change direction if needed
        if (movingRight)
        {
            GameObject rightWallHit = Physics2D.OverlapBox(rightWallCheck.bounds.center, rightWallCheck.bounds.size, 0f, groundLayer)?.gameObject;
            GameObject rightEdgeHit = Physics2D.OverlapBox(rightEdgeCheck.bounds.center, rightEdgeCheck.bounds.size, 0f, groundLayer)?.gameObject;
            if (rightWallHit != null || rightEdgeHit == null)
            {
                movingRight = false;
            }
        }
        else
        {
            GameObject leftWallHit = Physics2D.OverlapBox(leftWallCheck.bounds.center, leftWallCheck.bounds.size, 0f, groundLayer)?.gameObject;
            GameObject leftEdgeHit = Physics2D.OverlapBox(leftEdgeCheck.bounds.center, leftEdgeCheck.bounds.size, 0f, groundLayer)?.gameObject;
            if (leftWallHit != null || leftEdgeHit == null)
            {
                movingRight = true;
            }
        }

        // Movement
        if (movingRight)
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
        }

        // Sprite orientation
        if (movingRight)
        {
            greenSpriteRenderer.flipX = true;
            redSpriteRenderer.flipX = true;
            blueSpriteRenderer.flipX = true;
        }
        else
        {
            greenSpriteRenderer.flipX = false;
            redSpriteRenderer.flipX = false;
            blueSpriteRenderer.flipX = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Damage the player
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Lethal"))
        {
            Debug.Log("Enemy " + gameObject.name + " died");
            Destroy(gameObject);
        }
    }
}
