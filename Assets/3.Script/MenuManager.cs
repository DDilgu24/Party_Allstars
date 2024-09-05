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
        Debug.Log("멀티 플레이(미완따리)");
    }
    public void OnClickMuseum()
    {
        Debug.Log("뮤지엄(미완따리)");
    }
    public void OnClickSetting()
    {
        Debug.Log("설정(미완따리)");
    }
}
