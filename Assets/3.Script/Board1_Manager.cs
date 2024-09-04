using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board1_Manager : CharInfo
{
    [SerializeField] private GameObject[] CharUI; // 캐릭터 상태 UI
    [SerializeField] private Sprite[] CharLogo; // 캐릭터 스프라이트
    [SerializeField] private Sprite[] RankSp; // 순위 스프라이트
    [SerializeField] private Sprite[] PNoSp; // 플레이어 번호 스프라이트
    [SerializeField] private Sprite[] ItemSp; // 아이템 스프라이트
    [SerializeField] private Sprite[] NumSp; // 점수 표기용 숫자 스프라이트

    private void Awake()
    {
        for (int i = 1; i <= 4; i++)
        {
            // CharUI[i].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharLogo[GameManager.instance.selectChar[i]];
        }
    }

    private IEnumerator Start()
    {
        BD1SoundManager.instance.PlayBGM("Intro");
        yield return new WaitForSeconds(10f);
        BD1SoundManager.instance.PlayBGM("BGM1");
    }
}
