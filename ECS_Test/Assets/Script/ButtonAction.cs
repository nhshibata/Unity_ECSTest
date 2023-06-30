using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class ButtonAction : MonoBehaviour
{
    string loadSceneName = string.Empty;
    [SerializeField]TimelineAsset timelineAsset = null;
    PlayableDirector playableDirector = null;

    // Start is called before the first frame update
    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
        timelineAsset = playableDirector.playableAsset as TimelineAsset;

        playableDirector.stopped += PlayableDirector_stopped;
    }

    /// <summary>
    /// èIóπéûèàóù
    /// </summary>
    /// <param name="obj"></param>
    private void PlayableDirector_stopped(PlayableDirector obj)
    {
        SceneLoad();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameStart()
    {
        loadSceneName = "Game";

        // TimeLineÇÃçƒê∂
        playableDirector.Play();
    }

    public void GoTitle()
    {
        loadSceneName = "Title";
    }
    
    public void GameQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SceneLoad()
    {
        SceneManager.LoadSceneAsync(loadSceneName);
    }


}
