using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // �÷��̾ ������ ĳ������ �ε���(-1: �̼���)
    public int Player_Num, COM_Num; // �÷��̾� ��, COM ��
    public string[] Character_name = new string[16]
    { "������", "������", "���", "��ġ", "�ƹ�Ƽ", "���ǳ�", "�ñ�", "������",
        "����", "�޸�������", "ȣ��", "���", "�ٿ�", "����", "������", "������" };

    // ���� �Ŵ��� �̱��� ����
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(this.gameObject); }
        fadeImage = transform.Find("Canvas/FadeImage").GetComponent<Image>();
    }


    // ���� �Ŵ����� ���� ��� ������ ���̵� ����
    public Image fadeImage; // ���̵� �� �̹���
    public float fadeDuration; // ���̵� �ð�
    public bool isFading = true; // ���̵尡 �������ΰ�?

    private IEnumerator Fade(bool isIn, System.Action onComplete, float fadeDuration = 0.5f)
    {
        isFading = true;
        float elapsedTime = 0f; // ���̵尡 ����� �ð� 
        Color color = fadeImage.color; // ���̵� �̹����� ���� ĳ��
        float endAlpha = (isIn) ? 0 : 1; // ���̵�� �̹����� ���� ���� ��(0: ����)

        while (elapsedTime < fadeDuration) // ���� ����
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1 - endAlpha, endAlpha, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color; // Ȯ���� ������ ���� ���� �����ϰ� ����
        isFading = false;
        onComplete?.Invoke(); // ���� �Ϸ� �� ���� �� �ݹ� �Լ��� ȣ��
    }

    public void SelectInfo(int P_num, int C_num, int boardNO, int[] Character)
    {
        Player_Num = P_num;
        COM_Num = C_num;
        selectChar = Character;
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
