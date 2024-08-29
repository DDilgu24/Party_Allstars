using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharInfo
{
    int charIndex; // ĳ������ �ε��� (0~15) 
    int rank; // ���� ���� (1~4)
    int playerNO; // �÷��̾� ��ȣ (1~4: �÷��̾� / 5~8: CPU)
    int[] items = new int[3]; // ������ ������ 
    int score; // ���� [���� 4�� ��� HP]
}

public class CharUIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] CharUI; // ĳ���� ���� UI
    [SerializeField] private Sprite[] CharLogo; // ĳ���� ��������Ʈ
    [SerializeField] private Sprite[] RankSp; // ���� ��������Ʈ
    [SerializeField] private Sprite[] PNoSp; // �÷��̾� ��ȣ ��������Ʈ
    [SerializeField] private Sprite[] ItemSp; // ������ ��������Ʈ
    [SerializeField] private Sprite[] NumSp; // ���� ǥ��� ���� ��������Ʈ
}
