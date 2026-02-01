using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GreyPlayerController : MonoBehaviour
{
    public GameObject maskedPlayerPrefab;

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

    private float attackStartTime;

    [Header("Ground Check")]
    public BoxCollider2D groundCheck;
    public LayerMask groundLayer;
    public bool isGrounded;
    public LayerMask liquidLayer;

    [Header("Visuals")]
    public SpriteRenderer greySpriteRenderer;
    public PlayerSpritesData greyPlayerSpritesData;

    public List<Sprite> begginingAnimationSprites = new List<Sprite>();
    public float begginingAnimationFrameRate = 10f;

    [Header("SFX")]
    public AudioClipPlus walkSFX;
    public float walkSFXInterval = 0.4f;
    private float walkSFXTimer = 0f;
    public AudioClipPlus jumpSFX;
    public AudioClipPlus landSFX;
    public AudioClipPlus grabSFX;
    public AudioClipPlus whipAttackSFX;
    public AudioClipPlus groundPoundSFX;
    public AudioClipPlus dashSFX;

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;
    private Vector2 moveInput;
    private bool jumpPressed;
    private float defaultGravityScale;

    public bool debugObtainMask = false;

    private void OnValidate()
    {
        if (debugObtainMask)
        {
            debugObtainMask = false;
            ObtainMask();
        }
    }

    public void ObtainMask()
    {
        GetComponent<PlayerInput>().enabled = false;
        GameObject maskedPlayer = Instantiate(maskedPlayerPrefab, transform.position, transform.rotation);
        Camera.main.GetComponent<CameraBehavior>().target = maskedPlayer.transform;
        Destroy(gameObject);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        slipperiness = defaultSlipperiness;
        maxSpeed = defaultMaxSpeed;
        defaultGravityScale = rb.gravityScale;

        GetComponent<PlayerInput>().enabled = false;

        // use dotween to play beggining animation
        DOVirtual.DelayedCall(0.1f, () =>
        {
            float animationDuration = begginingAnimationSprites.Count / begginingAnimationFrameRate;
            float frameDuration = 1f / begginingAnimationFrameRate;
            int currentFrame = 0;
            DOVirtual.DelayedCall(frameDuration, null, true).OnStepComplete(() =>
            {
                if (currentFrame < begginingAnimationSprites.Count)
                {
                    greySpriteRenderer.sprite = begginingAnimationSprites[currentFrame];
                    currentFrame++;
                }
            }).SetLoops(begginingAnimationSprites.Count, LoopType.Restart).OnComplete(() =>
            {
                GetComponent<PlayerInput>().enabled = true;
            });
        });
    }

    private void Update()
    {
        UpdateAnimations();

        if (jumpBufferTimer > 0f)
        {
            if (Canjump()) // Regular jump
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

        attackStartTime = Time.time;
    }

    private void UpdateAnimations()
    {
        PlayerSpritesData.PlayerState currentState = PlayerSpritesData.PlayerState.Idle;

        if (!isGrounded)
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

        Sprite currentSprite = greyPlayerSpritesData.GetSprite(currentState, Time.time - attackStartTime, MaskType.Green);
        greySpriteRenderer.sprite = currentSprite;
    }

    private bool Canjump()
    {
        bool canJump = (isGrounded || coyoteTimer > 0) && !isJumping;
        return canJump;
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
            greySpriteRenderer.flipX = false;
        }
        if (rb.linearVelocity.x < 0)
        {
            greySpriteRenderer.flipX = true;
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
}
