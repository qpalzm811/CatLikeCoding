using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor.Search;

public class FrameRateCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    
    // 表示几秒范围内平均帧数，间隔 samleDuration 秒刷新一次
    [SerializeField, Range(0.1f, 2f)] private float sampleDuration = 1f;
    
    // Frame Durations
    // 当尝试达到目标帧率时，显示每帧持续时间更有用
    public enum DisplayMode
    {
        FPS, MS,
    }
    [SerializeField] private DisplayMode displayMode = DisplayMode.FPS; 
    private int frames;
    
    // 最佳持续时间设置为float.MaxValue，这是最坏的最佳持续时间
    float duration, bestDuration = float.MaxValue, worstDuration;
    
    Transform[] points;
    
    void Update()
    {
        UpdateFunction();
    }

    void UpdateFunction()
    {
        // 通常使用 Time.deltaTime 来控制游戏物体的移动、旋转、变换等操作
        // 对于涉及到物理模拟和游戏逻辑相关的操作，建议使用 Time.deltaTime
         
        // 不同于 Time.deltaTime，
        // 不受时间缩放影响的变量，表示上一帧和当前帧之间的真实时间（以秒为单位
        // 血条的渐变动画、粒子效果的生命周期等需要使用这个
        float frameDuration  = Time.unscaledDeltaTime;

        frames += 1;
        duration += frameDuration;
        
        if (frameDuration < bestDuration) {
            // 一帧运行的时间 比好的情况短 说明是更好的情况 作为分母小
            bestDuration = frameDuration;
        }
        if (frameDuration > worstDuration) {
            // 一帧运行的时间 比好的情况长 说明是更坏的情况 作为分母大
            worstDuration = frameDuration;
        }
        if (duration >= sampleDuration) {
            if (displayMode == DisplayMode.FPS)
            {
                display.SetText(
                    "FPS\n{0:0}\n{1:0}\n{2:0}",
                    1f / bestDuration,
                    frames / duration,
                    1f / worstDuration
                );
            }
            else
            {
                display.SetText(
                    "MS\n{0:1}\n{1:1}\n{2:1}",
                    1000f * bestDuration,
                    1000f * duration / frames,
                    1000f * worstDuration
                );
            }
            frames = 0;
            duration = 0f;
            bestDuration = float.MaxValue;
            worstDuration = 0f;
        }
    }
}
