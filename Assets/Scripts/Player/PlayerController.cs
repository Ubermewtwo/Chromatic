using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections.Generic;
using Chromatic;

public class PlayerController : MonoBehaviour
{
    //public MaskType currentMask = MaskType.Green;
    public List<MaskType> unlockedMasks = new List<MaskType> { MaskType.Green };
    public int currentMaskIndex = 0;

    public MaskType CurrentMask
    {
        get { return unlockedMasks[currentMaskIndex]; }
    }

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
    //Jungle
    public float attackDuration = 0.3f;
    public BoxCollider2D rightAttackHitbox;
    public BoxCollider2D leftAttackHitbox;
    public float upperAttackRange = 5f;

    //Volcano
    public float groundPoundDelay = 0.3f;

    //Ocean
    public float dashSpeed = 15f;
    public float dashDistance = 5f;
    public bool canDash = true;

    //All
    public bool isPeformingAttack = false;
    private float attackStartTime;

    [Header("Ground Check")]
    public BoxCollider2D groundCheck;
    public BoxCollider2D leftCheck;
    public BoxCollider2D rightCheck;
    public LayerMask groundLayer;
    public bool isGrounded;
    public LayerMask liquidLayer;

    [Header("Visuals")]
    public SpriteRenderer greenSpriteRenderer;
    public PlayerSpritesData greenPlayerSpritesData;
    public SpriteRenderer redSpriteRenderer;
    public PlayerSpritesData redPlayerSpritesData;
    public SpriteRenderer blueSpriteRenderer;
    public PlayerSpritesData bluePlayerSpritesData;

    private SpriteRenderer currentSpriteRenderer;
    private PlayerSpritesData currentPlayerSpritesData;

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

    private void Start()
    {
        SetCurrentMask(currentMaskIndex);
    }

    private void Update()
    {
        UpdateAnimations();

        if (isPeformingAttack)
        {
            return;
        }

        // Input handling
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

                attackStartTime = Time.time;

                float jumpDirection = currentSpriteRenderer.flipX ? 1f : -1f;
                rb.linearVelocity = new Vector2(jumpDirection * wallJumpVelocity, jumpVelocity);
                currentSpriteRenderer.flipX = !currentSpriteRenderer.flipX;
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
            canDash = true;
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

            bool facingRight = currentSpriteRenderer.flipX == false;

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

        attackStartTime = Time.time;
    }

    private void UpdateAnimations()
    {
        PlayerSpritesData.PlayerState currentState = PlayerSpritesData.PlayerState.Idle;

        if (isPeformingAttack)
        {
            currentState = PlayerSpritesData.PlayerState.Attack;
        }
        else if (isGrabbing)
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

        MaskType currentMask = unlockedMasks[currentMaskIndex];
        Sprite currentSprite = currentPlayerSpritesData.GetSprite(currentState, Time.time - attackStartTime, currentMask);
        currentSpriteRenderer.sprite = currentSprite;
    }

    private bool Canjump()
    {
        bool canJump = (isGrounded || coyoteTimer > 0) && !isJumping;
        return canJump;
    }

