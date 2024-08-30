using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public CanvasGroup canvasGroup; 
    public float fadeDuration = 1f; // ���̵� �ð�
    float elapsedTime = 0f; // ���� ���ӽð�

    private void Start()
    {
        StartCoroutine(Fade(true));
    }

    private IEnumerator Fade(bool isFadeIn)
    {
        elapsedTime = 0f; // ���� ���ӽð�
        fadeDuration = (isFadeIn) ? 2f : 1f; // ���̵� �ð�
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            if(isFadeIn) canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            else canvasGroup.alpha = Mathf.Clamp01(1 - elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = (isFadeIn)? 1f : 0f; // ���������� ������ ������ �Ǵ� �����ϰ� ����
        if (!isFadeIn) SceneManager.LoadScene("2. Main Menu");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && canvasGroup.alpha == 1f)
        {
            StartCoroutine(Fade(false));
        }
    }
}
