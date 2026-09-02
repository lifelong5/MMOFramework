using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 渲染一个相机
/// </summary>
partial class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;
    /// <summary>
    /// 创建一个命令缓存区
    /// </summary>
    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };
    /// <summary>
    /// 视锥体剔除的结果
    /// </summary>
    CullingResults cullingResults;
    /// <summary>
    /// 无光照着色器
    /// </summary>
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    /// <summary>
    /// 其他的着色器
    /// </summary>
    static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    static Material errorMaterial;
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;
        //摄像机没有正确的视锥体裁剪的数据，直接返回，说明摄像机有问题
        if (!Cull())
        {
            return;
        }
        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();//绘制使用其他shader的几何体
        Submit();
    }
    /// <summary>
    /// 绘制几何体
    /// </summary>
    void DrawVisibleGeometry()
    {
        var sortingSetting = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };//从相机获取排序设置
        var drawingSetting = new DrawingSettings(unlitShaderTagId, sortingSetting);//默认绘制设置
        var filteringSetting = new FilteringSettings(RenderQueueRange.opaque);//默认过滤设置

        context.DrawRenderers(cullingResults, ref drawingSetting, ref filteringSetting);
        context.DrawSkybox(camera);

        //因为透明几何体在绘制的时候是不写入深度信息的，如果sky在不透明之后绘制，就会覆盖掉透明几何体，所以在sky之后单独绘制
        sortingSetting.criteria = SortingCriteria.CommonTransparent;
        drawingSetting.sortingSettings = sortingSetting;
        filteringSetting.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawingSetting, ref filteringSetting);
    }
    /// <summary>
    /// 绘制使用legacyShaderTagIds的几何体
    /// </summary>
    void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawingSetting = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
        {
            overrideMaterial = errorMaterial//将材质替换成错误材质
        };
        var filteringSetting = FilteringSettings.defaultValue;

        //应用所有的legacyShaderTagIds中的shader
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSetting.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        context.DrawRenderers(cullingResults, ref drawingSetting, ref filteringSetting);
    }
    void Setup()
    {
        //先clear再set：clear的时候还没有建立上下文，不知道渲染目标，用Hidden/InternalClear内部着色器将颜色写入渲染目标，需要执行完整的像素着色器运行
        //先set再clear：set之后已经知道了渲染目标，直接由ROP光栅化输出单元，直接向渲染目标中写入目标颜色
        context.SetupCameraProperties(camera);//将摄像机组件的所有属性，同步到底层原生渲染API
        buffer.ClearRenderTarget(true, true, Color.clear);//清除渲染目标缓存
        buffer.BeginSample(bufferName);//开始性能采样
        ExecuteBuffer();
    }
    void Submit()
    {
        buffer.EndSample(bufferName);//停止性能采样
        ExecuteBuffer();
        context.Submit();
    }
    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))//从摄像机中提取出C++层需要的视锥体裁剪的数据
        {
            cullingResults = context.Cull(ref p);//执行剔除
            return true;
        }
        return false;
    }
    /// <summary>
    /// 执行命令缓冲区
    /// </summary>
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);//将命令传递给上下文
        buffer.Clear();//清空命令缓冲区
    }
}
