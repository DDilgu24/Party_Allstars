using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameObject Title;
    [SerializeField] private Text EnterToStart;

    private void Start()
    {
        GameManager.instance.FadeIn();
        /*
        Sequence mySequence = DOTween.Sequence(); // 시퀀스 생성
        // 1-1. 로고를 내리는 효과
        mySequence.Append(Title.transform.DOMoveY(0, 1.5f));
        // 1-2. 텍스트가 커졌다 작아졌다하는 효과 - 0.5초 대기 시간 적용
        mySequence.Join(EnterToStart.transform.DOScale(1.25f, 1.5f).SetEase(Ease.Linear));
        mySequence.AppendInterval(0.5f);
        // 2. 로고가 왔다갔다하는 효과
        mySequence.OnComplete(() =>
        {
            // EnterToStart를 계속해서 커졌다 작아졌다 하는 애니메이션으로 설정
            EnterToStart.transform.DOScale(1f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        });
        mySequence.Append(Title.transform.DOMoveY(100, 1.5f).SetEase(Ease.Linear));
        */
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && !GameManager.instance.isFading)
        {
            GameManager.instance.FadeOut(() =>
            {
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            }); // 페이드 아웃을 부름과 동시에, 완료되면 씬 전환 및 페이드 인이 자동으로 호출되는 콜백 함수
        }
    }
}
