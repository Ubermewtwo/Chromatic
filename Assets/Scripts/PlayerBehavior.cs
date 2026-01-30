using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 5f;
    public int extraJumps;
    private int currentJumps;
    public Animator anims;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Transform leftCheck;
    public Transform rightCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpPressed;

    [Header("Gameplay")]
    public Vector3 checkPointPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isGrounded)
        {
            if (CheckWalls() == false)
            {
                CharacterMovement();
            }
        }
        else
        {
            CharacterMovement();
            currentJumps = 0;
        }

        if (jumpPressed)
        {
            if (isGrounded)
            {
                Jump();
            }
            else
            {
                if (currentJumps < extraJumps)
                {
                    currentJumps++;
                    Jump();
                }
            }
        }
    }

    private void CharacterMovement()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        //Orientacion de la sprite

        if (rb.linearVelocity.x > 0)
        {
            rb.transform.localScale = Vector3.one;
        }
        if (rb.linearVelocity.x < 0)
        {
            rb.transform.localScale = new Vector3(-1, 1, 1);
        }

        //Animacion movimiento

        anims.SetBool("Moving", rb.linearVelocity.x != 0);
    }

    private bool CheckWalls()
    {
        bool nearWall = false;

        if (Physics2D.OverlapCircle(leftCheck.position, groundCheckRadius, groundLayer))
        {
            nearWall = true;
        }

        if (Physics2D.OverlapCircle(rightCheck.position, groundCheckRadius, groundLayer))
        {
            nearWall = true;
        }

        return nearWall;
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
        jumpPressed = false;
    }

    //InputSystem

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpPressed = true;
        if (context.canceled)
            jumpPressed = false;
    }

    //Triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kill"))
        {
            //Death. Aqui deberia haber una transicion y eso

            transform.position = checkPointPos;
        }

        if (collision.CompareTag("CheckPoint"))
        {
            //Checkpoint guardado

            checkPointPos = collision.transform.position;
        }
    }

    // Groundcheck

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (leftCheck == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(leftCheck.position, groundCheckRadius);

        if (rightCheck == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(rightCheck.position, groundCheckRadius);
    }
}