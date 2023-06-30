using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームに必要な情報をまとめる
/// 全てのシーンにあること前提
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

        // メモリ確保
        playTime = new PlayTime();
        
        // InputSystemの有効化
        playerInput = new Asset.PlayerInputAction();
        playerInput.Enable();

        gameAction = playerInput.GameState;
    }

    // Start is called before the first frame update
    void Start()
    {
        // ゲームシーン時処理
        if(SceneManager.GetActiveScene().name == "Game")
        {
            isGame = true;

            // データ管理のために使用(Entityからのアクセスを考えて)
            // 初期化
            PlayerPrefs.SetInt("Point", 0);
        }
    }

    private void OnDestroy()
    {
        // 無効化
        playerInput.Disable();
        playerInput.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        // ゲーム終了
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

        // ゲーム時処理
        PlayTime.DelTime(Time.deltaTime);

        if(!playTime.IsPlaying())
        {
            // リザルト画面へ

        }
    }

}
