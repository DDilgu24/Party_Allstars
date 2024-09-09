using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;

// [상태 패턴] 1단계: 게임 상태 인터페이스
public interface IGameState
{
    void EnterState(GameContext context); // 상태가 시작될 때
    void UpdateState(GameContext context); // 상태가 진행중일 때
    void ExitState(GameContext context); // 상태가 종료될 때
}

// [상태 패턴] 2단계: 게임 컨텍스트 클래스
public class GameContext : MonoBehaviour
{
    private IGameState _currentState;

    // 현재 상태를 설정하고 상태 전환을 처리합니다.
    public void SetState(IGameState newState)
    {
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    // 현재 상태를 업데이트합니다.
    private void Update()
    {
        _currentState?.UpdateState(this);
    }
}
/*
// [상태 패턴] 3단계: 상태별 구현
// 3-1. 순서 알림 상태
public class OrderAlertState : IGameState
{
    public void EnterState(GameContext context)
    {
        SetCameraTarget(order[Board1_Manager.orderPointer] - 1);
        // 알림 상태에서 필요한 초기화 작업 수행
    }

    public void UpdateState(GameContext context)
    {
        // 상태 업데이트 로직 (예: 플레이어 입력 처리, 몬스터 발견 등)

        // 예를 들어, 전투 상태로 전환하는 조건
        if (false)
        {
            // context.SetState(new CombatState());
        }
    }

    public void ExitState(GameContext context)
    {
        // 알림 상태 종료 시 작업 수행
    }
}
*/


public class Board1_Manager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform[] playerMarks; // 플레이어 말 오브젝트 배열

    public bool DieisLooping = true;
    [SerializeField] private GameObject[] CharUI; // 캐릭터 상태 UI
    [SerializeField] private GameObject Intro, characterMark;
    [SerializeField] private Sprite[] DiceNum;
    private int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // 순서를 정할 주사위 눈금
    private int[] order = new int[4] { 0, 0, 0, 0 }; // 순서. [3, 2, 1, 4] 이라면 3p > 2p > 1p > 4p 순서 임을 의미

    public GameContext gameContext;
    public static int orderPointer = 0; // 몇 번째 플레이어의 순서인가? (0~3 -> 1~4번째)

    private void Awake()
    {

    }

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
        // gameContext.SetState(new OrderAlertState());
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
        yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));
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
        Transform target = playerMarks[playerIndex];
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }

    public void DebugAddScore(int pNo)
    {
        playerMarks[pNo].transform.DOMove(playerMarks[pNo].transform.position - Vector3.right * 7, 0.5f);
    }
}
