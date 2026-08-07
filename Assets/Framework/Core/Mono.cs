using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mono : MonoSingleton<Mono>
{
    private event Action onUpdate;//每帧更新
    private event Action onFixedUpdate;//定时更新
    private event Action onLateUpdate;//延迟更新

    private void Update()
    {
        onUpdate?.Invoke();
    }

    private void FixedUpdate()
    {
        onFixedUpdate?.Invoke();
    }

    private void LateUpdate()
    {
        onLateUpdate?.Invoke();
    }
}
