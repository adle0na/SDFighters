using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float minDistance = 5f;
    public float maxDistance = 15f;
    public float lerpSpeed = 5f; // 조절할 값
    public Vector3 minCameraPosition = new Vector3(-30.89f, 2.65f, 18f);
    public Vector3 maxCameraPosition = new Vector3(-36.49f, 5.89f, -18f);

    void Update()
    {
        // 플레이어 1과 플레이어 2 간의 거리 계산
        float distance = Vector3.Distance(player1.position, player2.position) * 2f;

        // 거리를 기반으로 카메라의 위치 조절
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        Vector3 newCameraPosition = Vector3.Lerp(minCameraPosition, maxCameraPosition, t);

        // 카메라 위치를 부드럽게 업데이트
        transform.position = Vector3.Lerp(transform.position, newCameraPosition, Time.deltaTime * lerpSpeed);
    }
}
