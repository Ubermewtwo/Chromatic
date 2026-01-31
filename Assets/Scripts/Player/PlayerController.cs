using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public MaskType currentMask = MaskType.Green;

    [Header("Movement Settings")]
    public float moveSpeed = 50f;
    public float deceleration = 50f;
    public float defaultSlipperiness = 1f;
    public float defaultMaxSpeed = 5f;
    private float slipperiness = 1f;
    private float maxSpeed = 5f;
    //public Animator anims;

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

    [Header("Attack Settings")]
    public BoxCollider2D rightAttackHitbox;
    public BoxCollider2D leftAttackHitbox;
    public bool isPeformingAttack = false;

    [Header("Ground Check")]
    public BoxCollider2D groundCheck;
    public BoxCollider2D leftCheck;
    public BoxCollider2D rightCheck;
    public LayerMask groundLayer;
    public bool isGrounded;
    public LayerMask liquidLayer;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public PlayerSpritesData playerSpritesData;

    [Header("SFX")]
    public AudioClipPlus walkSFX;
    public float walkSFXInterval = 0.4f;
    private float walkSFXTimer = 0f;
    public AudioClipPlus jumpSFX;
    public AudioClipPlus landSFX;
    public AudioClipPlus grabSFX;

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool grabPressed;
    private bool attackPressed;
    private float defaultGravityScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        slipperiness = defaultSlipperiness;
        maxSpeed = defaultMaxSpeed;
        defaultGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        UpdateAnimations();

        //if (isPeformingAttack)
        //{
        //    return;
        //}

        //// Input handling
        //if (attackPressed)
        //{
        //    switch (currentMask)
        //    {
        //        case MaskType.Green:
        //            if (isGrounded)
        //            {
        //                bool isFacingRight = spriteRenderer.flipX == false;
        //                if (isFacingRight)
        //                {
        //                    // Perform ground green mask attack with rightAttackHitbox
        //                }
        //                else
        //                {
        //                    // Perform ground green mask attack with leftAttackHitbox
        //                }

        //                isPeformingAttack = true;

        //            }
        //            else
        //            {
        //                // Perform air green mask attack
        //            }
        //            break;
        //        case MaskType.Red:
        //            // Perform red mask attack
        //            break;
        //        case MaskType.Blue:
        //            // Perform blue mask attack
        //            break;
        //    }
        //}

        if (grabPressed)
        {
            if (CanGrab())
            {
                Grab();
            }
        }

        if (jumpBufferTimer > 0f)
        {
            if (isGrabbing) // Wall jump
            {
                CancelGrab();

                isJumping = true;
                jumpTimer = jumpDuration;
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                SFXManager.Instance.PlaySFX(jumpSFX);

                float jumpDirection = spriteRenderer.flipX ? 1f : -1f;
                rb.linearVelocity = new Vector2(jumpDirection * wallJumpVelocity, jumpVelocity);
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }
            else if (Canjump()) // Regular jump
            {
                Jump();
            }
            else if (!isGrounded) // Liquid jump
            {
                GameObject liquid = Physics2D.OverlapBox(playerCollider.bounds.center, playerCollider.bounds.size, 0f, liquidLayer)?.gameObject;

                if (liquid != null)
                {
                    Jump();
                }
            }

            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.bounds.center, groundCheck.bounds.size, 0f, groundLayer) && rb.linearVelocity.y == 0;
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (!wasGrounded && isGrounded)
        {
            SFXManager.Instance.PlaySFX(landSFX);
        }

        // Wall grab logic
        if (isGrabbing)
        {
            if (rb.linearVelocity.y < maxSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxSlideSpeed);
            }

            GameObject grabbedWall = null;

            bool facingRight = spriteRenderer.flipX == false;

            if (facingRight)
            {
                grabbedWall = Physics2D.OverlapBox(rightCheck.bounds.center, rightCheck.bounds.size, 0f, groundLayer)?.gameObject;
            }
            else
            {
                grabbedWall = Physics2D.OverlapBox(leftCheck.bounds.center, leftCheck.bounds.size, 0f, groundLayer)?.gameObject;
            }

            if (grabbedWall == null)
            {
                CancelGrab();
            }

            return;
        }

        // Update slipperiness and max speed based on ground surface
        GameObject groundObject = Physics2D.OverlapBox(groundCheck.bounds.center, groundCheck.bounds.size, 0f, groundLayer)?.gameObject;
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
        
        CharacterMovement(); // Handle horizontal movement

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

    private void Jump()
    {
        isJumping = true;
        jumpTimer = jumpDuration;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
        SFXManager.Instance.PlaySFX(jumpSFX);
    }

    private void UpdateAnimations()
    {
        PlayerSpritesData.PlayerState currentState = PlayerSpritesData.PlayerState.Idle;

        if (isGrabbing)
        {
            currentState = PlayerSpritesData.PlayerState.Grab;
        }
        else if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0)
            {
                currentState = PlayerSpritesData.PlayerState.Jump;
            }
            else
            {
                currentState = PlayerSpritesData.PlayerState.Fall;
            }
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            currentState = PlayerSpritesData.PlayerState.Walk;
        }
        else
        {
            currentState = PlayerSpritesData.PlayerState.Idle;
        }

        Sprite currentSprite = playerSpritesData.GetSprite(currentState, Time.time);
        spriteRenderer.sprite = currentSprite;
    }

    private bool Canjump()
    {
        bool canJump = (isGrounded || coyoteTimer > 0) && !isJumping;
        return canJump;
    }

    private bool CanGrab()
    {
        if (isGrabbing || isGrounded) return false;

        bool facingRight = spriteRenderer.flipX == false;

        GameObject wall = null;

        if (facingRight)
        {
            wall = Physics2D.OverlapBox(rightCheck.bounds.center, rightCheck.bounds.size, 0f, groundLayer)?.gameObject;
        }
        else
        {
            wall = Physics2D.OverlapBox(leftCheck.bounds.center, leftCheck.bounds.size, 0f, groundLayer)?.gameObject;
        }

        //Debug.Log("Wall object: " + (wall != null ? wall.name : "None"));

        if (wall != null)
        {
            if (wall.CompareTag("Grabbable"))
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

                if (walkSFXTimer <= 0f && Mathf.Abs(rb.linearVelocityX) > 0.1f)
                {
                    SFXManager.Instance.PlaySFX(walkSFX);
                    walkSFXTimer = walkSFXInterval;
                }
                else
                {
                    walkSFXTimer -= Time.deltaTime;
                }
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

        // Sprite orientation
        if (rb.linearVelocity.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        if (rb.linearVelocity.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void Grab()
    {
        GameObject wall = null;
        bool facingRight = spriteRenderer.flipX == false;

        if (facingRight)
        {
            wall = Physics2D.OverlapBox(rightCheck.bounds.center, rightCheck.bounds.size, 0f, groundLayer)?.gameObject;
        }
        else
        {
            wall = Physics2D.OverlapBox(leftCheck.bounds.center, rightCheck.bounds.size, 0f, groundLayer)?.gameObject;
        }

        // snap player to wall
        if (wall != null)
        {
            BoxCollider2D wallCollider = wall.GetComponent<BoxCollider2D>();

            if (wallCollider == null)
            {
                Debug.LogWarning("Wall object does not have a BoxCollider2D component.");
                return;
            }

            Vector3 wallPosition = wall.transform.position;
            float snapPosition = wallPosition.x;

            if (facingRight)
            {
                snapPosition -= (wallCollider.size.x * wallCollider.transform.localScale.x) / 2f;
                snapPosition -= playerCollider.size.x / 2f;
            }
            else
            {
                snapPosition += (wallCollider.size.x * wallCollider.transform.localScale.x) / 2f;
                snapPosition += playerCollider.size.x / 2f;
            }

            rb.transform.position = new Vector3(snapPosition, rb.transform.position.y, rb.transform.position.z);
            //Debug.Log("Snapped to wall at position: " + snapPosition);
        }

        isGrabbing = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = defaultGravityScale / 2f;
        SFXManager.Instance.PlaySFX(grabSFX);
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

    public void OnGrab(InputAction.CallbackContext context)
    {
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

public enum MaskType
{
    Green,
    Red,
    Blue
}