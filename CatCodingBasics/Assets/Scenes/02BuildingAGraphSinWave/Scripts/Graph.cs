using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] private Transform pointPrefab;

    [SerializeField, Range(10, 100)] private int resolution = 10;
    
    // [SerializeField, Range(0, 1)] private int function;
    [SerializeField] private FunctionLibrary.FunctionsName function;
    
    private Transform[] points;

    void Awake()
    {
        points = new Transform[resolution * resolution];
        
        // 步长，根据方块的数量，区间是 (-1,1)长度为2，
        // 除以方块数量，获得两个方块中心平均x坐标距离
        float step = 2f / resolution;
        var scale = Vector3.one * step;
        
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(pointPrefab, transform);
            // point.SetParent(transform, false);   // 这也可以设置父节点，但是低效

            point.localScale = scale;
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 获取函数
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;   // 当z变化的时候v才要变化，提出来初始化
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
            if (x == resolution) {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = f(u, v, time);
        }
        
        // 当xy点是恒定的时候，用x和y确定y位置的时候
        // for (int i = 0; i < points.Length; i++)
        // {
        //     Transform point = points[i];
        //     Vector3 position = point.localPosition;
        //     // 每次刷新只改变y的值
        //     position.y = f(position.x, position.z, time);
        //     point.localPosition = position;
        // }
    }
}
