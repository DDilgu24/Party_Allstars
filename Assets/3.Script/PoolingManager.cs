using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    // 1단계: 풀 생성
    // 1-1. 풀링할 오브젝트의 프리팹을 지정
    public GameObject ObjectPrefab;
    // 1-2. 풀링의 개수 지정
    public int poolSize;
    // 1-3. 풀링을 관리할 큐를 생성
    public Queue<GameObject> ObjectPool = new Queue<GameObject>();
    // 1-4. 큐에 오브젝트들을 채우기
    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            // 1-4-1. 오브젝트 생성
            GameObject note_obj = Instantiate(ObjectPrefab);
            // 1-4-2. 시작할 땐 비활성화
            note_obj.SetActive(false);
            // 1-4.3. 큐에 추가
            ObjectPool.Enqueue(note_obj);
        }
    }

    // 2단계: 오브젝트를 풀에서 꺼낼 때 메서드
    public GameObject GetObjectFromPool()
    {
        // 예외 처리: 풀을 전부 사용해버린 경우
        if (ObjectPool.Count.Equals(0))
        {
            Debug.Log("!!! 풀링 개수를 초과함");
            return null;
        }
        // 2-1. 큐에서 오브젝트를 꺼내기
        GameObject note_obj = ObjectPool.Dequeue();
        // 2-2. 꺼낼 땐 활성화
        note_obj.SetActive(true);
        // 2-3. 꺼낸 오브젝트를 반환
        return note_obj;
    }

    // 3단계: 오브젝트를 풀에 반납하는 메서드
    public void ReturnObjectToPool(GameObject obj)
    {
        // 3-1. 넣을 땐 비활성화
        obj.SetActive(false);
        // 3-2. 다시 큐에 넣기
        ObjectPool.Enqueue(obj);
    }
}
