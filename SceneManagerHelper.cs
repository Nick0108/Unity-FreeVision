using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagerHelper : MonoBehaviour, ISingleton
{
    //本来想用静态类，并扩展SceneManager的，但是由于StartCoroutine必须继承MonoBehaviour，而MonoBehaviour不能被非静态类继承，所以只能改为单例模式
    #region 单例模式Singleton
    private SceneManagerHelper() { }
    static SceneManagerHelper() { }
    private static readonly SceneManagerHelper _instance = new SceneManagerHelper();
    public static SceneManagerHelper Instance { get { return _instance; } }
    #endregion
    public void Init()
    {

    }

    public enum SceneEnum
    {
        GameStart = 0,
        Previewing = 1,
        AsyncLoadExhibition = 2,
        Exhibiting = 3
    }

    public AsyncOperation AsynLoadState;

    public void LoadGameScene(SceneEnum scene, bool IsNeedAsy = false)
    {
        if (IsNeedAsy)
        {
            AsynLoadState = SceneManager.LoadSceneAsync((int)scene);
            StartCoroutine(LoadScene());
        }
        else
        {
            SceneManager.LoadScene((int)scene);
        }
        
    }

    IEnumerator LoadScene()
    {
        yield return null;
        AsynLoadState.allowSceneActivation = false;
        Debug.Log("Pro :" + AsynLoadState.progress);
        while (!AsynLoadState.isDone)
        {
            //m_Text.text = "Loading progress: " + (AsynLoadState.progress * 100) + "%";
            if (AsynLoadState.progress >= 0.9f)
            {
                //m_Text.text = "Press the space bar to continue";
                if (Input.GetKeyDown(KeyCode.Space))
                    AsynLoadState.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
