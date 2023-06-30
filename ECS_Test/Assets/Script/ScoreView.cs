using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ポイント表示更新用
/// </summary>
public class ScoreView : MonoBehaviour
{
    int destPoint;  // 目標値
    int dispPoint;  // 表示値
    Text text = null;

    int DispPoint { get => dispPoint; 
        set
        {
            dispPoint = value;
            // 表示数の変更
            text.text = dispPoint.ToString();
        }
     }

    // Start is called before the first frame update
    void Start()
    {
        // 1が入っていれば異常
        destPoint = PlayerPrefs.GetInt("Point", 1);

        // Textにアタッチしているはず
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(DispPoint > destPoint)
        {
            DispPoint = destPoint;
        }
        else
        {
            int dist = destPoint - DispPoint;
            // 表示の変更
            DispPoint = dist / (60 * 5);
        }
    }

}
