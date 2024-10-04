using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private int cursor = 0;
    [SerializeField] private RectTransform CursorPanel;

    private void Update()
    {
        InputHandler();
    }

    private void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            CursorChanged((cursor + 2) % 4);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            CursorChanged(cursor + 2 % 4);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            CursorChanged(cursor - 1 < 0 ? 3 : cursor - 1);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            CursorChanged((cursor + 1) % 4);

        else if (Input.GetKeyDown(KeyCode.Return) && !GameManager.instance.IsFading)
            EnterMenu(cursor);
        else if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.instance.IsFading)
            EnterMenu(4);
    }

    public void CursorChanged(int newCursor)
    {
        cursor = newCursor;
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        Vector3 newPosition = cursor switch
        {
            0 => new Vector3(-400, 200, 0),
            1 => new Vector3(300, 200, 0),
            2 => new Vector3(-300, -200, 0),
            3 => new Vector3(400, -200, 0),
            _ => CursorPanel.anchoredPosition // 기본값
        };
        CursorPanel.anchoredPosition = newPosition;
    }

    public void EnterMenu(int menuIndex)
    {
        switch (menuIndex)
        {
            case 1:
                Debug.Log("네트워크(미완따리)");
                break;
            case 2:
                Debug.Log("뮤지엄(미완따리)");
                break;
            case 3:
                Debug.Log("설정(미완따리)");
                break;
            default:
                LoadScene(menuIndex);
                break;
        }
    }

    private void LoadScene(int menuIndex)
    {
        string sceneName = (menuIndex == 0) ? "3. Board Ready" : "1. TitleScreen";
        GameManager.instance.FadeOut(() =>
        {
            SceneManager.LoadScene(sceneName);
            GameManager.instance.FadeIn();
        });
    }
}
