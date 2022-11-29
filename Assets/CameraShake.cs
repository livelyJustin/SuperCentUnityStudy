using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraShake : MonoBehaviour
{
    [SerializeField] Transform camTr;
    [SerializeField] AnimationCurve animationCurve;
    [SerializeField] Transform playertr;
    Vector3 camYpos;
    bool isShake = false;
    float shakeTime = 0;
    // float Time2 = 0;

    // private void Start()
    // {
    //     playertr
    // }

    private void Update()
    {
        float x = (playertr.position.x - transform.position.x) + transform.position.x;
        // print("x: " + (playertr.position.x - transform.position.x));
        float z = (playertr.position.z - transform.position.z) + transform.position.z + -2f;
        // print("z: " + (playertr.position.z - transform.position.z));
        transform.position = new Vector3(x, transform.position.y, z);
    }
    // // private void Update()
    // // {
    // //     Time2 += Time.deltaTime;
    // //     print("업데이트: " + Time2);
    // // }
    // // public IEnumerator Test()
    // // {
    // //     yield return new WaitForSeconds(1);
    // //     print("코루틴: " + Time2);
    // // }
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
