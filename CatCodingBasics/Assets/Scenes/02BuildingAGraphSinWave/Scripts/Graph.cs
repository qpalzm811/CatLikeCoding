using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] 
    private Transform pointPrefab;

    [SerializeField, Range(10, 100)] private int resolution = 10;

    private Transform[] points;
    void Awake()
    {
        points = new Transform[resolution];
        
        // 步长，根据方块的数量，区间是 (-1,1)长度为2，
        // 除以方块数量，获得两个方块中心平均x坐标距离
        float step = 2f / resolution;
        
        Vector3 position = Vector3.zero;
        var scale = Vector3.one  * step;
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(pointPrefab, transform);
            // points[i] = point;
            
            // -1f是因为要限定 -1到1
            position.x = (i + 0.5f) * step - 1f;
            // position.y = functionY(position.x); //若要变化，则不设置固定y值
            
            point.localPosition = position;
            point.localScale = scale;
            
            // point.SetParent(transform, false);   // 这也可以设置父节点，但是低效
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.time;
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            // 每次刷新只改变y的值
            position.y = Mathf.Sin(Mathf.PI * (position.x + time));
            point.localPosition = position;
        }

    }

    private float functionY(float x)
    {
        return x * x * x;
    }
}
