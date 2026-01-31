using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 50f;
    public float deceleration = 50f;
    public float defaultSlipperiness = 1f;
    public float defaultMaxSpeed = 5f;
    private float slipperiness = 1f;
    private float maxSpeed = 5f;
    public Animator anims;

    public float airSpeed = 30f;
    public float maxAirSpeed = 3f;
    public float airSlipperiness = 2f;

    [Header("Jump Settings")]
    public float jumpVelocity = 10f;
    public float jumpDuration = 0.2f;
    public float maxfallSpeed = -10f;
    private float jumpTimer = 0f;
    private bool isJumping = false;

    [Header("Coyote Time and Jump Buffering Settings")]
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;

    [Header("Grab Settings")]
    public bool isGrabbing = false;
    public float wallJumpVelocity = 8f;
    public float maxSlideSpeed = 2.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Transform leftCheck;
    public Transform rightCheck;
    public LayerMask groundLayer;
    public bool isGrounded;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool grabPressed;
    private float defaultGravityScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        slipperiness = defaultSlipperiness;
        maxSpeed = defaultMaxSpeed;
        defaultGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (isGrabbing)
        {
            if (rb.linearVelocity.y < maxSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxSlideSpeed);
            }
            return;
        }

        if (grabPressed)
        {
            if (CanGrab())
            {
                Grab();
                return;
            }
        }

        if (jumpBufferTimer > 0f)
        {
            if (Canjump())
            {
                isJumping = true;
                jumpTimer = jumpDuration;
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            }

            jumpBufferTimer -= Time.deltaTime;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer) && rb.linearVelocityY == 0;
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        GameObject groundObject = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer)?.gameObject;
        if (groundObject != null)
        {
            //Debug.Log("Ground object: " + groundObject.name);
            IWalkableSurface walkableSurface = groundObject.GetComponent<IWalkableSurface>();
            if (walkableSurface != null)
            {
                //Debug.Log("Surface detected: " + groundObject.name);
                slipperiness = walkableSurface.Slipperiness;
                maxSpeed = walkableSurface.MaxSpeed;
            }
            else
            {
                slipperiness = defaultSlipperiness;
                maxSpeed = defaultMaxSpeed;
            }
        }
        else
        {
            slipperiness = defaultSlipperiness;
            maxSpeed = defaultMaxSpeed;
        }

        CharacterMovement();

        if (rb.linearVelocity.y < 0)
        {
            isJumping = false;
        }

        if (isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

            jumpTimer -= Time.deltaTime;

            if (jumpTimer <= 0f)
            {
                isJumping = false;
                jumpTimer = 0f;
            }
        }
    }

    private bool Canjump()
    {
        bool canJump = (isGrounded || coyoteTimer > 0) && !isJumping;
        Debug.Log("Can Jump: " + canJump);
        return canJump;
    }

    private bool CanGrab()
    {
        if (isGrabbing || isGrounded) return false;

        bool facingRight = rb.transform.localScale.x > 0;

        GameObject wall = null;

        if (facingRight)
        {
            wall = Physics2D.OverlapCircle(rightCheck.position, 0.1f, groundLayer)?.gameObject;
        }
        else
        {
            wall = Physics2D.OverlapCircle(rightCheck.position, 0.1f, groundLayer)?.gameObject;
            // When flipping the characters the checks also flip, so both checks are on the right side.
            //wall = Physics2D.OverlapCircle(leftCheck.position, 0.1f, groundLayer)?.gameObject;
        }

        Debug.Log("Wall object: " + (wall != null ? wall.name : "None"));

        if (wall != null)
        {
            IGrabbableSurface grabbableSurface = wall.GetComponent<IGrabbableSurface>();
            if (grabbableSurface != null && grabbableSurface.Grabbable)
            {
                return true;
            }
        }

        return false;
    }

    private void CharacterMovement()
    {
        float xVelocity = 0f;
        if (moveInput != Vector2.zero)
        {
            if (isGrounded)
            {
                float acceleration = moveInput.x * moveSpeed;
                xVelocity = rb.linearVelocityX + (acceleration / slipperiness) * Time.deltaTime;
            }
            else
            {
                float acceleration = moveInput.x * airSpeed;
                xVelocity = rb.linearVelocityX + (acceleration / airSlipperiness) * Time.deltaTime;

                float currentAbsAirSpeed = Mathf.Abs(rb.linearVelocityX);

                if (currentAbsAirSpeed < maxAirSpeed) // if we are below max air speed, clamp to max air speed to not exceed it
                {
                    xVelocity = Mathf.Clamp(xVelocity, -maxAirSpeed, maxAirSpeed);

                }
                else // if we are above max air speed, clamp to current air speed to not decelerate while in air
                {
                    xVelocity = Mathf.Clamp(xVelocity, -currentAbsAirSpeed, currentAbsAirSpeed);
                }
            }
        }
        else
        {
            xVelocity = rb.linearVelocityX - ((deceleration / slipperiness) * Time.deltaTime * Mathf.Sign(rb.linearVelocityX));
            if (Mathf.Sign(rb.linearVelocityX) != Mathf.Sign(xVelocity))
            {
                xVelocity = 0f;
            }
        }

        if (isGrounded)
        {
            xVelocity = Mathf.Clamp(xVelocity, -maxSpeed, maxSpeed);
        }

        rb.linearVelocity = new Vector2(xVelocity, rb.linearVelocity.y);

        // Limit fall speed
        if (rb.linearVelocity.y < maxfallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxfallSpeed);
        }

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

    private void Grab()
    {
        isGrabbing = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = defaultGravityScale / 2f;
    }

    private void CancelGrab()
    {
        if (isGrabbing)
        {
            rb.gravityScale = defaultGravityScale;
            isGrabbing = false;
        }
    }

    //InputSystem

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!isGrabbing)
        {
            if (context.started)
            {
                jumpPressed = true;
                jumpBufferTimer = jumpBufferTime;
            }

            if (context.canceled)
            {
                jumpPressed = false;
                isJumping = false;
            }
        }
        else
        {
            if (context.started)
            {
                CancelGrab();

                isJumping = true;
                jumpTimer = jumpDuration;

                float jumpDirection = rb.transform.localScale.x > 0 ? -1f : 1f;
                rb.linearVelocity = new Vector2(jumpDirection * wallJumpVelocity, jumpVelocity);
                rb.transform.localScale = new Vector3(-rb.transform.localScale.x, 1, 1);
            }
        }
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        Debug.Log("Grab input received");

        if (context.started)
        {
            grabPressed = true;
        }

        if (context.canceled)
        {
            grabPressed = false;
            CancelGrab();
        }
    }
}