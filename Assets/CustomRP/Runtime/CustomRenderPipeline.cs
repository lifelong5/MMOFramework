using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    CameraRenderer cameraRenderer = new CameraRenderer();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
    }
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach(Camera cam in cameras)
        {
            //¸³Öµ
            cameraRenderer.Render(context, cam);
        }
    }
}