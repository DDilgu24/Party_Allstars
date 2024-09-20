using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Cinemachine;

public class Board_Manager : MonoBehaviour
{
    [SerializeField] public CinemachineVirtualCamera virtualCamera;
    [SerializeField] public Transform[] playerMarks; // 플레이어 말 오브젝트 배열
    [SerializeField] public Transform[] Spaces; // 발판 위치 배열

    [SerializeField] public GameObject[] CharUI; // 캐릭터 상태 UI
    [SerializeField] public GameObject TurnAlert, Intro;
    [SerializeField] public Sprite[] DiceNum;
    [SerializeField] public Sprite[] playerNum;
    public int[] orderDecideNum = new int[4] { 0, 0, 0, 0 }; // 순서를 정할 주사위 눈금
    public int[] order = new int[4] { 0, 0, 0, 0 }; // 순서. [3, 2, 1, 4] 이라면 3p > 2p > 1p > 4p 순서 임을 의미


    public Color[] playerColor = new Color[5] { Color.gray, Color.blue, Color.red, Color.green, Color.yellow }; // 플레이어 번호 별 컬러
    private Vector3[] playerOffset = new Vector3[5] { Vector3.zero, new Vector3(1f, 0, 0), new Vector3(-1f, 0, 0), new Vector3(0.5f, 0, 1f), new Vector3(-0.5f, 0, 1f) }; // 플레이어 번호 별 위치 오프셋

    private int turns = 0; // 턴 수
    private int phase; // 현재 차례(0: 턴 초기 / 1~4: n번째 플레이어 / 5: 턴 종료(미니게임) / 6: 미니게임 결과(현재 상황))
    private bool EndGame = false;

