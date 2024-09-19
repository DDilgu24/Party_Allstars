using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private int cursor = 0;
    [SerializeField] private RectTransform CursorPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) CursorChanged((cursor + 2) % 4);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) CursorChanged(cursor + 1 - (cursor % 2) * 2);
        if (Input.GetKeyDown(KeyCode.Return)) EnterMenu(cursor);
        if (Input.GetKeyDown(KeyCode.Escape)) EnterMenu(4);
    }

    public void CursorChanged(int newcursor)
    {
        cursor = newcursor;
        if (newcursor.Equals(0)) CursorPanel.anchoredPosition = new Vector3(-400, 200, 0);
        else if (newcursor.Equals(1)) CursorPanel.anchoredPosition = new Vector3(300, 200, 0);
        else if (newcursor.Equals(2)) CursorPanel.anchoredPosition = new Vector3(-300, -200, 0);
        else if (newcursor.Equals(3)) CursorPanel.anchoredPosition = new Vector3(400, -200, 0);
    }

    public void EnterMenu(int n)
    {
        if (n.Equals(1)) Debug.Log("멀티 플레이(미완따리)");
        else if (n.Equals(2)) Debug.Log("뮤지엄(미완따리)");
        else if (n.Equals(3)) Debug.Log("설정(미완따리)");
        else
        {
            string s = (n.Equals(0))? "3. Board Ready" : "1. TitleScreen";
            GameManager.instance.FadeOut(() =>
            {
                SceneManager.LoadScene(s);
                GameManager.instance.FadeIn();
            });
        }
    }

}
