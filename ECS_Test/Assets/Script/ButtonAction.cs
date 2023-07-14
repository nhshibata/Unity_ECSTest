using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

/// <summary>
/// UI����ɂ��V�[���J��
/// Timeline���g�p����
/// ��Ұ��ݏ��� -> Fade���� -> �֐��Ăяo���iScene�J�ځj
/// </summary>
public class ButtonAction : MonoBehaviour
{
    [SerializeField]TimelineAsset timelineAsset = null; // �Đ�����Timeline
    string loadSceneName = string.Empty;                // �ǂݍ��݃V�[����
    PlayableDirector playableDirector = null;           // Timeline����

    // Start is called before the first frame update
    void Start()
    {
        // �Q�Ƃ̎擾
        playableDirector = GetComponent<PlayableDirector>();
        timelineAsset = playableDirector.playableAsset as TimelineAsset;

        // �f���Q�[�g�ݒ�
        // �I�����ɃV�[���ǂݍ��݂��s��
        playableDirector.stopped += TimelineStopped;
    }

    /// <summary>
    /// Timeline�I��������
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
    /// Button�Ăяo��
    /// </summary>
    public void GameStart()
    {
        PlayToScene("Game");
    }

    /// <summary>
    /// Button�Ăяo��
    /// </summary>
    public void GoTitle()
    {
        PlayToScene("Title");
    }

    private void PlayToScene(string scene)
    {
        loadSceneName = scene;
        // TimeLine�̍Đ�
        playableDirector.Play();
    }

    /// <summary>
    /// Button�Ăяo��
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
    /// Timeline�Ăяo������
    /// ���[�h���J�n����
    /// </summary>
    public void SceneLoad()
    {

#if UNITY_EDITOR
        var scene = SceneManager.GetSceneByName(loadSceneName);
        if(scene == null)
        {
            Debug.LogError("SceneLoad" + loadSceneName + "������܂���");
        }
#endif

        SceneManager.LoadSceneAsync(loadSceneName);
    }


}