    private bool CanGrab()
    {
        if (isGrabbing || isGrounded) return false;

        bool facingRight = currentSpriteRenderer.flipX == false;

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
        if (moveInput != Vector2.zero && !isPeformingAttack)
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

        if (isGrounded && !isPeformingAttack)
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
            currentSpriteRenderer.flipX = false;
        }
        if (rb.linearVelocity.x < 0)
        {
            currentSpriteRenderer.flipX = true;
        }
    }

    private void Grab()
    {
        GameObject wall = null;
        bool facingRight = currentSpriteRenderer.flipX == false;

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

    public void OnSwitchMask(InputAction.CallbackContext context)
    {
        if (context.started && MaskTransitionBehaviour.Instance.CanRestart)
        {
            int value = (int)context.ReadValue<float>();
            Debug.Log("Switching mask by value: " + value);
            int newMaskIndex = Mathf.Abs(currentMaskIndex + value) % 3;
            SetCurrentMask(newMaskIndex);
        }
    }

    private void SetCurrentMask(int newMaskIndex)
    {
        MaskTransitionBehaviour.Instance.transform.parent.position = transform.position;
        currentMaskIndex = newMaskIndex;
        switch (unlockedMasks[newMaskIndex])
        {
            case MaskType.Green:
                currentSpriteRenderer = greenSpriteRenderer;
                currentPlayerSpritesData = greenPlayerSpritesData;
                MaskTransitionBehaviour.Instance.ConfigureMask(MaskType.Green);
                break;
            case MaskType.Red:
                currentSpriteRenderer = redSpriteRenderer;
                currentPlayerSpritesData = redPlayerSpritesData;
                MaskTransitionBehaviour.Instance.ConfigureMask(MaskType.Red);
                break;
            case MaskType.Blue:
                currentSpriteRenderer = blueSpriteRenderer;
                currentPlayerSpritesData = bluePlayerSpritesData;
                MaskTransitionBehaviour.Instance.ConfigureMask(MaskType.Blue);
                break;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            attackPressed = true;

            if (!isPeformingAttack && !isGrabbing)
            {
                MaskType currentMask = unlockedMasks[currentMaskIndex];

                switch (currentMask)
                {
                    case MaskType.Green:
                        if (isGrounded)
                        {
                            bool isFacingRight = currentSpriteRenderer.flipX == false;
                            if (isFacingRight)
                            {
                                rightAttackHitbox.gameObject.SetActive(true);
                                DOVirtual.DelayedCall(attackDuration, () => { rightAttackHitbox.gameObject.SetActive(false); });
                            }
                            else
                            {
                                leftAttackHitbox.gameObject.SetActive(true);
                                DOVirtual.DelayedCall(attackDuration, () => { leftAttackHitbox.gameObject.SetActive(false); });
                            }

                            isPeformingAttack = true;
                            attackStartTime = Time.time;
                            isJumping = false;
                            SFXManager.Instance.PlaySFX(whipAttackSFX);
                            DOVirtual.DelayedCall(attackDuration, () => { isPeformingAttack = false; });
                            //Debug.Log("Performing ground green mask attack");

                        }
                        else
                        {
                            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, upperAttackRange, groundLayer);

                            GameObject platformAbove = hit.collider != null ? hit.collider.gameObject : null;

                            if (platformAbove != null)
                            {
                                SpecialPlatform platform = platformAbove.GetComponent<SpecialPlatform>();
                                if (platform != null && currentMask == MaskType.Green)
                                {
                                    //Debug.Log("Climbing platform above: " + platformAbove.name);
                                    isPeformingAttack = true;
                                    attackStartTime = Time.time;
                                    isJumping = false;
                                    SFXManager.Instance.PlaySFX(whipAttackSFX);
                                    rb.linearVelocity = Vector2.zero;
                                    StartCoroutine(ClimbPlatform(platform));
                                }
                                else
                                {
                                    Debug.Log("The platform above does not have a SpecialPlatform component.");
                                }
                            }
                            else
                            {
                                Debug.Log("No platform detected above.");
                            }
                        }
                        break;
                    case MaskType.Red:
                        if (!isGrounded)
                        {
                            isPeformingAttack = true;
                            attackStartTime = Time.time;
                            isJumping = false;
                            SFXManager.Instance.PlaySFX(groundPoundSFX);
                            rb.linearVelocity = Vector2.zero;
                            rb.gravityScale = 0f;
                            StartCoroutine(GroundPound());
                        }
                        break;
                    case MaskType.Blue:
                        if (!canDash) return;
                        canDash = false;

                        isPeformingAttack = true;
                        attackStartTime = Time.time;
                        isJumping = false;
                        SFXManager.Instance.PlaySFX(dashSFX);
                        rb.linearVelocity = Vector2.zero;
                        rb.gravityScale = 0f;
                        float dashDirection = moveInput.x != 0 ? Mathf.Sign(moveInput.x) : (currentSpriteRenderer.flipX ? -1f : 1f);
                        StartCoroutine(Dash(dashDirection));
                        break;
                }
            }
        }
        if (context.canceled)
        {
            attackPressed = false;
        }
    }

    private IEnumerator ClimbPlatform(SpecialPlatform platform)
    {
        while (true)
        {
            Vector3 playerFeetPos = new Vector3(transform.position.x, transform.position.y - (playerCollider.size.y / 2f), transform.position.z);
            Vector3 platformTopPos = new Vector3(platform.transform.position.x, platform.transform.position.y + ((platform.GetComponent<BoxCollider2D>().size.y * platform.transform.localScale.y) / 2f), platform.transform.position.z);

            if (playerFeetPos.y >= platformTopPos.y)
            {
                Debug.Log("Finished climbing platform.");
                isPeformingAttack = false;
                yield break;
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                yield return null;
            }
        }
    }

    private IEnumerator GroundPound()
    {
        bool goingDown = false;

        while (true)
        {
            if (!goingDown)
            {
                float timeSinceStart = Time.time - attackStartTime;

                if (timeSinceStart >= groundPoundDelay)
                {
                    goingDown = true;
                    rb.gravityScale = defaultGravityScale * 3f;
                }
            }
            else if (isGrounded)
            {
                Debug.Log("Landed from ground pound.");
                rb.gravityScale = defaultGravityScale;
                isPeformingAttack = false;
                // break the ground here if needed

                GameObject groundObject = Physics2D.OverlapBox(groundCheck.bounds.center, groundCheck.bounds.size, 0f, groundLayer)?.gameObject;

                SpecialPlatform platform = groundObject != null ? groundObject.GetComponent<SpecialPlatform>() : null;

                if (platform != null)
                {
                    platform.Break();
                }

                yield break;
            }

            yield return null;
        }
    }

    public void LavaJump()
    {
        StopAllCoroutines();
        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity * 2f);
        // other logic
    }

    private IEnumerator Dash(float dashDirection)
    {
        float initialXPosition = transform.position.x;

        while (true)
        {
            GameObject wall = null;

            if (dashDirection == 1f)
            {
                wall = Physics2D.OverlapBox(rightCheck.bounds.center, rightCheck.bounds.size, 0f, groundLayer)?.gameObject;
            }
            else
            {
                wall = Physics2D.OverlapBox(leftCheck.bounds.center, leftCheck.bounds.size, 0f, groundLayer)?.gameObject;
            }

            if (wall != null)
            {
                Debug.Log("Dash interrupted by wall.");
                isPeformingAttack = false;
                rb.gravityScale = defaultGravityScale;
                yield break;
            }

            float distanceTravelled = Mathf.Abs(transform.position.x - initialXPosition);
            if (distanceTravelled >= dashDistance)
            {
                Debug.Log("Finished dashing.");
                isPeformingAttack = false;
                rb.gravityScale = defaultGravityScale;
                if (isGrounded)
                {
                    rb.linearVelocity = new Vector2(dashDirection * defaultMaxSpeed, 0f);
                }
                else
                {
                    rb.linearVelocity = new Vector2(dashDirection * maxAirSpeed, 0f);
                }
                yield break;
            }
            else
            {
                rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
                yield return null;
            }
        }
    }
}

public enum MaskType
{
    Green,
    Red,
    Blue
}