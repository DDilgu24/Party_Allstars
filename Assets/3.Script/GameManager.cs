using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int[] selectChar = new int[5] { -1, -1, -1, -1, -1 }; // 플레이어별 선택한 캐릭터의 인덱스(-1: 미선택)
    public int Player_Num, COM_Num; // 플레이어 수, COM 수
    public string[] Character_name = new string[16]
    { "마리오", "루이지", "요시", "피치", "아미티", "라피나", "시그", "렘레스",
        "팬텀", "메르세데스", "호영", "라라", "다오", "배찌", "디지니", "마리드" };

    // 게임 매니져 싱글톤 적용
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


    // 게임 매니져를 통해 모든 씬에서 페이드 관리
    public Image fadeImage; // 페이드 용 이미지
    public float fadeDuration; // 페이드 시간
    public bool isFading = true; // 페이드가 진행중인가?

    private IEnumerator Fade(bool isIn, System.Action onComplete, float fadeDuration = 0.5f)
    {
        isFading = true;
        float elapsedTime = 0f; // 페이드가 진행된 시간 
        Color color = fadeImage.color; // 페이드 이미지의 색상 캐싱
        float endAlpha = (isIn) ? 0 : 1; // 페이드용 이미지의 최종 투명도 값(0: 투명)

        while (elapsedTime < fadeDuration) // 투명도 조절
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1 - endAlpha, endAlpha, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color; // 확실히 정해진 알파 값에 도달하게 보정
        isFading = false;
        onComplete?.Invoke(); // 동작 완료 후 전달 된 콜백 함수를 호출
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
