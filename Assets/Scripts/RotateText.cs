using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RotateText : MonoBehaviour
{
    [SerializeField] float rotateAngles = 10f;
    [SerializeField] float rotateSpeed = 30f;

    bool isRotatingCCW = true;
    float rotation = 0f;

    TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        float addRot = Time.deltaTime * rotateSpeed;
        rotation += isRotatingCCW ? addRot : -addRot;
        text.rectTransform.rotation = Quaternion.Euler(0, 0, rotation);

        if (rotation > rotateAngles) { isRotatingCCW = false; }
        if (rotation < -rotateAngles) { isRotatingCCW = true; }
    }
}
