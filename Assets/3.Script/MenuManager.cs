using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnClickSingle()
    {
        SceneManager.LoadScene("3. Board Ready");
    }
    public void OnClickMulti()
    {
        Debug.Log("��Ƽ �÷���(�̿ϵ���)");
    }
    public void OnClickMuseum()
    {
        Debug.Log("������(�̿ϵ���)");
    }
    public void OnClickSetting()
    {
        Debug.Log("����(�̿ϵ���)");
    }
}
