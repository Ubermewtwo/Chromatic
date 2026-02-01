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
    public LayerMask liquidLayer;
    private Rigidbody2D rb;

    public float damageKnockbackForce = 5f;

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
        GameObject rightWallHit = Physics2D.OverlapBox(rightWallCheck.bounds.center, rightWallCheck.bounds.size, 0f, groundLayer)?.gameObject;
        GameObject rightEdgeHit = Physics2D.OverlapBox(rightEdgeCheck.bounds.center, rightEdgeCheck.bounds.size, 0f, groundLayer)?.gameObject;
        GameObject leftWallHit = Physics2D.OverlapBox(leftWallCheck.bounds.center, leftWallCheck.bounds.size, 0f, groundLayer)?.gameObject;
        GameObject leftEdgeHit = Physics2D.OverlapBox(leftEdgeCheck.bounds.center, leftEdgeCheck.bounds.size, 0f, groundLayer)?.gameObject;

        if (movingRight)
        {
            if ((rightWallHit != null || rightEdgeHit == null) && rightEdgeHit != leftEdgeHit)
            {
                movingRight = false;
            }
        }
        else
        {
            if ((leftWallHit != null || leftEdgeHit == null) && rightEdgeHit != leftEdgeHit)
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
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
            if (playerController != null)
            {
                playerController.Knockback(transform.position, damageKnockbackForce);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if tag is lethal and collision is not with liquid layer
        if (collision.CompareTag("Lethal") && collision.gameObject.layer != LayerMask.NameToLayer("Liquid"))
        {
            Debug.Log("Enemy " + gameObject.name + " died");
            Destroy(gameObject);
        }
    }
}
