using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // 플레이어별 선택한 캐릭터의 인덱스(-1: 미선택)
    public string[] Character_name = new string[16]
    { "마리오", "루이지", "요시", "피치", "아미티", "라피나", "시그", "렘레스",
        "팬텀", "메르세데스", "호영", "라라", "다오", "배찌", "디지니", "마리드" };
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
