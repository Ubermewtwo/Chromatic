using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    public float damageCooldown = 1.0f;
    private float lastDamageTime = -Mathf.Infinity;


    private PlayerController playerController;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (Time.time - lastDamageTime < damageCooldown && damage < maxHealth)
        {
            return; // Still in cooldown
        }

        currentHealth -= damage;
        lastDamageTime = Time.time;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle player death (e.g., respawn, game over)
        Debug.LogWarning("Player has died.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lethal"))
        {
            TakeDamage(3);
        }
        else if (collision.CompareTag("Damaging"))
        {
            Debug.Log("Collided with damaging object: " + collision.gameObject.name);
            TakeDamage(1);
        }

        // if object is in the liquid layer and the player controller mask type is red, call the jump function

        if (collision.gameObject.layer == LayerMask.NameToLayer("Liquid") && playerController.CurrentMask == MaskType.Red)
        {
            playerController.LavaJump();
        }
    }
}