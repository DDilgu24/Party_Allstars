using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharInfo
{
    int charIndex; // 캐릭터의 인덱스 (0~15) 
    int rank; // 현재 순위 (1~4)
    int playerNO; // 플레이어 번호 (1~4: 플레이어 / 5~8: CPU)
    int[] items = new int[3]; // 보유한 아이템 
    int score; // 점수 [보드 4의 경우 HP]
}

public class CharUIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] CharUI; // 캐릭터 상태 UI
    [SerializeField] private Sprite[] CharLogo; // 캐릭터 스프라이트
    [SerializeField] private Sprite[] RankSp; // 순위 스프라이트
    [SerializeField] private Sprite[] PNoSp; // 플레이어 번호 스프라이트
    [SerializeField] private Sprite[] ItemSp; // 아이템 스프라이트
    [SerializeField] private Sprite[] NumSp; // 점수 표기용 숫자 스프라이트
}
