using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroy : MonoBehaviour
{
    private void Awake()
    {
        GameObject[] musicObj = GameObject.FindGameObjectsWithTag("GameMusic");

        if (musicObj.Length > 1)
        {
            // 현재 게임 오브젝트가 배열 중 첫 번째 인덱스가 아닌 경우 파괴
            if (musicObj[0] != this.gameObject)
            {
                Destroy(this.gameObject);
                return;
            }
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
