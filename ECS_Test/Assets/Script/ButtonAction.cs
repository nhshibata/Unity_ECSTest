using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

/// <summary>
/// UI操作によるシーン遷移
/// Timelineを使用する
/// ｱﾆﾒｰｼｮﾝ処理 -> Fade処理 -> 関数呼び出し（Scene遷移）
/// </summary>
public class ButtonAction : MonoBehaviour
{
    [SerializeField]TimelineAsset timelineAsset = null; // 再生するTimeline
    string loadSceneName = string.Empty;                // 読み込みシーン名
    PlayableDirector playableDirector = null;           // Timeline制御

    // Start is called before the first frame update
    void Start()
    {
        // 参照の取得
        playableDirector = GetComponent<PlayableDirector>();
        timelineAsset = playableDirector.playableAsset as TimelineAsset;

        // デリゲート設定
        // 終了時にシーン読み込みを行う
        playableDirector.stopped += TimelineStopped;
    }

    /// <summary>
    /// Timeline終了時処理
    /// </summary>
    /// <param name="obj"></param>
    private void TimelineStopped(PlayableDirector obj)
    {
        SceneLoad();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Button呼び出し
    /// </summary>
    public void GameStart()
    {
        PlayToScene("Game");
    }

    /// <summary>
    /// Button呼び出し
    /// </summary>
    public void GoTitle()
    {
        PlayToScene("Title");
    }

    private void PlayToScene(string scene)
    {
        loadSceneName = scene;
        // TimeLineの再生
        playableDirector.Play();
    }

    /// <summary>
    /// Button呼び出し
    /// </summary>
    public void GameQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Timeline呼び出し処理
    /// ロードを開始する
    /// </summary>
    public void SceneLoad()
    {

#if UNITY_EDITOR
        var scene = SceneManager.GetSceneByName(loadSceneName);
        if(scene == null)
        {
            Debug.LogError("SceneLoad" + loadSceneName + "がありません");
        }
#endif

        SceneManager.LoadSceneAsync(loadSceneName);
    }


}
