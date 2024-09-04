using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board1_Manager : CharInfo
{
    [SerializeField] private GameObject[] CharUI; // ĳ���� ���� UI
    [SerializeField] private Sprite[] CharLogo; // ĳ���� ��������Ʈ
    [SerializeField] private Sprite[] RankSp; // ���� ��������Ʈ
    [SerializeField] private Sprite[] PNoSp; // �÷��̾� ��ȣ ��������Ʈ
    [SerializeField] private Sprite[] ItemSp; // ������ ��������Ʈ
    [SerializeField] private Sprite[] NumSp; // ���� ǥ��� ���� ��������Ʈ

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
