using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mono : MonoSingleton<Mono>
{
    public event Action onUpdate;//每帧更新
    public event Action onFixedUpdate;//定时更新
    public event Action onLateUpdate;//延迟更新

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
