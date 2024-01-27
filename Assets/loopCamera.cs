using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loopCamera : MonoBehaviour
{
    public Transform startPoint; // 시작 지점 설정
    public float moveSpeed = 5f; // 이동 속도 설정

    void Update()
    {
        MoveUntilTrigger();
    }

    void MoveUntilTrigger()
    {
        Vector3 direction = transform.forward; // 전진 방향 설정

        // Ray를 사용하여 트리거 감지
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity) && hit.collider.CompareTag("CameraWall"))
        {
            Debug.Log("CameraWall에 도달!");

            // 트리거 감지 후, 시작 지점으로 이동
            MoveToStartPoint();
        }
        else
        {
            // 트리거가 없으면 계속해서 이동
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }

    void MoveToStartPoint()
    {
        // 시작 지점으로 이동
        transform.position = startPoint.position;
    }

    void OnTriggerEnter(Collider other)
    {
        // 트리거에 진입할 때 호출되는 함수
        if (other.CompareTag("CameraWall"))
        {
            Debug.Log("CameraWall에 도달!");

            MoveToStartPoint();
        }
    }
}
