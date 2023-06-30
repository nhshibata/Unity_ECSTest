using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �|�C���g�\���X�V�p
/// </summary>
public class ScoreView : MonoBehaviour
{
    int destPoint;  // �ڕW�l
    int dispPoint;  // �\���l
    Text text = null;

    int DispPoint { get => dispPoint; 
        set
        {
            dispPoint = value;
            // �\�����̕ύX
            text.text = dispPoint.ToString();
        }
     }

    // Start is called before the first frame update
    void Start()
    {
        // 1�������Ă���Έُ�
        destPoint = PlayerPrefs.GetInt("Point", 1);

        // Text�ɃA�^�b�`���Ă���͂�
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
            // �\���̕ύX
            DispPoint = dist / (60 * 5);
        }
    }

}
