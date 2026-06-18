using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换管理器 主要用于切换场景
/// </summary>
public class SceneMgr : BaseSingleton<SceneMgr>
{
    private SceneMgr() { }

    //同步切换场景的方法
    public void LoadScene(string sceneName, UnityAction callback = null)
    {
        SceneManager.LoadScene(sceneName);
        callback?.Invoke();
        callback = null;
    }

    //异步切换场景的方法
    public void LoadSceneAsync(string sceneName, UnityAction callback = null)
    {
        MonoMgr.Instance.StartCoroutine(RealLoadSceneAsync(sceneName, callback));
    }
    private IEnumerator RealLoadSceneAsync(string sceneName, UnityAction callback)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        //不停的在协同程序中每帧检测是否加载结束 如果加载结束就不会进这个循环每帧执行了
        while (!ao.isDone) 
        {
            //可以在这里利用事件中心 每一帧将进度发送给想要得到的地方
            EventCenter.Instance.EventTrigger<float>(E_EventType.E_UpdateSceneLoad, ao.progress);
            yield return 0;
        }
        //避免最后一帧直接结束了 没有同步100%出去
        EventCenter.Instance.EventTrigger<float>(E_EventType.E_UpdateSceneLoad, 1.0f);

        callback?.Invoke();
        callback = null;
    }
}
