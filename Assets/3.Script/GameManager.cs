using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // �÷��̾ ������ ĳ������ �ε��� (-1: �̼���)
    public int PlayerNum { get; private set; }
    public int COMNum { get; private set; }
    public int TotalNum => PlayerNum + COMNum;

    public string[] Character_name = new string[16]
    {
        "������", "������", "���", "��ġ", "�ƹ�Ƽ", "���ǳ�", "�ñ�", "������",
        "����", "�޸�������", "ȣ��", "���", "�ٿ�", "����", "������", "������"
    };

    // ���� �Ŵ����� ���� ��� ������ ���̵� ����
    public Image fadeImage; // ���̵� �� �̹���
    public float fadeDuration = 0.8f; // ���̵� �ð�
    public bool IsFading { get; private set; } = true; // ���̵尡 �������ΰ�?


    // ���� �Ŵ��� �̱��� ����
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            fadeImage = transform.Find("Canvas/FadeImage").GetComponent<Image>();
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }

    private IEnumerator Fade(bool isIn, System.Action onComplete)
    {
        fadeImage.gameObject.SetActive(true);
        IsFading = true;

        float elapsedTime = 0f; // ���̵尡 ����� �ð� 
        Color color = fadeImage.color; // ���̵� �̹����� ���� ĳ��
        float endAlpha = isIn? 0 : 1; // ���̵�� �̹����� ���� ������ ��(0: ����)

        while (elapsedTime < fadeDuration) // ������ ����
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1 - endAlpha, endAlpha, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color; // Ȯ���� ������ ���� ���� �����ϰ� ����
        IsFading = false;
        fadeImage.gameObject.SetActive(false);
        onComplete?.Invoke(); // ���� �Ϸ� �� ���� �� �ݹ� �Լ��� ȣ��
    }

    public void SelectInfo(int playerNum, int comNum, int boardNo, int[] character)
    {
        PlayerNum = playerNum;
        COMNum = comNum;
        selectChar = character;
    }

    public void FadeIn(System.Action onComplete = null)
    {
        StartCoroutine(Fade(true, onComplete));
    }

    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(Fade(false, onComplete));
    }

}
