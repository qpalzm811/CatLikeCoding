using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.Mathf;
public static class FunctionLibrary
{
    public delegate Vector3 Function(float u, float v, float time);
    public enum FunctionName
    {
        Wave, MultiWave, Ripple,
        CylinderWithCollapsingRadius,
        StaticSphere, DynamicSphere,
        VerticalBandsSphere,
        VerticalBandsDynamicSphere,
        HorizontalBandsSphere,
        HorizontalBandsDynamicSphere,
        TwistedSphere,
        SelfIntersectingSpindleTorus,
        Torus,
        StaticTorus,
        StarTwistingTorus,
        
    }
    private static Function[] functions =
    {
        Wave, MultiWave, Ripple, CylinderWithCollapsingRadius, StaticSphere, DynamicSphere,
        VerticalBandsSphere,
        VerticalBandsDynamicSphere,
        HorizontalBandsSphere,
        HorizontalBandsDynamicSphere,
        TwistedSphere,
        SelfIntersectingSpindleTorus,
        Torus,
        StaticTorus,
        StarTwistingTorus,
    };
    public static Function GetFunction(FunctionName name)
    {
        return functions[(int)name];
    }

    // 返回下一个函数
    public static FunctionName  GetNextFunctionName(FunctionName  name)
    {
        if ((int)name < functions.Length - 1) {
            return name + 1;
        }
        else {
            return 0;
        }
    }
    // 获取随机函数
    public static FunctionName GetRandomFunctionNameOtherThan(FunctionName name) {
        var choice = (FunctionName)Random.Range(0, functions.Length);
        while (true)
        {
            // 保证每次不会重复
            if (name == choice)
            {
                choice = (FunctionName)Random.Range(0, functions.Length);
            }
            else
            {
                break;
            }
        }

        return choice;
    }

    // 插值变化 各个功能
    public static Vector3 Morph(
        float u, float v, float t, Function from, Function to, float progress
    )
    {
        // SmoothStep = 3x^2-2x^3
        // Lerp会把第三个参数范围在 0~1，因为SmoothStep做了，就用LerpUnclamped
        return Vector3.LerpUnclamped(
            from(u, v, t), to(u, v, t), SmoothStep(0f, 1f, progress)
            );
    }
    
    // f(x,t)=sin(π(x+t))
    public static Vector3 Wave(float u, float v, float time)
    {
        Vector3 p = new Vector3(u, Sin(PI * (u + v + time)), v);
        return p;
        // return Sin(PI * (x + z + time));
    }

    public static Vector3 MultiWave(float u, float v, float time)
    {
        // float y = Sin(PI * (x + 0.5f * time));
        // y += Sin(2f * PI * (x + time)) * 0.5f;    // 范围是[1.5, −1.5]
        // y += Sin(PI * (x + z + 0.25f * time));  //上为单向波，加了这一行就是多向波，时间放慢到1/4
        // // return y * (2f / 3f); //y / 1.5f;
        // return y * (1f / 2.5f);
        Vector3 p = new Vector3(u, 0, v);
        p.y = Sin(PI * (u + 0.5f * time));
        p.y += 0.5f * Sin(2f * PI * (v + time));
        p.y += Sin(PI * (u + v + 0.25f * time));
        return p;
    }

    public static Vector3 Ripple(float u, float v, float time)
    {
        // float d = Sqrt(x * x + z * z);
        // float y = Sin(PI * (4f * d - time));
        // return y / (1f + 10f * d);
        float d = Sqrt(u * u + v * v);
        Vector3 p = new Vector3(u, 0, v);
        p.y = Sin(PI * (4f * d - time));
        p.y /= 1f + 10f * d;
        return p;
    }
    public static Vector3 CylinderWithCollapsingRadius(float u, float v, float time)
    {
        float r = Cos(0.5f * PI * v);
        Vector3 p;
        p.x = r * Sin(PI * u);
        p.y = v;
        p.z = r * Cos(PI * u);
        return p;
    }
    public static Vector3 StaticSphere(float u, float v, float time)
    {
        float r = Cos(0.5f * PI * v);
        Vector3 p;
        p.x = r * Sin(PI * u);
        p.y = Sin(PI * 0.5f * v);
        p.z = r * Cos(PI * u);
        return p;
    }
    public static Vector3 DynamicSphere(float u, float v, float time)
    {
        float r = 0.5f + 0.5f * Sin(PI * time);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 VerticalBandsSphere(float u, float v, float time)
    {
        float r = 0.9f + 0.1f * Sin(8f * PI * u);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 VerticalBandsDynamicSphere(float u, float v, float time)
    {
        float r = 0.9f + 0.1f * Sin(8f * PI * u * time);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 HorizontalBandsSphere(float u, float v, float time)
    {
        float r = 0.9f + 0.1f * Sin(8f * PI * v);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 HorizontalBandsDynamicSphere(float u, float v, float time)
    {
        float r = 0.9f + 0.1f * Sin(8f * PI * v * time);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 TwistedSphere(float u, float v, float time)
    {
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + time));
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 SelfIntersectingSpindleTorus(float u, float v, float time)
    {
        float r = 1f;
        float s = 0.5f + r * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 Torus(float u, float v, float time)
    {
        //float r = 1f;
        float r1 = 0.75f;
        float r2 = 0.25f;
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 StaticTorus(float u, float v, float time)
    {
        //float r = 1f;
        float r1 = 0.75f;
        float r2 = 0.25f;
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    public static Vector3 StarTwistingTorus(float u, float v, float time)
    {
        //float r = 1f;
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * time));
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * time));
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
}
