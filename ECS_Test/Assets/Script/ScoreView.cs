using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ポイント表示更新用
/// リザルトにも使用
/// </summary>

//[RequireComponent(typeof(Text))]
public class ScoreView : MonoBehaviour
{
    int nDispPoint = 0;  // 表示値
    Text pText = null;

    int DispPoint { 
        get => nDispPoint; 
        set
        {
            nDispPoint = value;
            // 表示数の変更
            pText.text = nDispPoint.ToString();
        }
     }

    // Start is called before the first frame update
    void Start()
    {
        // Textにアタッチしているはず
        pText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        // 1が入っていれば異常
        int destPoint = PlayerPrefs.GetInt("Point", 1);

        if (DispPoint > destPoint)
        {
            DispPoint = destPoint;
        }
        else
        {
            int dist = destPoint - DispPoint;
            dist = dist / (60 * 5);

            // 表示の変更
            DispPoint = DispPoint + dist;
        }
    }

}
