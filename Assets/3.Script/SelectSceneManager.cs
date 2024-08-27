using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectSceneManager : MonoBehaviour
{
    private int cursorIndex = 0;
    private string[] Character_name = new string[16] 
    { "마리오", "루이지", "요시", "피치", "아미티", "라피나", "시그", "렘레스", 
        "팬텀", "메르세데스", "호영", "라라", "배찌", "다오", "디지니", "마리드" };
    [SerializeField] private GameObject cursor;
    [SerializeField] private Text char_name;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if ((cursorIndex % 8).Equals(0)) cursorIndex += 8;
            cursorIndex -= 1;
            CursorChange();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            cursorIndex += 1;
            if ((cursorIndex % 8).Equals(0)) cursorIndex -= 8;
            CursorChange();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            cursorIndex -= 8;
            if (cursorIndex < 0) cursorIndex += 16;
            CursorChange();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            cursorIndex += 8;
            if (cursorIndex > 15) cursorIndex -= 16;
            CursorChange();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackButton();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            NextButton();
        }
    }

    private void CursorChange()
    {
        cursor.GetComponent<RectTransform>().anchoredPosition = new Vector3((cursorIndex % 8) * 300 - 1050, 300 - (cursorIndex / 8) * 400 - (cursorIndex % 2) * 100, 0);
        char_name.text = Character_name[cursorIndex];
    }

    public void BackButton()
    {
        Debug.Log("뒤로 가기(아직 안 만듦)");
    }

    public void NextButton()
    {
        Debug.Log("확인(아직 안 만듦)");
    }
}
