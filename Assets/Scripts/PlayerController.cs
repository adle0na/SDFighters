using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    public CapsuleCollider playerCapsuleCollider;

    public bool isMoving    = false;
    public bool isDashing   = false;
    public bool isJumping   = false;
    public bool isGuarding  = false;
    public bool isAttacking = false;
    public bool isATK0      = false;
    public bool isATK1      = false;
    public bool isATK2      = false;
    public bool isLeft      = false;

    public bool canDash = true;
    public bool canMove = true;
    
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode upKey;
    private KeyCode downKey;
    
    private KeyCode attackAKey;
    private KeyCode attackBKey;
    private KeyCode jumpKey;

    public bool isGrounded = true;
    public bool isDamaged = false;
    
    private float lastTapTime = 0f;
    private float doubleTapTimeThreshold = 0.2f;
    
    private float nextfiretime   = 1f;
    private static int noOfCombo = 0;
    private float lastAttackTime = 0;
    private float maxComboDelay  = 1;
    
    private bool isInvincible = false; // 캐릭터가 무적 상태인지 나타내는 변수
    private float invincibleDelay = 0.3f;

    public Slider playerHpBar;
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerCapsuleCollider = GetComponent<CapsuleCollider>();
        
        playerHpBar.value = 1;
        
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

        DoGuard();
        
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

        if(canMove)
            Move(horizontalInput);
        
        if (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f &&
            playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("ATK0"))
        {
            isATK0 = false;
            playerAnimator.SetBool("ATK0", isATK0);
        }
        if (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f &&
            playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("ATK1"))
        {
            isATK1 = false;
            playerAnimator.SetBool("ATK1", isATK1);
        }
        if (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f &&
            playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("ATK2"))
        {
            isATK2 = false;
            playerAnimator.SetBool("ATK2", isATK2);
            noOfCombo = 0;
        }

        // if (Time.time - lastAttackTime > maxComboDelay)
        // {
        //     noOfCombo = 0;
        // }

        if (Time.time > nextfiretime)
        {
            if (Input.GetKey(attackAKey))
            {
                StartComboAttack();
            }
        }
        
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon") ||
            other.gameObject.CompareTag("RangeAttack") ||
            other.gameObject.CompareTag("RangeAttack2") ||
            other.gameObject.CompareTag("RangeAttack3") ||
            other.gameObject.CompareTag("MeleeAttack") ||
            other.gameObject.CompareTag("MeleeAttack2") ||
            other.gameObject.CompareTag("MeleeAttack3"))
        {
            Debug.Log("충돌감지");
            
            if (!isInvincible)
            {
                isDamaged = true;
                Debug.Log("피격");
                playerAnimator.SetBool("IsDamaged", isDamaged);
                playerHpBar.value -= GetDamageValue(other.gameObject.tag);

                // 무적 코루틴 시작
                StartCoroutine(InvincibleDelay());
            }
        }
    }

    private float GetDamageValue(string tag)
    {
        switch (tag)
        {
            case "Weapon":
                return 0.01f;
            case "RangeAttack":
                return 0.03f;
            case "RangeAttack2":
                return 0.05f;
            case "RangeAttack3":
                return 0.1f;
            case "MeleeAttack":
                return 0.05f;
            case "MeleeAttack2":
                return 0.1f;
            case "MeleeAttack3":
                return 0.2f;
            default:
                return 0f;
        }
    }
    private IEnumerator InvincibleDelay()
    {
        isInvincible = true;
        
        // 무적 딜레이 시간 동안 대기
        yield return new WaitForSeconds(invincibleDelay);

        // 무적 해제
        isInvincible = false;

        // 피격 상태 해제
        isDamaged = false;
        playerAnimator.SetBool("IsDamaged", isDamaged);
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
        if (Input.GetKeyDown(leftKey) || Input.GetKeyDown(rightKey)&& canDash && !isAttacking)
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

    private void StartComboAttack()
    {
        if (Input.GetKeyDown(attackBKey)) return;

        lastTapTime = Time.time;

        noOfCombo++;

        Debug.Log("콤보어택" + noOfCombo);

        isAttacking = true;
        canDash = false;
        canMove = false;

        if (noOfCombo == 1)
        {
            isATK0 = true;
            isATK1 = false;
            isATK2 = false;

            playerAnimator.SetBool("ATK0", isATK0);
            playerAnimator.SetBool("ATK1", isATK1);
            playerAnimator.SetBool("ATK2", isATK2);
        }
        else if (noOfCombo == 2)
        {
            isATK0 = false;
            isATK1 = true;
            isATK2 = false;

            playerAnimator.SetBool("ATK0", isATK0);
            playerAnimator.SetBool("ATK1", isATK1);
            playerAnimator.SetBool("ATK2", isATK2);
        }
        else if (noOfCombo >= 3)
        {
            isATK0 = false;
            isATK1 = false;
            isATK2 = true;

            playerAnimator.SetBool("ATK0", isATK0);
            playerAnimator.SetBool("ATK1", isATK1);
            playerAnimator.SetBool("ATK2", isATK2);
            noOfCombo = 0; // 콤보가 3 이상이면 초기화
        }

        StartCoroutine(AttackTime());
    }

    private void DoGuard()
    {
        if (isAttacking)
        {
            isAttacking = false;
            isATK0 = false;
            isATK1 = false;
            isATK2 = false;
        }
        
        if (Input.GetKeyDown(attackAKey) && Input.GetKeyDown(attackBKey))
        {
            isGuarding = true;

            canMove = false;
            canDash = false;
            
            playerAnimator.SetBool("IsGuarding", isGuarding);

            float guardStartTime = Time.time;

            StartCoroutine(GuardTimer(guardStartTime));
        }

        if (Input.GetKeyUp(attackAKey) || Input.GetKeyUp(attackBKey))
        {
            isGuarding = false;
            
            playerAnimator.SetBool("IsGuarding", isGuarding);
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

    private IEnumerator AttackTime()
    {
        // 공격 애니메이션의 지속 시간이라 가정
        float attackAnimationDuration = 1.5f;

        yield return new WaitForSeconds(attackAnimationDuration);

        // 공격이 끝난 후 대시 가능하도록 설정
        isAttacking = false;
        canDash = true;
        canMove = true;
    }
    
    private  IEnumerator GuardTimer(float startTime)
    {
        float maxGuardDuration = 5f;

        while (Time.time - startTime < maxGuardDuration)
        {
            yield return null; // 한 프레임 대기
        }
        isGuarding = false;
        canDash = true;
        canMove = true;
        playerAnimator.SetBool("IsGuarding", isGuarding);
    }
}
