using System;
using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;
// using Vector3 = System.Numerics.Vector3;

public class GPUGraph : MonoBehaviour
{
    // ComputeShader
    [SerializeField]
    private ComputeShader computeShader;
    // 我们需要设置Compute Shader的一些属性
    // 这些标识符identifiers  自己定义，并在应用程序或编辑器运行期间保持不变
    private static readonly int
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time");
    
    
    [SerializeField]
    private Material material;
    [SerializeField]
    private Mesh mesh;
    
    [SerializeField, Range(10, 1000)]
    private int resolution = 10;
    
    // [SerializeField, Range(0, 1)] private int function;
    [SerializeField]
    private FunctionLibrary.FunctionName function;
    
    public enum TransitionMode { Cycle, Random, }
    [SerializeField]
    private TransitionMode transitionMode;
    
    // 每个函数持续的时间，0的话每帧都切换函数
    [SerializeField, Min(0f)] 
    private float functionDuration = 1f, transitionDuration = 1f;

    private float duration;
    
    bool transitioning;
    // 用于存储当前运行的功能 函数名
    FunctionLibrary.FunctionName transitionFunction;

    ComputeBuffer positionsBuffer;

    void OnEnable () {
        // 3个数xyz表坐标，4字节float
        positionsBuffer = new ComputeBuffer(resolution * resolution, 3*4);
    }

    private void OnDisable()
    {
        // release buffer 释放显存
        positionsBuffer.Release();
        positionsBuffer = null;
    }

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

        UpdateFunctionOnGPU();
    }
    // calculates the step size
    // sets the resolution, step, and time properties of the compute shader.
    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        // 不会拷贝数据，而是 链接buffer 到kernel
        computeShader.SetBuffer(0, positionsId, positionsBuffer);
        
        // 因为 1,1,1 是 8*8 的大小，所以要用分辨率除以8f
        int groups = Mathf.CeilToInt(resolution / 8f);  // 向上取整
        // 每个维度分别计算。如果在所有维度上都使用1，则意味着只会计算第一组8×8位置的值。
        // 如果调用多个内核函数，需要传索引，但是只有一个传0也行
        computeShader.Dispatch(0,groups,groups,1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        
        // 因为这种绘制方式不使用游戏对象，所以Unity不知道绘制发生在场景中的哪个位置。
        // 我们需要通过提供一个边界框作为额外参数来指示这一点。这是一个轴对齐的包围盒，表示我们要绘制的物体的空间边界。
        // Unity使用这个包围盒来确定绘制是否可以被跳过，视野外的被剔除这被称为视锥剔除。
        // 图形位于原点，并且点应该保持在大小为2的立方体内。
        // 通过调用Bounds构造方法并将Vector3.zero和Vector3.one缩放两倍作为参数来创建一个边界值。
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        // 2f + 2f / resolution 相应增加边界大小
        
        // 实例数量和其他属性已知并且与Mesh对象相关，则可以选择DrawMeshInstanced函数
            // CPU端指定要绘制的实例数量和位置，但是每个实例的其他属性必须在Mesh对象中预定义。
            // 因此，当需要绘制的实例具有相同的属性时，可以使用该函数。
        // 要动态地指定实例数量以及其他属性，则应该优先使用DrawMeshInstancedIndirect函数
            // 在GPU上调度实例渲染，而不需要从CPU发送每个实例的数据
        // 需要在CPU端指定实例数量和其他属性，则应该考虑使用DrawMeshInstancedProcedural函数
            // CPU不知道绘制在屏幕上的位置
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);
        // 五参提供是绘制多少个实例，应当吻合 positions buffer，通过.count属性传入
        
        // all points are drawn with a single draw call
    }
    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }
}
