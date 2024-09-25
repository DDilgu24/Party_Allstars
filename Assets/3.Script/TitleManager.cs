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
    [SerializeField] private Transform CharUISpawner; // 캐릭터 로고 스폰 위치용
    [SerializeField] private Sprite[] CharLogos;
    public PoolingManager poolingMng;
    public int[] charIndexOrder = new int[16]; // 캐릭터 로고가 나올 순서를 담을 배열
    public int appearCount = 0; // 로고가 나온 횟수. 한번씩 다 나오면(16번) 다시 셔플

    private void Start()
    {
        GameManager.instance.FadeIn();
        for (int i = 0; i < 16; i++) charIndexOrder[i] = i;
        IndexOrderShuffle();
        StartCoroutine(CharUIAppear());
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
        if (Input.GetKeyDown(KeyCode.Return) && !GameManager.instance.isFading)
        {
            GameManager.instance.FadeOut(() =>
            {
                DOTween.KillAll();
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            }); // 페이드 아웃을 부름과 동시에, 완료되면 씬 전환 및 페이드 인이 자동으로 호출되는 콜백 함수
        }
    }

    private void IndexOrderShuffle()
    {
        for (int i = 0; i < 16; i++)
        {
            int j = Random.Range(0, 16);
            int tmp = charIndexOrder[i];
            charIndexOrder[i] = charIndexOrder[j];
            charIndexOrder[j] = tmp;
        }
        appearCount = 0;
    }

    private IEnumerator CharUIAppear()
    {
        GameObject charUI = poolingMng.GetObjectFromPool();
        charUI.transform.Find("Mask/Character").GetComponent<Image>().sprite = CharLogos[charIndexOrder[appearCount]];
        charUI.transform.Find("Edge").GetComponent<Image>().color = new Color(Random.value, Random.value, Random.value);
        charUI.transform.position = CharUISpawner.position + Vector3.right * (Random.Range(-800, 0) + (appearCount % 2) * 800); // 스폰될 좌표를 설정
        charUI.transform.DOLocalMoveY(1400, Random.Range(4f, 5f)).OnComplete(() => poolingMng.ReturnObjectToPool(charUI));
        yield return new WaitForSeconds(Random.Range(0.8f, 1.0f)); // 딜레이
        if (++appearCount > 15) IndexOrderShuffle(); // 16 캐릭터 모두 등장했으면 셔플
        StartCoroutine(CharUIAppear());
    }
}
