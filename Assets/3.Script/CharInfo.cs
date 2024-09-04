using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharInfo : MonoBehaviour
{
    [SerializeField] private int playerNO; // �÷��̾� ��ȣ (1~4: �÷��̾� / 5~8: CPU)
    private int charIndex; // ĳ������ �ε��� (0~15) 
    private int rank; // ���� ���� (1~4)
    private int[] items = new int[3]; // ������ ������ 
    private int score; // ���� [���� 4�� ��� HP]

    private void Awake()
    {
        charIndex = GameManager.instance.selectChar[playerNO];
        rank = 1;
        score = 0;
    }
}
