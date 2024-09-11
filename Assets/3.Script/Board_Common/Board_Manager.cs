using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;


public class Board_Manager : MonoBehaviour
{
    [SerializeField] public CinemachineVirtualCamera virtualCamera;
    [SerializeField] public Transform[] playerMarks; // 플레이어 말 오브젝트 배열

    [SerializeField] public GameObject[] CharUI; // 캐릭터 상태 UI
    [SerializeField] public GameObject TurnAlert, Intro, characterMark;
    [SerializeField] public Sprite[] DiceNum;
    [SerializeField] public Sprite[] playerNum;
    public int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // 순서를 정할 주사위 눈금
    public int[] order = new int[4] { 0, 0, 0, 0 }; // 순서. [3, 2, 1, 4] 이라면 3p > 2p > 1p > 4p 순서 임을 의미


    public Color[] playerColor = new Color[5] { Color.gray, Color.blue, Color.red, Color.green, Color.yellow }; // 플레이어 번호 별 컬러

    private int turns = 0; // 턴 수
    private int phase; // 현재 차례(0: 턴 초기 / 1~4: n번째 플레이어 / 5: 턴 종료(미니게임) / 6: 미니게임 결과(현재 상황))
    private bool co_in_update; // 업데이트 안의 코루틴이 동작 중인가?

    private IEnumerator Start()
    {
        // 1단계 : 인트로
        BD1SoundManager.instance.PlayBGM("Intro");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.one, 3f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 2f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(8.5f);
        // 2단계 : 게임 준비
        // 2-1. 게임 시작을 알리는 문구
        BD1SoundManager.instance.PlayBGM("BGM1");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("그럼 플레이할 순서를 정하겠습니다!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        // 2-2. 순서 정하기
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -1500), 0.5f).SetEase(Ease.Linear);
        for (int i = 1; i <= 4; i++)
        {
            yield return new WaitForSeconds(0.2f);
            characterMark.transform.Find($"{i}P_Dice").gameObject.SetActive(true);
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        HitDice(1);
        yield return new WaitUntil(() => !orderDecideNum.Any(value => value.Equals(0))); // 4명이 주사위 다 굴릴때까지 대기
        // 2-3. 순서 결정 및 UI 재배치
        DecisionOrder();
        yield return new WaitForSeconds(1f);
        // 2-4. 진짜 게임 시작 전
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("순서가 정해졌어요!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return null;
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("그럼 렛츠 파티!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        Intro.SetActive(false);
        print("게임 시작");
        for (int i = 1; i <= 4; i++)
        {
            characterMark.transform.Find($"{i}P_Dice").gameObject.SetActive(false);
        }
        virtualCamera.Priority = 11;
        turns = 1;
        phase = 1;
        // co_in_update = false;
        StartCoroutine(PlayerTurn_co());
    }
    /*
    private void Update()
    {
        if (turns < 1) return;
        // 0 : 턴 초기
        if(phase.Equals(0))
        {
            // 나중에 이벤트 추가(마지막 5턴 이벤트 같은 것...)하면 건드리기
            // 지금은 일단 패스
            phase++;
        }
        // 1~4 : 플레이어 턴
        else if(phase <= 4)
        {
            if(!co_in_update)
            {
                co_in_update = true;
                StartCoroutine(PlayerTurn_co());
            }
        }
        // 5 : 미니게임
        else if(phase.Equals(5))
        {
            // 지금은 일단 패스
            phase++;
        }
        // 6 : 중간 결과
        else
        {
            // 지금은 일단 패스 - 다음 턴으로
            turns++;
            phase = 0;
        }
    }
    */
    private IEnumerator PlayerTurn_co()
    {
        // co_in_update = false;
        int playerNo = order[phase - 1];
        // 1. 현재 차례인 캐릭터를 중심으로 카메라 이동
        SetCameraTarget(playerNo);
        // 2. 현재 차례가 누군지 알려주는 UI
        Vector3 v = new Vector3(1375, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[playerNo];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[playerNo];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[playerNo];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[playerNo - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        yield return null;
        v = new Vector3(-125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // 3. 스페이스 바 누르면 알림 UI 사라짐
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        v = new Vector3(-1500, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        print("일단은 여기까지");
        // 4. [일단 패스] 아이템 선택 받기
        // 5. 주사위 두드리기
        characterMark.transform.Find($"{playerNo}P_Dice").gameObject.SetActive(true);
        // 6. 캐릭터 이동
        // 7. [일단 패스] 멈춘 칸에 맞는 이벤트
    }



    private void HitDice(int p = 1)
    {
        characterMark.transform.Find($"{p}P").transform.DOLocalMoveY(3.5f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        Transform tf = characterMark.transform.Find($"{p}P_Dice");
        tf.GetChild(0).GetComponent<ParticleSystem>().Play();
        int r = Random.Range(1, 7);
        while(orderDecideNum.Any(value => value.Equals(r))) // 순서 정하기 이므로 중복을 배제
        {
            r = Random.Range(1, 7);
        }
        orderDecideNum[p - 1] = r;
        tf.GetComponent<Animator>().SetTrigger("WasHit");
        tf.GetComponent<Animator>().enabled = false;
        tf.GetChild(1).GetComponent<SpriteRenderer>().sprite = DiceNum[r];
        if (p.Equals(1))
        {
            StartCoroutine(ComHitDice(2));
            StartCoroutine(ComHitDice(3));
            StartCoroutine(ComHitDice(4));
        }
    }

    private IEnumerator ComHitDice(int p)
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 1.5f));
        HitDice(p);
    }

    private void DecisionOrder()
    {
        int index = 0;
        for (int i = 6; i > 0; i--)
        {
            for (int j = 0; j < 4; j++)
            {
                if (orderDecideNum[j].Equals(i)) 
                { 
                    order[index] = j + 1;
                    Vector3 newPos = new Vector3(300 * index - 800, 300 - 25 * (index % 2), 0);
                    CharUI[j].GetComponent<RectTransform>().DOAnchorPos(newPos, 1f).SetEase(Ease.OutQuad);
                    index++;
                    break; 
                }
            }
            if (index > 3) break;
        }
    }

    public void SetCameraTarget(int playerIndex)
    {
        // 플레이어 말 오브젝트의 Transform을 카메라의 타겟으로 설정
        Transform target = playerMarks[playerIndex - 1];
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }

    public void DebugAddScore(int pNo)
    {
        playerMarks[pNo].transform.DOMove(playerMarks[pNo].transform.position - Vector3.right * 7, 0.5f);
    }


}
