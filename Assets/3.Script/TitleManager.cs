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
    [SerializeField] private Transform CharUISpawner; // ĳ���� �ΰ� ���� ��ġ��
    [SerializeField] private Sprite[] CharLogos;
    public PoolingManager poolingMng;
    public int[] charIndexOrder = new int[16]; // ĳ���� �ΰ� ���� ������ ���� �迭
    public int appearCount = 0; // �ΰ� ���� Ƚ��. �ѹ��� �� ������(16��) �ٽ� ����

    private void Start()
    {
        GameManager.instance.FadeIn();
        for (int i = 0; i < 16; i++) charIndexOrder[i] = i;
        IndexOrderShuffle();
        StartCoroutine(CharUIAppear());
        /*
        Sequence mySequence = DOTween.Sequence(); // ������ ����
        // 1-1. �ΰ� ������ ȿ��
        mySequence.Append(Title.transform.DOMoveY(0, 1.5f));
        // 1-2. �ؽ�Ʈ�� Ŀ���� �۾������ϴ� ȿ�� - 0.5�� ��� �ð� ����
        mySequence.Join(EnterToStart.transform.DOScale(1.25f, 1.5f).SetEase(Ease.Linear));
        mySequence.AppendInterval(0.5f);
        // 2. �ΰ� �Դٰ����ϴ� ȿ��
        mySequence.OnComplete(() =>
        {
            // EnterToStart�� ����ؼ� Ŀ���� �۾����� �ϴ� �ִϸ��̼����� ����
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
            }); // ���̵� �ƿ��� �θ��� ���ÿ�, �Ϸ�Ǹ� �� ��ȯ �� ���̵� ���� �ڵ����� ȣ��Ǵ� �ݹ� �Լ�
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
        charUI.transform.position = CharUISpawner.position + Vector3.right * (Random.Range(-800, 0) + (appearCount % 2) * 800); // ������ ��ǥ�� ����
        charUI.transform.DOLocalMoveY(1400, Random.Range(4f, 5f)).OnComplete(() => poolingMng.ReturnObjectToPool(charUI));
        yield return new WaitForSeconds(Random.Range(0.8f, 1.0f)); // ������
        if (++appearCount > 15) IndexOrderShuffle(); // 16 ĳ���� ��� ���������� ����
        StartCoroutine(CharUIAppear());
    }
}
