using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public UserType userType;

    private float walkSpeed = 2f;
    private float runSpeed = 5f;
    
    private float dashSpeed = 10f;
    private float dashTime = 1f;
    
    public float jumpForce = 10f;
    
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCapsuleCollider;

    private bool isMoving = false;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isLeft = false;

    private bool canDash = true;
    private bool canMove = true;
    
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode upKey;
    private KeyCode downKey;
    
    private KeyCode attackAKey;
    private KeyCode attackBKey;
    private KeyCode jumpKey;

    private bool isGrounded = true;
    
    private float lastTapTime = 0f;
    private float doubleTapTimeThreshold = 0.2f;

    private float turnSmoothVelocity;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerCapsuleCollider = GetComponent<CapsuleCollider>();
        
        if (userType == UserType.Player1)
        {
            leftKey  = KeyCode.A;
            rightKey = KeyCode.D;
            upKey    = KeyCode.W;
            downKey  = KeyCode.S;

            attackAKey = KeyCode.F;
            attackBKey = KeyCode.G;
            jumpKey    = KeyCode.H;
        }
        else if (userType == UserType.Player2)
        {
            leftKey = KeyCode.RightArrow;
            rightKey = KeyCode.LeftArrow;
            upKey    = KeyCode.UpArrow;
            downKey  = KeyCode.DownArrow;

            attackAKey = KeyCode.Keypad4;
            attackBKey = KeyCode.Keypad5;
            jumpKey    = KeyCode.Keypad6;
        }
    }

    private void Update()
    {
        DashInput(leftKey, rightKey);

        DoJump();
        
        float horizontalInput = 0f;

        if (Input.GetKey(leftKey))
        {
            horizontalInput = (userType == UserType.Player1) ? 1f : -1f;
        }
        else if (Input.GetKey(rightKey))
        {
            horizontalInput = (userType == UserType.Player1) ? -1f : 1f;
        }

        isMoving = Mathf.Abs(horizontalInput) > 0.1f;

        if(canDash)
            Move(horizontalInput);
        
        playerAnimator.SetBool("IsJumping", isJumping);
        playerAnimator.SetBool("IsDashing", isDashing);
        playerAnimator.SetBool("IsMoving", isMoving);
    }
    
    private void FixedUpdate()
    {
        if (isDashing)
        {
            DoDash();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // 플레이어가 다시 지면에 착지하면 isGrounded를 true로 설정
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("땅감지");
            playerAnimator.SetBool("IsJumping", isJumping);
        }
    }

    private void Move(float horizontalInput)
    {
        Vector3 moveDirection = new Vector3(0f, 0f, horizontalInput);
        moveDirection.Normalize();

        // 플레이어의 방향을 바로 설정
        if (moveDirection != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        Vector3 walkDirection = new Vector3(0f, 0f, horizontalInput); // Z 축 방향으로 설정
        Vector3 walkVelocity = walkDirection * walkSpeed;
        playerRigidbody.velocity = walkVelocity;
    }
    
    private void DashInput(KeyCode leftKey, KeyCode rightKey)
    {
        if (Input.GetKeyDown(leftKey) || Input.GetKeyDown(rightKey)&& canDash)
        {
            if (Time.time - lastTapTime < doubleTapTimeThreshold)
            {
                isLeft = Input.GetKey(leftKey);
                isDashing = true;
                canDash = false;
                
                if (isLeft)
                    Debug.Log("Double tap for dash to the left");
                else
                    Debug.Log("Double tap for dash to the right");

                playerAnimator.SetBool("IsDashing", isDashing);
                StartCoroutine(ResetDash());
            }

            lastTapTime = Time.time;
        }
        else if (Input.GetKeyUp(leftKey) || Input.GetKeyUp(rightKey))
        {
            isMoving = false;
            isDashing = false;
            Debug.Log(leftKey + " or " + rightKey + " 키 떼기");
        }
        
        if (isDashing && !Input.GetKey(leftKey) && !Input.GetKey(rightKey))
        {
            isDashing = false;
            playerAnimator.SetBool("IsDashing", isDashing);
        }
    }
    
    private void DoDash()
    {
        Vector3 dashDirection = transform.forward; // 대시 방향을 현재 바라보는 방향으로 고정
        Vector3 dashVelocity = dashDirection * dashSpeed;
        playerRigidbody.velocity = dashVelocity;
    }

    private void DoJump()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            isJumping = true;
            isGrounded = false;
            
            playerAnimator.SetBool("IsJumping", isJumping);

            Debug.Log("H 점프키 입력");

            StartCoroutine(ResetJump());
        }
    }
    
    private IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(0.5f);
        isJumping = false;
        playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private IEnumerator ResetDash()
    {
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
        canDash = true;
    }
}
