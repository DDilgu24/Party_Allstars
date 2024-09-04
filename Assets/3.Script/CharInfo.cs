using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharInfo : MonoBehaviour
{
    [SerializeField] private int playerNO; // 플레이어 번호 (1~4: 플레이어 / 5~8: CPU)
    private int charIndex; // 캐릭터의 인덱스 (0~15) 
    private int rank; // 현재 순위 (1~4)
    private int[] items = new int[3]; // 보유한 아이템 
    private int score; // 점수 [보드 4의 경우 HP]

    private void Awake()
    {
        charIndex = GameManager.instance.selectChar[playerNO];
        rank = 1;
        score = 0;
    }
}
