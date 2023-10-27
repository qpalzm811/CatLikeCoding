using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fractal : MonoBehaviour
{
    // public GameObject myObject;

    [SerializeField, Range(1, 8)] private int depth = 1;

    [SerializeField] private Mesh mesh;
    [SerializeField] private Material matetial;

    static Vector3[] directions =
    {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back, Vector3.down,
    };

    static Quaternion[] rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)
    };

    struct FractalPart
    {
        public Vector3 direction, worldPosition;
        public Quaternion rotation, worldRotation;

        // 旋转角度单独存为浮点值，防止浮点精度造成的 四元数多次相乘 扩大的误差
        public float spinAngle;
        
        // public Transform transform;  // 不再用Transform存储世界坐标位置旋转，放在上面
    }

    // 二维，为同一级别的所有部分提供自己的数组
    FractalPart[][] parts;
    
    // 由于不再具有这些组件，需要自己创建矩阵。
    // 我们将它们存储在每个级别的数组中，就像存储零件一样。
    Matrix4x4[][] matrices;

    // 1.0版本，对于每个孩子都设置名字，生成mesh对象
    // 添加子索引，因为每个孩子都有 各自的方向和旋转
    // FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    // {
    //     // var go = new GameObject("Fractal Part L" + levelIndex + " C" + childIndex);
    //     //
    //     // go.transform.localScale = scale * Vector3.one;
    //     //
    //     // go.transform.SetParent(transform, false);
    //     //
    //     // go.AddComponent<MeshFilter>().mesh = mesh;
    //     // go.AddComponent<MeshRenderer>().material = matetial;
    //
    //     // return new FractalPart 创建新的结构
    //     return new FractalPart()
    //     {
    //         direction = directions[childIndex],
    //         rotation = rotations[childIndex],
    //         // transform = go.transform
    //     };
    // }
    FractalPart CreatePart(int childIndex) => new FractalPart{
        direction = directions[childIndex],
        rotation = rotations[childIndex],
    };

    // 渲染的缓冲数组
    ComputeBuffer[] matricesBuffers;

    void Awake()
    {
        // 不需要防止无限递归，所以不需要等到Start
        
        parts = new FractalPart[depth][];
        // 因为不再有Transform组件，所以自己创建矩阵存储
        matrices = new Matrix4x4[depth][];
        
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
            // 每个Level都是前一个Level的五倍，五个子级所以五个元素大小
            parts[i] = new FractalPart[length];
            matrices[i] = new Matrix4x4[length];
        }

        // float scale = 1f;
        parts[0][0] = CreatePart(0);
        
        for (int li = 1; li < parts.Length; li++)
        {
            // li代表层级索引，ci代表这个层级孩子索引
            // 这个地方 li从1开始 因为0级的大球只有一个 (就在上面实例化
            
            // scale *= 0.5f;
            
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }
    }
    void Update ()
    {
        float scale = 1f;

        // animation动画
        Quaternion deltaRotation = Quaternion.Euler(0f, 22.5f * Time.deltaTime, 0f);
        FractalPart rootPart = parts[0][0];
        rootPart.rotation *= deltaRotation;
        rootPart.worldRotation = rootPart.rotation;   //调整根的Rotation，让他的分型也旋转
        parts[0][0] = rootPart; // 值类型再拷贝回去
        
        // 记录三个项，返回一个Matrix4x4结构体
        matrices[0][0] = Matrix4x4.TRS(
            rootPart.worldPosition, rootPart.worldRotation, Vector3.one
        );
        
        // 遍历所有级别的所有部分
        for (int li = 1; li < parts.Length; li++) {
            scale *= 0.5f;

            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            Matrix4x4[] levelMatrices = matrices[li];
            
            for (int fpi = 0; fpi < levelParts.Length; fpi++) {
                // 父transform
                // Transform parentTransform = parentParts[fpi / 5].transform;
                FractalPart parent = parentParts[fpi / 5];
                
                // 子对象节点 值类型在此是复制
                FractalPart part = levelParts[fpi];
                
                part.rotation *= deltaRotation;
                part.worldRotation = parent.worldRotation * part.rotation;
                
                part.worldPosition =
                    parent.worldPosition +
                    parent.worldRotation  * (1.5f * scale * part.direction);
                // part.direction表示旋转方向
                // 1.5会为了避免圆球之间距离太近了
                
                levelParts[fpi] = part;
                levelMatrices[fpi] = Matrix4x4.TRS(
                    part.worldPosition, part.worldRotation, scale * Vector3.one
                );
            }
        }
    }
    // void Start()
    // {
    //     name = "Fractal " + depth;
    //     if (depth <= 1)
    //     {
    //         return;
    //     }
    //
    //     var childA = CreateChild(Vector3.up, Quaternion.identity);
    //     var childB = CreateChild(Vector3.right, Quaternion.Euler(0f, 0f, -90f));
    //     var childC = CreateChild(Vector3.left, Quaternion.Euler(0f, 0f, 90f));
    //     var childD = CreateChild(Vector3.forward, Quaternion.Euler(90f, 0f, 0f));
    //     var childE = CreateChild(Vector3.back, Quaternion.Euler(-90f, 0f, 0f));
    //     
    //     childA.transform.SetParent(transform, false);
    //     childB.transform.SetParent(transform, false);
    //     childC.transform.SetParent(transform, false);
    //     childD.transform.SetParent(transform, false);
    //     childE.transform.SetParent(transform, false);
    // }
    //
    // // Update is called once per frame
    // void Update()
    // {
    //     // transform.Rotate(0f, 22.5f * Time.deltaTime, 0);
    //     Debug.Log("Time.deltaTime\t" + Time.deltaTime);
    //     Debug.Log("Time.fixedTime\t" + Time.fixedTime);
    //     Debug.Log("Time.time\t" + Time.time);
    // }
    //
    // /// <summary>
    // /// 实例化 返回子对象
    // /// </summary>
    // /// <param name="direction">矩阵方向</param>
    // /// <param name="rotation">旋转方向</param>
    // /// <returns></returns>
    // Fractal CreateChild(Vector3 direction, Quaternion rotation)
    // {
    //     Fractal child = Instantiate(this);
    //     child.depth = depth - 1;
    //     
    //     Transform childTransform = child.transform;
    //     // childTransform.SetParent(transform, false); 这里写这个会多一层
    //     childTransform.localPosition = 0.75f * direction;
    //     childTransform.localScale = 0.5f * Vector3.one;
    //     childTransform.localRotation = rotation;
    //     return child;
    // }
}
