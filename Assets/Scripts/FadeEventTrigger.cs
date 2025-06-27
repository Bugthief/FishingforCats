using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用于触发淡入淡出动画并在全黑时执行指定事件
/// 将此脚本挂载到任何需要触发淡入淡出动画的对象上
/// </summary>
public class FadeEventTrigger : MonoBehaviour
{
    [Header("淡入淡出类型")]
    public FadeType fadeType = FadeType.FadeOnce;
    
    [Header("在全黑时执行的事件")]
    public UnityEvent onBlackPointEvent;
    
    /// <summary>
    /// 淡入淡出类型枚举
    /// </summary>
    public enum FadeType
    {
        FadeIn,     // 淡入（从透明到不透明）
        FadeOut,    // 淡出（从不透明到透明）
        FadeOnce    // 淡入淡出（从透明到不透明再到透明）
    }
    
    /// <summary>
    /// 触发淡入淡出动画并执行指定事件
    /// </summary>
    public void TriggerFadeWithEvent()
    {
        // 确保FadeManager实例存在
        if (FadeManager.Instance == null)
        {
            Debug.LogError("FadeEventTrigger: FadeManager实例不存在！");
            return;
        }
        
        // 将自身设置为FadeManager的最后一个触发器
        FadeManager.Instance.SetLastTrigger(this);
        
        // 根据淡入淡出类型触发相应的动画
        switch (fadeType)
        {
            case FadeType.FadeIn:
                FadeManager.Instance.FadeIn();
                break;
                
            case FadeType.FadeOut:
                FadeManager.Instance.FadeOut();
                break;
                
            case FadeType.FadeOnce:
                FadeManager.Instance.FadeOnce();
                break;
        }
    }
    
    /// <summary>
    /// 触发淡入动画并执行指定事件
    /// </summary>
    public void TriggerFadeIn()
    {
        if (FadeManager.Instance != null)
        {
            // 将自身设置为FadeManager的最后一个触发器
            FadeManager.Instance.SetLastTrigger(this);
            
            // 触发淡入动画
            FadeManager.Instance.FadeIn();
        }
    }
    
    /// <summary>
    /// 触发淡出动画并执行指定事件
    /// </summary>
    public void TriggerFadeOut()
    {
        if (FadeManager.Instance != null)
        {
            // 将自身设置为FadeManager的最后一个触发器
            FadeManager.Instance.SetLastTrigger(this);
            
            // 触发淡出动画
            FadeManager.Instance.FadeOut();
        }
    }
    
    /// <summary>
    /// 触发淡入淡出动画并执行指定事件
    /// </summary>
    public void TriggerFadeOnce()
    {
        if (FadeManager.Instance != null)
        {
            // 将自身设置为FadeManager的最后一个触发器
            FadeManager.Instance.SetLastTrigger(this);
            
            // 触发淡入淡出动画
            FadeManager.Instance.FadeOnce();
        }
    }
    
    /// <summary>
    /// 设置要执行的事件并触发淡入淡出动画
    /// </summary>
    /// <param name="newEvent">要执行的新事件</param>
    public void SetEventAndTriggerFade(UnityEvent newEvent)
    {
        // 清除当前事件
        onBlackPointEvent.RemoveAllListeners();
        
        // 复制新事件的所有监听器
        if (newEvent != null)
        {
            for (int i = 0; i < newEvent.GetPersistentEventCount(); i++)
            {
                var listener = newEvent.GetPersistentTarget(i);
                var methodName = newEvent.GetPersistentMethodName(i);
                
                if (listener != null && !string.IsNullOrEmpty(methodName))
                {
                    // 使用反射添加监听器
                    UnityAction action = () => {
                        try {
                            var method = listener.GetType().GetMethod(methodName, System.Type.EmptyTypes);
                            if (method != null)
                            {
                                method.Invoke(listener, null);
                            }
                            else
                            {
                                // 尝试查找具有参数的方法
                                var methods = listener.GetType().GetMethods();
                                foreach (var m in methods)
                                {
                                    if (m.Name == methodName)
                                    {
                                        Debug.LogWarning($"找到方法 {methodName}，但它需要参数。请确保使用无参数的方法。");
                                        break;
                                    }
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"调用方法 {methodName} 时出错: {e.Message}");
                        }
                    };
                    
                    onBlackPointEvent.AddListener(action);
                }
            }
        }
        
        // 触发淡入淡出动画
        TriggerFadeWithEvent();
    }
    
}
