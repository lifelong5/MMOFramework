using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRenderer
{
    //必须要实现，没有实现的话，就会删除对应的调用
    partial void DrawUnsupportedShaders();
    //Gizmos 是 Unity 编辑器中一套用于在 Scene 视图中进行可视化调试和辅助设计的绘图工具
    partial void DrawGizmos();
    /// <summary>
    /// 在Scene中显示UI元素
    /// </summary>
    partial void PrepareForSceneWindow();
    /// <summary>
    /// 当有多个摄像机的时候FrameDebugger的层级数中不同相机的会混在一起，为了使得分开，分别设置对应的CommandBuffer的名字
    /// </summary>
    partial void PrepareBuffer();
#if UNITY_EDITOR
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
    /// <summary>
    /// 绘制使用legacyShaderTagIds的几何体
    /// </summary>
    partial void DrawUnsupportedShaders()
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
    partial void DrawGizmos()
    {
        //在Scene视图下或者开启了Gizmos
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);//在场景渲染完毕后但是在所有后处理之前，保证这一层的绘制不会受到后处理的影响
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);//在所有渲染和后处理之后绘制Gizmo，确保永远在屏幕最上层
        }
    }
    /// <summary>
    /// 通过Canvas组件下的测试当RenderMode是Screen Space - Overlay的时候，是UGUI.Rendering,RenderOverlays管线下渲染的
    /// RenderMode是Screen Space - Camera或者World Space的时候，是在对应摄像机的渲染里进行渲染，这种情况下Scene场景下是看不到UI元素的，
    /// 为了解决这个问题，在编辑器情况下，如果是Scene窗口，将UI元素转换到3D世界空间中，并且加入当前摄像机的上下文中和其他的3D对象一起渲染
    /// </summary>
    partial void PrepareForSceneWindow()
    {
        if(camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }
    partial void PrepareBuffer()
    {
        buffer.name = camera.name;
    }
#endif
}
