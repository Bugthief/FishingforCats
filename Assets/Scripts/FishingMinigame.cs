using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 钓鱼小游戏控制器
/// 实现钓鱼小游戏的核心逻辑，包括目标区域移动、鱼的移动、成功判定等
/// </summary>
public class FishingMinigame : MonoBehaviour
{
    public bool isGameActive = false;
    public bool isInArea = false;

    [Header("游戏对象引用")]
    [Tooltip("矩形外框（竖直放置）")]
    public Transform frameRect;

    [Tooltip("目标区域（判定区）")]
    public Transform targetArea;

    [Tooltip("可移动的鱼Sprite")]
    public Transform fishImage;

    [Header("动画控制")]
    [Tooltip("目标区域的Animator组件")]
    public Animator targetAreaAnimator;

    [Tooltip("鱼的Animator组件")]
    public Animator fishAnimator;

    [Tooltip("目标区域动画机的难度参数名")]
    public string targetDifficultyParameterName = "DifficultyLevel";

    [Tooltip("鱼动画机的难度参数名")]
    public string fishDifficultyParameterName = "DifficultyLevel";

    [Header("鱼&钓竿脚本引用")]
    [Tooltip("鱼的Fish脚本组件，用于获取难度等级")]
    public Fish fishScript;
    public PoleManager poleManager;

    [Header("鱼的设置")]
    [Tooltip("鱼的起始Y轴位置（相对坐标）")]
    public float fishStartY = -4f;

    // [Tooltip("鱼向上移动的初始速度")]
    // public float fishInitialSpeed = 1f;

    // [Tooltip("鱼向上移动的加速度")]
    // public float fishAcceleration = 0.5f;

    [Header("交互设置")]
    [Tooltip("成功按键")]
    public KeyCode successKey = KeyCode.Space;

    [Header("游戏事件")]
    [Tooltip("成功捕获鱼时触发")]
    public UnityEvent onFishCaught;

    [Tooltip("鱼逃脱时触发")]
    public UnityEvent onFishEscaped;



    // 私有变量
    private Vector3 fishStartPosition;
    private float currentFishSpeed; // 当前鱼的速度
    private float gameStartTime; // 游戏开始时间

    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void Start()
    {
        // 确保所有必要的组件都已分配
        if (frameRect == null || targetArea == null || fishImage == null)
        {
            Debug.LogError("钓鱼小游戏缺少必要的组件引用！");
            enabled = false;
            return;
        }

        // 初始化鱼的起始位置（使用相对Y坐标）
        fishStartPosition = fishImage.position;
        fishStartPosition.y = fishStartY;

        // 游戏开始时不激活
        isGameActive = false;
    }

    /// <summary>
    /// 开始钓鱼小游戏
    /// </summary>
    public void StartGame()
    {
        if (isGameActive) return;

        // 重置鱼的位置
        fishImage.position = fishStartPosition;

        // 初始化运动参数
        //currentFishSpeed = fishInitialSpeed;
        gameStartTime = Time.time;

        // 根据难度等级触发相应的动画事件
        TriggerAnimationsByDifficulty();

        isGameActive = true;
    }

    /// <summary>
    /// 根据难度等级设置动画机参数
    /// </summary>
    private void TriggerAnimationsByDifficulty()
    {
        // 从鱼脚本获取难度等级
        int difficultyLevel = GetFishDifficultyFromScript();
        
        Debug.Log($"设置动画难度等级: {difficultyLevel}");

        // 设置目标区域动画机的难度参数
        if (targetAreaAnimator != null && !string.IsNullOrEmpty(targetDifficultyParameterName))
        {
            // 检查Animator是否有有效的Controller
            if (targetAreaAnimator.runtimeAnimatorController != null)
            {
                // 检查参数是否存在
                if (HasAnimatorParameter(targetAreaAnimator, targetDifficultyParameterName, AnimatorControllerParameterType.Int))
                {
                    targetAreaAnimator.SetInteger(targetDifficultyParameterName, difficultyLevel);
                    Debug.Log($"目标区域动画机参数 '{targetDifficultyParameterName}' 设置为: {difficultyLevel}");
                }
                else
                {
                    Debug.LogWarning($"目标区域动画机中没有找到整数参数: {targetDifficultyParameterName}");
                }
            }
            else
            {
                Debug.LogWarning("目标区域Animator没有分配AnimatorController！");
            }
        }
        else
        {
            Debug.LogWarning("目标区域Animator引用为空或参数名为空");
        }

        // 设置鱼动画机的难度参数
        if (fishAnimator != null && !string.IsNullOrEmpty(fishDifficultyParameterName))
        {
            // 检查Animator是否有有效的Controller
            if (fishAnimator.runtimeAnimatorController != null)
            {
                // 检查参数是否存在
                if (HasAnimatorParameter(fishAnimator, fishDifficultyParameterName, AnimatorControllerParameterType.Int))
                {
                    fishAnimator.SetInteger(fishDifficultyParameterName, difficultyLevel);
                    Debug.Log($"鱼动画机参数 '{fishDifficultyParameterName}' 设置为: {difficultyLevel}");
                }
                else
                {
                    Debug.LogWarning($"鱼动画机中没有找到整数参数: {fishDifficultyParameterName}");
                }
            }
            else
            {
                Debug.LogWarning("鱼Animator没有分配AnimatorController！");
            }
        }
        else
        {
            Debug.LogWarning("鱼Animator引用为空或参数名为空");
        }
    }
    
