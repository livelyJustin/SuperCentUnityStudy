using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraShake : MonoBehaviour
{
    [SerializeField] Transform camTr;
    [SerializeField] AnimationCurve animationCurve;

    Vector3 camYpos;
    bool isShake = false;
    float shakeTime = 0;

    public IEnumerator DoShake()
    {
        if (!isShake)
        {
            isShake = true;
            camYpos = camTr.localPosition;
            while (true)
            {
                yield return null;
                shakeTime += Time.deltaTime;
                if (shakeTime >= 1)
                {
                    isShake = false;
                    shakeTime = 0;
                    yield break;
                }
                camTr.localPosition = new Vector3(camTr.localPosition.x, camYpos.y + animationCurve.Evaluate(shakeTime), camTr.localPosition.z);
            }
        }
    }
}
