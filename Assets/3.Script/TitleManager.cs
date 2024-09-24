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
    // public PoolingManager poolingMng;


    private void Start()
    {
        GameManager.instance.FadeIn();
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
        if(Input.GetKeyDown(KeyCode.Return) && !GameManager.instance.isFading)
        {
            GameManager.instance.FadeOut(() =>
            {
                SceneManager.LoadScene("2. Main Menu");
                GameManager.instance.FadeIn();
            }); // ���̵� �ƿ��� �θ��� ���ÿ�, �Ϸ�Ǹ� �� ��ȯ �� ���̵� ���� �ڵ����� ȣ��Ǵ� �ݹ� �Լ�
        }
    }
    /*
    private IEnumerator CharUIAppear()
    {
        GameObject charUI = poolingMng.GetObjectFromPool();
        charUI.transform.position = CharUISpawner.position + Vector3.right * Random.Range(-600, 600); // ������ ��ǥ�� ����
        charUI.transform.SetParent(this.transform);
        charUI.transform.DOLocalMoveY(700, 3.0f);
        yield return null;
    }
    */
}