    private IEnumerator Start()
    {
        DOTween.Init();
        // 1단계 : 인트로
        BD1SoundManager.instance.PlayBGM("Intro");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.one, 3f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 2f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(8.5f);
        // 2단계 : 게임 준비
        // 2-1. 게임 시작을 알리는 문구
        BD1SoundManager.instance.PlayBGM("BGM1");
        Intro.transform.Find("BoardTitle").transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("그럼 플레이할 순서를 정하겠습니다!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        // 2-2. 순서 정하기
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -1500), 0.5f).SetEase(Ease.Linear);
        for (int i = 1; i <= 4; i++)
        {
            yield return new WaitForSeconds(0.2f);
            playerMarks[i - 1].GetChild(0).gameObject.SetActive(true); // 주사위 활성화 
        }
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        HitDice(1);
        yield return new WaitUntil(() => !orderDecideNum.Any(value => value.Equals(0))); // 4명이 주사위 다 굴릴때까지 대기
        // 2-3. 순서 결정 및 UI 재배치
        DecisionOrder();
        yield return new WaitForSeconds(1f);
        // 2-4. 진짜 게임 시작 전
        Intro.transform.Find("Toad").GetComponent<RectTransform>().DOAnchorPos(new Vector2(-900, -500), 0.5f).SetEase(Ease.Linear);
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("순서가 정해졌어요!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return null;
        Intro.transform.Find("Toad/Says").GetComponent<Text>().text = "";
        Intro.transform.Find("Toad/Says").GetComponent<Text>().DOText("그럼 렛츠 파티!", 0.5f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        Intro.SetActive(false);
        print("게임 시작");
        for (int i = 1; i <= 4; i++)
        {
            playerMarks[i - 1].GetChild(0).gameObject.SetActive(false);
        }
        virtualCamera.Priority = 11;
        turns = 1;
        phase = 1;
        // co_in_update = false;
        StartCoroutine(PlayerTurn_co());
    }

    private IEnumerator PlayerTurn_co()
    {
        Tween moveTween;
        int playerNo = order[phase - 1];
        int ifComThat0 = (playerNo > GameManager.instance.Player_Num)? 0 : 1;
        // 1. 현재 차례인 캐릭터를 중심으로 카메라 이동
        SetCameraTarget(playerNo);
        playerMarks[playerNo - 1].transform.position += 0.1f * Vector3.forward; // 현재 캐릭터를 앞으로 보이게?
        // 2. 현재 차례가 누군지 알려주는 UI
        Vector3 v = new Vector3(1375, -25, 0);
        TurnAlert.GetComponent<RectTransform>().anchoredPosition = v;
        TurnAlert.transform.Find("Center/Edge").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right").GetComponent<Image>().color = playerColor[playerNo * ifComThat0];
        TurnAlert.transform.Find("Right/PlayerNo").GetComponent<Image>().sprite = playerNum[playerNo * ifComThat0];
        TurnAlert.transform.Find("Center/Mask/Character").GetComponent<Image>().sprite = CharUI[playerNo - 1].transform.Find("Center/Mask/Character").GetComponent<Image>().sprite;
        yield return null;
        v = new Vector3(-125, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // 3. 스페이스 바 누르면 알림 UI 사라짐
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        v = new Vector3(-1500, -25, 0);
        TurnAlert.GetComponent<RectTransform>().DOAnchorPos(v, 0.5f).SetEase(Ease.OutQuad);
        // 4. [일단 패스] 아이템 선택 받기
        // 5. 주사위 두드리기
        yield return new WaitForSeconds(0.25f);
        playerMarks[playerNo - 1].GetChild(0).gameObject.SetActive(true); // 주사위 활성화 
        playerMarks[playerNo - 1].GetChild(0).gameObject.GetComponent<Animator>().enabled = true; // 주사위 애니도 활성화
        playerMarks[playerNo - 1].GetChild(0).gameObject.GetComponent<Animator>().SetBool("DiceStop", false); // 주사위를 다시 돌아가게
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        int move = HitDice(playerNo);
        // 6. 캐릭터 이동
        playerMarks[playerNo - 1].GetChild(0).GetChild(2).gameObject.SetActive(false); // 주사위 테두리 비활성화. 숫자만 보이게
        yield return new WaitForSeconds(0.5f);
        while (move > 0)
        {
            yield return new WaitForSeconds(0.25f);
            /*
            Vector3 oldpos = Spaces[CharInfoManager.instance.charinfo[playerNo - 1].score].position; // 현재 칸을 시작 좌표로
            CharInfoManager.instance.ScoreAdd(playerNo); // 점수를 1 더하기
            Vector3 newpos = Spaces[CharInfoManager.instance.charinfo[playerNo - 1].score].position; // 다음 칸을 도착 좌표로
            Vector3 midpos = (oldpos + newpos) * 0.5f; // 중간 좌표를 계산
            midpos.y = Mathf.Max(oldpos.y, newpos.y) + 2; // 중간 좌표의 y값을 보정
            Vector3[] movepath = new Vector3[] { oldpos, midpos, newpos };
            moveTween = playerMarks[playerNo - 1].transform.DOPath(movepath, 0.3f, PathType.CatmullRom).SetEase(Ease.Linear); // 캐릭터 말을 포물선으로 이동
            */

            CharInfoManager.instance.ScoreAdd(playerNo); // 점수를 1 더하기
            int newscore = CharInfoManager.instance.charinfo[playerNo - 1].score; // 바뀐 점수를 캐싱
            Vector3 newpos = Spaces[newscore].position + Vector3.forward * 0.1f; // 다음 칸을 도착 좌표로

            bool moveIsJump = false; // 다음 칸에 따라 캐릭터 이동 방식이 결정 (직선 OR 점프)
            if (newscore >= 23 && newscore != 30) moveIsJump = true;
            else if (4 <= newscore && newscore <= 11) moveIsJump = true; 

            if (moveIsJump) moveTween = playerMarks[playerNo - 1].transform.DOLocalJump(newpos, 2f, 1, 0.3f); // 캐릭터 말 이동 - 점프
            else moveTween = playerMarks[playerNo - 1].transform.DOMove(newpos, 0.3f).SetEase(Ease.Linear); // 캐릭터 말 이동 - 직선

            yield return moveTween.WaitForCompletion(); // 말 이동이 끝날 때까지 대기
            move--; // 남은 눈금 1 감소
            playerMarks[playerNo - 1].GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = DiceNum[move]; // 남은 눈금으로 이미지 변경
            if (CharInfoManager.instance.charinfo[playerNo - 1].score > 33) // 골대 도착하면
            {
                BD1SoundManager.instance.BGMPlayer.pitch = 1f;
                move = 0;
                EndGame = true;
                newpos += Vector3.down * 9.5f;
                BD1SoundManager.instance.StopBGM();
                BD1SoundManager.instance.PlaySFX("FlagDown");
                CharUI[0].transform.parent.GetComponent<RectTransform>().DOMoveY(1000, 0.5f); // 순위표 UI 치우기
                moveTween = playerMarks[playerNo - 1].transform.DOMove(newpos, 1.5f).SetEase(Ease.Linear); // 깃대 아래로 이동
                yield return moveTween.WaitForCompletion(); // 말 이동이 끝날 때까지 대기
                playerMarks[playerNo - 1].transform.DOMove(Spaces[35].position + Vector3.forward * 2, 0.5f).SetEase(Ease.Linear);
                BD1SoundManager.instance.PlaySFX("Victory");
            }
            else if (CharInfoManager.instance.charinfo[playerNo - 1].score >= 25) // 25점 도달하면
                BD1SoundManager.instance.BGMPlayer.pitch = 1.1f;
        }
        yield return new WaitForSeconds(0.25f);
        playerMarks[playerNo - 1].GetChild(0).GetChild(2).gameObject.SetActive(true); // 주사위 테두리 다시 활성화하고
        playerMarks[playerNo - 1].GetChild(0).gameObject.SetActive(false); // 주사위를 비활성화 
        // 7. [일단 패스] 멈춘 칸에 맞는 이벤트
        // 8. 다음 차례로
        yield return new WaitForSeconds(0.5f);
        if (EndGame) // 게임 끝났으면
        {
            yield return StartCoroutine(CharInfoManager.instance.ResultUISetting());
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
            GameManager.instance.FadeOut(() =>
            {
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            });
        }
        else // 아니면
        {
            phase++;
            if (phase > 4) { turns++; phase = 1; } // 이건 임시. 4번째 차례 끝나면 바로 1번째로
            StartCoroutine(PlayerTurn_co()); // 다음 플레이어로
        }
    }
    private int HitDice(int p)
    {
        Transform tf = playerMarks[p - 1].GetChild(0); // 주사위를 캐시
        playerMarks[p - 1].transform.DOLocalMoveY(playerMarks[p - 1].transform.position.y + 1.8f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad); // 캐릭터 점프 효과
        tf.transform.DOLocalMoveY(3 - 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad); // 주사위는 가만히 있도록 보이게
        tf.GetChild(0).GetComponent<ParticleSystem>().Play(); // 파티클 재생
        int r = UnityEngine.Random.Range(1, 7); // 주사위 범위: 1~6

        // 순서 정하기 단계인 경우
        if(turns.Equals(0))
        {
            while (orderDecideNum.Any(value => value.Equals(r))) // 순서 정하기 이므로 중복을 배제
            {
                r = UnityEngine.Random.Range(1, 7);
            }
            orderDecideNum[p - 1] = r;
            if (p.Equals(1))
            {
                StartCoroutine(ComHitDice(2));
                StartCoroutine(ComHitDice(3));
                StartCoroutine(ComHitDice(4));
            }
        }

        tf.GetComponent<Animator>().SetBool("DiceStop",true); // 주사위: 애니메이션을 정지하고 이미지를 결과값으로 변경 
        tf.GetComponent<Animator>().enabled = false;
        tf.GetChild(1).GetComponent<SpriteRenderer>().sprite = DiceNum[r];
        return r;
    }

    private IEnumerator ComHitDice(int p)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 1.5f));
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

}