    /// <summary>
    /// 检查Animator是否有指定的参数
    /// </summary>
    /// <param name="animator">动画控制器</param>
    /// <param name="parameterName">参数名</param>
    /// <param name="parameterType">参数类型</param>
    /// <returns>是否存在该参数</returns>
    private bool HasAnimatorParameter(Animator animator, string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;
            
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName && param.type == parameterType)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 从鱼脚本获取难度等级
    /// </summary>
    /// <returns>难度等级</returns>
    private int GetFishDifficultyFromScript()
    {
        if (fishScript == null)
        {
            Debug.LogWarning("鱼脚本引用为空，使用默认难度等级1");
            return 1;
        }

        // 直接从Fish脚本获取难度等级
        return Mathf.Clamp(fishScript.difficultyLevel, 1, 5);
    }

    /// <summary>
    /// 获取当前鱼的难度等级（从鱼脚本获取）
    /// </summary>
    /// <returns>当前难度等级</returns>
    public int GetFishDifficultyLevel()
    {
        return GetFishDifficultyFromScript();
    }

    /// <summary>
    /// 获取当前钓竿信息
    /// </summary>
    /// <returns>当前钓竿数据</returns>
    public PoleManager.PoleData GetCurrentPole()
    {
        if (poleManager != null)
        {
            return poleManager.GetCurrentPole();
        }
        
        Debug.LogWarning("FishingMinigame: PoleManager引用为空！");
        return null;
    }
    
    /// <summary>
    /// 获取当前钓竿等级
    /// </summary>
    /// <returns>钓竿等级</returns>
    public int GetCurrentPoleLevel()
    {
        if (poleManager != null)
        {
            return poleManager.GetCurrentPoleLevel();
        }
        
        return 1; // 默认等级
    }
    
    /// <summary>
    /// 获取钓竿成功率加成
    /// </summary>
    /// <returns>成功率加成</returns>
    public float GetPoleSuccessBonus()
    {
        if (poleManager != null)
        {
            return poleManager.GetSuccessBonus();
        }
        
        return 0f; // 默认无加成
    }

    /// <summary>
    /// 设置鱼的起始Y轴位置
    /// </summary>
    /// <param name="startY">起始Y轴位置（相对坐标）</param>
    public void SetFishStartY(float startY)
    {
        fishStartY = startY;
        // 更新起始位置
        fishStartPosition = fishImage.position;
        fishStartPosition.y = fishStartY;
    }

    /// <summary>
    /// 游戏主循环
    /// </summary>
    private void Update()
    {
        if (!isGameActive) return;

        // 更新鱼的位置（匀加速运动）
        //UpdateFishPosition();

        // 检查玩家输入
        CheckPlayerInput();
    }


    /// <summary>
    /// 检查玩家输入
    /// </summary>
    private void CheckPlayerInput()
    {
        if (Input.GetKeyDown(successKey))
        {
            // 检查鱼是否在目标区域内
            if (IsFishInTargetArea())
            {
                // 成功捕获鱼
                isGameActive = false;
                onFishCaught?.Invoke();
            }
        }
    }

    /// <summary>
    /// 检查鱼是否在目标区域内（通过外部TriggerEvent设置isInArea）
    /// </summary>
    private bool IsFishInTargetArea()
    {
        // 直接返回isInArea的值，这个值将通过TriggerEvent设置
        return isInArea;
    }

    /// <summary>
    /// 停止游戏
    /// </summary>
    public void StopGame()
    {
        isGameActive = false;
    }

    /// <summary>
    /// 设置鱼是否在目标区域内（供TriggerEvent调用）
    /// </summary>
    /// <param name="inArea">是否在区域内</param>
    public void SetFishInArea(bool inArea)
    {
        isInArea = inArea;
    }

    /// <summary>
    /// 触发鱼逃脱事件（供上边界TriggerEvent调用）
    /// </summary>
    public void TriggerFishEscaped()
    {
        if (isGameActive)
        {
            isGameActive = false;
            onFishEscaped?.Invoke();
        }
    }
}
