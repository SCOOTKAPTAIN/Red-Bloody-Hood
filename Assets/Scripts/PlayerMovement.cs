using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    public ParticleSystem smokeFX;
    BoxCollider2D playerCollider;
    bool isFacingRight = false;
    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;

    [Header("Jump")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    private int JumpsRemaining;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;
    bool isOnPlatform;

    [Header("Gravity")]
    public float baseGravity = 2;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2;
    bool isWallSliding;
    //wall jump
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.2f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.1f;
    bool isDashing;
    bool canDash = true;
    TrailRenderer trailRenderer;    

    private void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
    }


    void Update()
    {
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isWallSliding", isWallSliding);
        
        if(isDashing)
        {
            return;
        }

        GroundCheck();
        Gravity();
        ProcessWallSlide();
        ProcessWallJump();
        if(!isWallJumping)
        {
            rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
            Flip();
        }
        animator.SetFloat("magnitude", rb.velocity.magnitude);
    }

    private void Gravity()
    {
        if(rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier; //fall increasingly faster
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    

    private void ProcessWallSlide()
    {
        // not on ground & on a wall & movement !=0
        if(!isGrounded & WallCheck() & horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Math.Max(rb.velocity.y, -wallSlideSpeed));
            smokeFX.Play();
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if(isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        }
        else if(wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if(context.performed && isGrounded && isOnPlatform && playerCollider.enabled)
        {
            StartCoroutine(DisablePlayerCollider(0.25f));
        }
    }

    private IEnumerator DisablePlayerCollider(float disableTime)
    {
        playerCollider.enabled = false;
        yield return new WaitForSeconds(disableTime);
        playerCollider.enabled = true;

    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if(collision.gameObject.CompareTag("platform"))
        {
            isOnPlatform = true;
        } 
    }

     private void OnCollisionExit2D(Collision2D collision) 
    {
        if(collision.gameObject.CompareTag("platform"))
        {
            isOnPlatform = false;
        } 
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if(context.performed && canDash)
        {
            StartCoroutine(DashCoroutine());

        }
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;

        trailRenderer.emitting = true;
        float dashDirection = isFacingRight ? 1f : -1f;

        rb.velocity = new Vector2(dashDirection * dashSpeed, rb.velocity.y);
        yield return new WaitForSeconds(dashDuration);
        rb.velocity = new Vector2(0f,rb.velocity.y);

        isDashing = false;
        trailRenderer.emitting = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(JumpsRemaining > 0)
        {
             // hold down = more height
        if(context.performed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            JumpsRemaining--;
            JumpFX();
        }
        // tap = less height
        else if (context.canceled)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            JumpsRemaining--;
            JumpFX();
        }

        } 
        // wall jump
        if(context.performed && wallJumpTimer > 0f)
        {
            Debug.Log("Wall jmped!");
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;
            JumpFX();

            //force flip
            if(transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }
        
            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f); // wall jump = 0.5f then jump again = 0.6f
        }
    }

    private void JumpFX()
    {
        animator.SetTrigger("jump");
            smokeFX.Play();
    }
    
    private void GroundCheck()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
           JumpsRemaining = maxJumps;
           isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
       
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);

    }


    private void Flip()
    {
        if(isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;

            if(rb.velocity.y == 0)
            {
                smokeFX.Play();
            }         
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
        
    }

}
