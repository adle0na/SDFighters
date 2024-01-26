using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public UserType userType;

    private float walkSpeed = 1;
    private float runSpeed = 5;
    
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCapsuleCollider;

    private bool isMoving = false;
    private bool isDashing = false;
    private bool isJumping = false;

    private bool canDash = false;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerCapsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (userType == UserType.Player1)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                isMoving = true;
                Debug.Log("A or D 방향키 입력");
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                isJumping = true;
                Debug.Log("H 점프키 입력");
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                isMoving = true;
                Debug.Log("왼쪽 or 오른쪽 키입력");
            }
            
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                isJumping = true;
                Debug.Log("6 점프키 입력");
            }
        }

        playerAnimator.SetBool("IsMoving", isMoving);
        playerAnimator.SetBool("IsJumping", isJumping);
    }
}
