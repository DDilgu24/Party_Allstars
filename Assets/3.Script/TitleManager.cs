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
    [SerializeField] private Transform charUISpawner; // 캐릭터 로고 스폰 위치용
    [SerializeField] private Sprite[] charLogos;
    public PoolingManager poolingManager;

    private int[] charIndexOrder = new int[16]; // 캐릭터 로고가 나올 순서를 담을 배열
    private int appearCount = 0; // 로고가 나온 횟수. 한번씩 다 나오면(16번) 다시 셔플

    private void Start()
    {
        GameManager.instance.FadeIn();
        InitCharIndexOrder();
        StartCoroutine(CharUIAppear());
    }

    private void Update()
    {
        InputHandler();
    }

    private void InitCharIndexOrder()
    {
        for (int i = 0; i < charIndexOrder.Length; i++)
        {
            charIndexOrder[i] = i;
        }
        ShuffleIndexOrder();
    }

    private void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !GameManager.instance.IsFading)
        {
            // 페이드 아웃을 부름과 동시에, 완료되면 씬 전환 및 페이드 인이 자동으로 호출되는 콜백 함수
            GameManager.instance.FadeOut(() =>
            {
                DOTween.KillAll();
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            });
        }
    }

    private void ShuffleIndexOrder()
    {
        for (int i = 0; i < charIndexOrder.Length; i++)
        {
            int j = Random.Range(0, charIndexOrder.Length);
            Swap(ref charIndexOrder[i], ref charIndexOrder[j]);
        }
        appearCount = 0;
    }

    private void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }

    private IEnumerator CharUIAppear()
    {
        GameObject charUI = poolingManager.GetObjectFromPool();
        SetCharUIProperties(charUI);
        charUI.transform.DOLocalMoveY(1400, Random.Range(4f, 5f)) // 오브젝트를 위로 올리고
            .OnComplete(() => poolingManager.ReturnObjectToPool(charUI)); // 다 올라갔으면 풀링 반환
        yield return new WaitForSeconds(Random.Range(0.8f, 1.0f)); // 0.8~ 1.0초 딜레이

        if (++appearCount > 15) ShuffleIndexOrder(); // 16 캐릭터 모두 등장했으면 셔플
        StartCoroutine(CharUIAppear()); // 재귀 호출
    }

    private void SetCharUIProperties(GameObject charUI)
    {
        charUI.transform.localScale = Vector3.one * Random.Range(0.6f, 1.0f); // 크기 랜덤하게
        charUI.transform.Find("Mask/Character").GetComponent<Image>().sprite = charLogos[charIndexOrder[appearCount]]; // 캐릭터 로고는 셔플된 인덱스대로
        charUI.transform.Find("Edge").GetComponent<Image>().color = new Color(Random.value, Random.value, Random.value); // 테두리 컬러는 올 랜덤
        charUI.transform.position = charUISpawner.position + Vector3.right * (Random.Range(-800, 0) + (appearCount % 2) * 800); // 스폰될 좌표를 설정
    }
}
