using System.Collections;
using System.Collections.Generic;
using Demo_Project;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject startUIObject;
    public GameObject gameOverUIObject;

    public TextMeshProUGUI gameOverTitleText;

    public PlayerController player1;
    public PlayerController player2;

    public bool isGameOver = false;

    public void GameOverUI(bool isPlayer1Win)
    {
        if (isGameOver) return;
        
        gameOverUIObject.SetActive(true);

        if (isPlayer1Win)
        {
            gameOverTitleText.text = "플레이어1 승리!";
            
            Vector3 currentRotation = player1.transform.eulerAngles;
            player1.transform.eulerAngles = new Vector3(currentRotation.x, -90f, currentRotation.z);
            
            player1.playerAnimator.SetBool("IsWin", true);
            player2.playerAnimator.SetBool("IsDie", true);
        }
        else
        {
            gameOverTitleText.text = "플레이어2 승리!";
            
            Vector3 currentRotation = player2.transform.eulerAngles;
            player2.transform.eulerAngles = new Vector3(currentRotation.x, -90f, currentRotation.z);
            
            player2.playerAnimator.SetBool("IsWin", true);
            player1.playerAnimator.SetBool("IsDie", true);
        }
         
        isGameOver = true;
    }

    public void PauseGame()
    {
        
    }

    public void MoveToTitle()
    {
        
    }
    
}
