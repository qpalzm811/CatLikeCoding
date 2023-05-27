Shader "Graph/PointGPU"
{
    Properties {
		_Smoothness ("Smoothness", Range(0,1)) = 0.5
	}
    
    SubShader{
        CGPROGRAM
        // 启用了全向光阴影（fullforwardshadows）和实例化选项（instancing_options）
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        // assumeuniformscaling告诉Unity引擎当前实例化的对象是均匀缩放
        // 当我们不知道实例化对象的缩放比例时，Unity会在处理每个实例时都计算单位长度，以便正确地调整法线和切线等向量。

        // unity默认异步编译，若是没有编译完成，用虚拟着色器代替
        // 这个命令让 同步编译，强制unity第一次使用着色器前，立即编译，确保使用时已经编译完成
        // 避免虚拟着色器导致性能下降
        #pragma editor_sync_compilation
        #pragma target 4.5
        // #include "PointGPU.hlsl"

        struct Input
        {
            float3 worldPos;
            
        };
        
        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            // 仅将其用于特别编译为过程绘制的着色器变体
	        StructuredBuffer<float3> _Positions;
        #endif
        
        float _Smoothness;
		float _Step;

        void ConfigureProcedural ()
        {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                // _Positions 全局可访问
                // 此处对应 ComputeShader中的 _Positions
				float3 position = _Positions[unity_InstanceID];
                // 转换矩阵定义
                unity_ObjectToWorld = 0.0;
                // 提供列向量column vector,最右边一列的平移
            	unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
                // 缩放scale
				unity_ObjectToWorld._m00_m11_m22 = _Step;

			#endif
        }

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
        {
            surface.Smoothness = _Smoothness;
            // rgb xyz对应
            surface.Albedo = saturate(input.worldPos * 0.5 + 0.5);
        }
        ENDCG
    }

    FallBack "Diffuse"
}
