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
    [SerializeField] private Transform charUISpawner; // ĳ���� �ΰ� ���� ��ġ��
    [SerializeField] private Sprite[] charLogos;
    public PoolingManager poolingManager;

    private int[] charIndexOrder = new int[16]; // ĳ���� �ΰ��� ���� ������ ���� �迭
    private int appearCount = 0; // �ΰ��� ���� Ƚ��. �ѹ��� �� ������(16��) �ٽ� ����

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
            // ���̵� �ƿ��� �θ��� ���ÿ�, �Ϸ�Ǹ� �� ��ȯ �� ���̵� ���� �ڵ����� ȣ��Ǵ� �ݹ� �Լ�
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
        charUI.transform.DOLocalMoveY(1400, Random.Range(4f, 5f)) // ������Ʈ�� ���� �ø���
            .OnComplete(() => poolingManager.ReturnObjectToPool(charUI)); // �� �ö����� Ǯ�� ��ȯ
        yield return new WaitForSeconds(Random.Range(0.8f, 1.0f)); // 0.8~ 1.0�� ������

        if (++appearCount > 15) ShuffleIndexOrder(); // 16 ĳ���� ��� ���������� ����
        StartCoroutine(CharUIAppear()); // ��� ȣ��
    }

    private void SetCharUIProperties(GameObject charUI)
    {
        charUI.transform.localScale = Vector3.one * Random.Range(0.6f, 1.0f); // ũ�� �����ϰ�
        charUI.transform.Find("Mask/Character").GetComponent<Image>().sprite = charLogos[charIndexOrder[appearCount]]; // ĳ���� �ΰ��� ���õ� �ε������
        charUI.transform.Find("Edge").GetComponent<Image>().color = new Color(Random.value, Random.value, Random.value); // �׵θ� �÷��� �� ����
        charUI.transform.position = charUISpawner.position + Vector3.right * (Random.Range(-800, 0) + (appearCount % 2) * 800); // ������ ��ǥ�� ����
    }
}
