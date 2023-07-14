using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �|�C���g�\���X�V�p
/// ���U���g�ɂ��g�p
/// </summary>

//[RequireComponent(typeof(Text))]
public class ScoreView : MonoBehaviour
{
    int nDispPoint = 0;  // �\���l
    Text pText = null;

    int DispPoint { 
        get => nDispPoint; 
        set
        {
            nDispPoint = value;
            // �\�����̕ύX
            pText.text = nDispPoint.ToString();
        }
     }

    // Start is called before the first frame update
    void Start()
    {
        // Text�ɃA�^�b�`���Ă���͂�
        pText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        // 1�������Ă���Έُ�
        int destPoint = PlayerPrefs.GetInt("Point", 1);

        if (DispPoint > destPoint)
        {
            DispPoint = destPoint;
        }
        else
        {
            int dist = destPoint - DispPoint;
            dist = dist / (60 * 5);

            // �\���̕ύX
            DispPoint = DispPoint + dist;
        }
    }

}
