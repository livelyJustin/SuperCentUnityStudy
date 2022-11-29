using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
    double time = 0;
    void Start()
    {
        StartCoroutine(test1());
    }

    void Update()
    {
        time += Time.deltaTime;
    }
    public class TimeManager : MonoBehaviour
    {
        // void Start()
        // {
        //     System.Diagnostics.Stopwatch watch

        // }
    }

    IEnumerator test1()
    {
        watch.Start();
        yield return new WaitForSeconds(1);
        watch.Stop();
        Debug.Log(watch.ElapsedMilliseconds + " ms");
        // print("wfs: " + time);
        yield return new WaitForSeconds(1);
        // print("wfs222: " + time);
        StartCoroutine(test2());
        // watch.Reset();

    }

    IEnumerator test2()
    {
        watch.Restart();
        yield return new WaitForSecondsRealtime(1);
        watch.Stop();
        Debug.Log(watch.ElapsedMilliseconds + " ms");
        // print("wfsreal: " + time);
        yield return new WaitForSecondsRealtime(1);
        // print("wfsreal2222: " + time);
    }
}
