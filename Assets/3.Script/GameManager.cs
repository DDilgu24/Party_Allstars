using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // �÷��̾ ������ ĳ������ �ε���(-1: �̼���)
    public string[] Character_name = new string[16]
    { "������", "������", "���", "��ġ", "�ƹ�Ƽ", "���ǳ�", "�ñ�", "������",
        "����", "�޸�������", "ȣ��", "���", "�ٿ�", "����", "������", "������" };
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
