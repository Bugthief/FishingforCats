using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 管理屏幕淡入淡出动画，允许动态设置和重置在全黑时触发的事件
/// </summary>
public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("淡入淡出动画控制器")]
    public Animator fadeAnimator;

    [Header("淡入淡出动画触发器名称")]
    public string fadeInTrigger = "FadeIn";
    public string fadeOutTrigger = "FadeOut";
    public string fadeOnceTrigger = "FadeOnce";

    // 最后一个触发淡入淡出动画的FadeEventTrigger
    private FadeEventTrigger lastTrigger;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 在全黑时触发的事件，由动画事件调用
    /// </summary>
    public void OnBlackPoint()
    {
        try
        {
            // 如果有最后一个触发器，调用其事件
            if (lastTrigger != null && lastTrigger.onBlackPointEvent != null)
            {
                lastTrigger.onBlackPointEvent.Invoke();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("触发事件时出错: " + e.Message);
        }
    }
    
    /// <summary>
    /// 在动画结束时调用，由动画事件触发
    /// </summary>
    public void OnAnimationComplete()
    {
        // 清除最后一个触发器引用
        lastTrigger = null;
    }
    
    /// <summary>
    /// 设置最后一个触发淡入淡出动画的FadeEventTrigger
    /// </summary>
    /// <param name="trigger">触发器</param>
    public void SetLastTrigger(FadeEventTrigger trigger)
    {
        lastTrigger = trigger;
    }

    /// <summary>
    /// 执行淡入动画
    /// </summary>
    public void FadeIn()
    {
        // 触发淡入动画
        fadeAnimator.SetTrigger(fadeInTrigger);
    }

    /// <summary>
    /// 执行淡出动画
    /// </summary>
    public void FadeOut()
    {
        // 触发淡出动画
        fadeAnimator.SetTrigger(fadeOutTrigger);
    }
    
    /// <summary>
    /// 执行一次淡入淡出动画
    /// </summary>
    public void FadeOnce()
    {
        // 触发淡入淡出动画
        fadeAnimator.SetTrigger(fadeOnceTrigger);
    }
}
