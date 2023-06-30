using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// �Q�[���ɕK�v�ȏ����܂Ƃ߂�
/// �S�ẴV�[���ɂ��邱�ƑO��
/// </summary>
public class GameManager : MonoBehaviour
{
    static GameManager instance = null;
    PlayTime playTime;
    bool isGame = false;
    Asset.PlayerInputAction playerInput;
    Asset.PlayerInputAction.GameStateActions gameAction;

    public static GameManager Instance { get => instance; private set => instance = value; }
    public PlayTime PlayTime { get => playTime; private set => playTime = value; }

    private void Awake()
    {
        Instance = this;

        // �������m��
        playTime = new PlayTime();
        
        // InputSystem�̗L����
        playerInput = new Asset.PlayerInputAction();
        playerInput.Enable();

        gameAction = playerInput.GameState;
    }

    // Start is called before the first frame update
    void Start()
    {
        // �Q�[���V�[��������
        if(SceneManager.GetActiveScene().name == "Game")
        {
            isGame = true;

            // �f�[�^�Ǘ��̂��߂Ɏg�p(Entity����̃A�N�Z�X���l����)
            // ������
            PlayerPrefs.SetInt("Point", 0);
        }
    }

    private void OnDestroy()
    {
        // ������
        playerInput.Disable();
        playerInput.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        // �Q�[���I��
        if (gameAction.ESC.IsPressed())
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (!isGame)
        {
            return;
        }

        // �Q�[��������
        PlayTime.DelTime(Time.deltaTime);

        if(!playTime.IsPlaying())
        {
            // ���U���g��ʂ�

        }
    }

}
