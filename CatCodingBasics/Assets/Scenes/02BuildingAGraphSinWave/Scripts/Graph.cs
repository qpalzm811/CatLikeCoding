using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] private Transform pointPrefab;

    [SerializeField, Range(10, 100)] private int resolution = 10;
    
    // [SerializeField, Range(0, 1)] private int function;
    [SerializeField] private FunctionLibrary.FunctionName function;
    
    public enum TransitionMode { Cycle, Random, }
    [SerializeField] private TransitionMode transitionMode;
    
    
    private Transform[] points;
    
    // 每个函数持续的时间，0的话每帧都切换函数
    [SerializeField, Min(0f)] 
    private float functionDuration = 1f, transitionDuration = 1f;

    private float duration;
    
    bool transitioning;
    // 用于存储当前运行的功能 函数名
    FunctionLibrary.FunctionName transitionFunction;
    
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
    
    // Update is called once per frame
    void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            // 不能让过渡的状态一直持续，过渡一次之后要false
            // 并且记得减去持续时间
            if (duration >= transitionDuration) {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            // 难易精确到达 functionDuration这个时间，应该是每次超出一点
            // 所以保持每次切换时间都一致，所以减的数都一样是 functionDuration
            duration -= functionDuration;

            // 将要切换函数，存下现在运行的功能
            transitioning = true;
            transitionFunction = function;

            PickNextFunction();
        }

        if (transitioning) {
            UpdateFunctionTransition();
        }
        else {
            UpdateFunction();
        }
        
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }
    void UpdateFunction()
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
                // 给x和y分量各加上0.5。这一步是为了将采样位置移动到像素/纹素的中心，以减少采样误差。
                // (id.xy + 0.5) * _Step: 将结果乘以步长（_Step）参数，
                // 将整数坐标映射到对应的UV范围内。步长通常等于1除以纹理的宽度或高度。
                // 最后，将结果减去1.0，将UV坐标范围从[0, 1]调整到[-1, 0]。
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = f(u, v, time);
        }
    }
    // 过渡
    void UpdateFunctionTransition()
    {
        // 获取函数 上一个 下一个函数，progress是 持续时间 / 过渡时间
        FunctionLibrary.Function
            from = FunctionLibrary.GetFunction(transitionFunction),
            to = FunctionLibrary.GetFunction(function);
        float progress = duration / transitionDuration;
        
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
            points[i].localPosition = FunctionLibrary.Morph(
                u, v, time, from, to, progress
            );
        }
    }
}
