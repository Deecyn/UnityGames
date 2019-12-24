using UnityEngine;
using System.Collections;

public class PacdotSpawner : MonoBehaviour
{
    // 初始界面豆豆的生成
    public GameObject pacdot;
    public float interval;
    public float startOffset;

    private float startTime;

    // Use this for initialization
    void Start()
    {
        startTime = Time.time + startOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > startTime + interval)
        {
            // 初始豆子的位置以及旋转前的初始角度
            // 做成prefabs以后可以反复实现
            GameObject obj = (GameObject)Instantiate(pacdot, transform.position, Quaternion.identity);
            obj.transform.parent = transform;

            startTime = Time.time;
        }
    }
}
