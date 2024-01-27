using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    
    public SphereCollider weaponCollider;

    public bool isMoving    = false;
    public bool isDashing   = false;
    public bool isJumping   = false;
    public bool isGuarding  = false;
    public bool isAttacking = false;
    public bool isRest      = false;
    public bool isATK0      = false;
    public bool isATK1      = false;
    public bool isATK2      = false;
    public bool isLeft      = false;

    public bool canDash = true;
    public bool canMove = true;

    public bool Win = false;
    public bool Lose = false;

    public bool isSkillUsing = false;

    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode upKey;
    private KeyCode downKey;
    
    private KeyCode attackAKey;
    private KeyCode attackBKey;
    private KeyCode jumpKey;

    public bool isGrounded = true;
    public bool isDamaged = false;
    public bool isTimed = false;
    
    private float lastTapTime = 0f;
    private float doubleTapTimeThreshold = 0.2f;
    
    private float nextfiretime   = 1f;
    private static int noOfCombo = 0;
    private float lastAttackTime = 0;
    private float maxComboDelay  = 1;
    
    private float restStartTime;
    private float restDuration = 5f; // Adjust the duration of rest as needed
    private float healthRegenRate = 0.001f;
    
    private bool isInvincible = false; // 캐릭터가 무적 상태인지 나타내는 변수
    private float invincibleDelay = 0.3f;

    public bool isSkillCoolDown = false;
    private float skillcoolDownRate = 5f;

    public Slider playerHpBar;

    public List<GameObject> RangeSkills;
    public List<GameObject> MeleeSkills;

    public List<Transform> RangeSkillPositions;
    public List<Transform> MeleeSkillPositions;

    public List<float> RangeSkillTimes;
    public List<float> MeleeSkillTimes;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();

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

        //DoGuard();

        DoRest();

        DoRangeSkill();
        
        DoMeleeSkill();
        
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

        if(canMove && !isTimed)
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
        if (isDashing && !isTimed)
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
        if (userType == UserType.Player1)
        {
            if (other.gameObject.CompareTag("Weapon") ||
                other.gameObject.CompareTag("Player2MeleeSkill1") ||
                other.gameObject.CompareTag("Player2MeleeSkill2") ||
                other.gameObject.CompareTag("Player2MeleeSkill3") ||
                other.gameObject.CompareTag("Player2MeleeSkill5") ||
                other.gameObject.CompareTag("Player2RangeSkill1")
               )
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

            if (other.gameObject.CompareTag("Player2MeleeSkill4"))
            {
                isTimed = true;
                
                Debug.Log("타임 감지");
                StartCoroutine(TimeSlow());
            }
        }
        else
        {
            if (other.gameObject.CompareTag("Weapon") ||
                other.gameObject.CompareTag("Player1MeleeSkill1") ||
                other.gameObject.CompareTag("Player1MeleeSkill2") ||
                other.gameObject.CompareTag("Player1MeleeSkill3") ||
                other.gameObject.CompareTag("Player1MeleeSkill5") ||
                other.gameObject.CompareTag("Player1RangeSkill1"))
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
            
            if (other.gameObject.CompareTag("Player1MeleeSkill4"))
            {
                isTimed = true;
                
                Debug.Log("타임 감지");
                StartCoroutine(TimeSlow());
            }
        }

    }

    private float GetDamageValue(string tag)
    {
        switch (tag)
        {
            case "Weapon":
                return 0.01f;
            case "Player1MeleeSkill1":
                return 0.05f;
            case "Player2MeleeSkill1":
                return 0.05f;
            case "Player1MeleeSkill2":
                return 0.15f;
            case "Player2MeleeSkill2":
                return 0.15f;
            case "Player1MeleeSkill3":
                return 0.3f;
            case "Player2MeleeSkill3":
                return 0.3f;
            case "Player1MeleeSkill5":
                return 0.3f;
            case "Player2MeleeSkill5":
                return 0.3f;
            case "Player1RangeSkill1":
                return 0.03f;
            case "Player2RangeSkill1":
                return 0.03f;
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
        if (isTimed) return;
        
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
            isMoving  = false;
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
        if (isTimed) return;
        
        if (Input.GetKeyDown(attackBKey)) return;

        lastTapTime = Time.time;

        noOfCombo++;

        Debug.Log("콤보어택" + noOfCombo);

        isAttacking = true;
        canDash = false;
        canMove = false;

        weaponCollider.gameObject.SetActive(true);
        
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

    // private void DoGuard()
    // {
    //     if (isAttacking)
    //     {
    //         isAttacking = false;
    //         isATK0 = false;
    //         isATK1 = false;
    //         isATK2 = false;
    //     }
    //     
    //     if (Input.GetKeyDown(attackAKey) && Input.GetKeyDown(attackBKey) && !Input.GetKeyDown(downKey))
    //     {
    //         isGuarding = true;
    //
    //         canMove = false;
    //         canDash = false;
    //         
    //         playerAnimator.SetBool("IsGuarding", isGuarding);
    //
    //         float guardStartTime = Time.time;
    //
    //         StartCoroutine(GuardTimer(guardStartTime));
    //     }
    //
    //     if (Input.GetKeyUp(attackAKey) || Input.GetKeyUp(attackBKey))
    //     {
    //         isGuarding = false;
    //         
    //         playerAnimator.SetBool("IsGuarding", isGuarding);
    //     }
    // }

    private void DoRest()
    {
        if (Input.GetKeyDown(attackAKey) && Input.GetKeyDown(attackBKey) && Input.GetKeyDown(downKey))
        {
            isRest = true;
            restStartTime = Time.time;

            playerAnimator.SetBool("IsResting", isRest);

            StartCoroutine(RestTimer(restStartTime));
        }

        if (Input.GetKeyUp(attackAKey) || Input.GetKeyUp(attackBKey) || Input.GetKeyUp(downKey))
        {
            isRest = false;
            canDash = true;
            canMove = true;

            playerAnimator.SetBool("IsResting", isRest);
        }

        // Check if currently resting to apply health regeneration
        if (isRest)
        {
            // Implement health regeneration logic here
            float elapsedRestTime = Time.time - restStartTime;
            float healthRegenAmount = elapsedRestTime * healthRegenRate;
            // Assuming you have a method to update the player's health, like UpdateHealth(float amount)
            UpdateHealth(healthRegenAmount);
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
        if (Input.GetKeyDown(jumpKey) && isGrounded && !isTimed)
        {
            isJumping = true;
            isGrounded = false;
            
            playerAnimator.SetBool("IsJumping", isJumping);

            Debug.Log("H 점프키 입력");

            StartCoroutine(ResetJump());
        }
    }

    private void DoRangeSkill()
    {
        if (Input.GetKeyDown(leftKey) && Input.GetKeyDown(rightKey) && Input.GetKeyDown(attackBKey) && !isSkillCoolDown)
        {
            isSkillUsing = true;

            playerAnimator.SetBool("IsSkillUsing", isSkillUsing);

            UsingSkill(true);
            
            StartCoroutine(SkillCoolDown());
        }
    }

    private void DoMeleeSkill()
    {
        if (Input.GetKeyDown(upKey))
        {
            Debug.Log("위키눌림");
        }
        
        if (Input.GetKeyDown(downKey))
        {
            Debug.Log("아래키눌림");
        }
        
        if (Input.GetKeyDown(attackBKey))
        {
            Debug.Log("스킬키눌림" + isSkillCoolDown);
        }
        
        if (Input.GetKeyDown(upKey) && Input.GetKeyDown(downKey) && Input.GetKeyDown(attackBKey) && !isSkillCoolDown)
        {
            Debug.Log("근거리스킬 발동!");
            isSkillUsing = true;

            playerAnimator.SetBool("IsSkillUsing", isSkillUsing);

            UsingSkill(false);
            
            StartCoroutine(SkillCoolDown());
        }
        
        if (Input.GetKeyDown(upKey) && Input.GetKeyDown(downKey) && Input.GetKeyDown(attackAKey) && !isSkillCoolDown)
        {
            Debug.Log("원거리스킬 발동!");
            isSkillUsing = true;

            playerAnimator.SetBool("IsSkillUsing", isSkillUsing);

            UsingSkill(true);
            
            StartCoroutine(SkillCoolDown());
        }
    }

    private void UsingSkill(bool isRangeSkill)
    {
        // int randomIndex = Random.Range(0, MeleeSkills.Count);
        // Debug.Log((randomIndex + 1) + "번째 근거리 스킬");

        int randomIndex = 1;

        if (isRangeSkill)
        {
            if (randomIndex == 0)
            {
                Transform skillPosition = RangeSkillPositions[randomIndex];

                GameObject skillObj = Instantiate(RangeSkills[randomIndex], skillPosition);

                StartCoroutine(EffectTimer(RangeSkillTimes[randomIndex], skillObj));
            }
            else
            {
                Transform skillPosition = RangeSkillPositions[randomIndex];

                // skillPosition.position 및 skillPosition.rotation에서 위치와 회전 정보를 가져와 사용합니다.
                GameObject skillObj = Instantiate(RangeSkills[randomIndex], skillPosition.position, skillPosition.rotation);

                StartCoroutine(EffectTimer(RangeSkillTimes[randomIndex], skillObj));
            }
        }
        else
        {
            if (randomIndex == 0 || randomIndex == 2 || randomIndex == 3)
            {
                Transform skillPosition = MeleeSkillPositions[randomIndex];

                GameObject skillObj = Instantiate(MeleeSkills[randomIndex], skillPosition);

                StartCoroutine(EffectTimer(MeleeSkillTimes[randomIndex], skillObj));
            }
            else
            {
                Transform skillPosition = MeleeSkillPositions[randomIndex];

                // skillPosition.position 및 skillPosition.rotation에서 위치와 회전 정보를 가져와 사용합니다.
                GameObject skillObj = Instantiate(MeleeSkills[randomIndex], skillPosition.position, skillPosition.rotation);

                StartCoroutine(EffectTimer(MeleeSkillTimes[randomIndex], skillObj));
            }
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

    private IEnumerator TimeSlow()
    {
        yield return new WaitForSeconds(5f);

        isTimed = false;
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
        
        weaponCollider.gameObject.SetActive(false);
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

    private IEnumerator RestTimer(float startTime)
    {
        float maxRestDuration = 15f;

        while (Time.time - startTime < maxRestDuration)
        {
            yield return null;
        }

        isRest = false;
        canDash = true;
        canMove = true;
        
        playerAnimator.SetBool("IsResting", isRest);
    }

    private IEnumerator SkillCoolDown()
    {
        float skillAnimRate = 0.5f;
        isSkillCoolDown = true;
        isSkillUsing = false;
        
        yield return new WaitForSeconds(skillAnimRate);

        playerAnimator.SetBool("IsSkillUsing", isSkillUsing);
        
        yield return new WaitForSeconds(skillcoolDownRate - skillAnimRate);

        isSkillCoolDown = false;
    }

    private void UpdateHealth(float amount)
    {
        playerHpBar.value += amount;
    }

    private IEnumerator EffectTimer(float EffectTime, GameObject effectObj)
    {
        yield return new WaitForSeconds(EffectTime);
        
        Destroy(effectObj);
    }
}
