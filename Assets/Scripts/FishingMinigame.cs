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
    
    [Header("目标区域设置")]
    [Tooltip("目标区域活动范围的最小值（0-1之间）")]
    [Range(0f, 1f)]
    public float targetMinRange = 0.2f;
    
    [Tooltip("目标区域活动范围的最大值（0-1之间）")]
    [Range(0f, 1f)]
    public float targetMaxRange = 0.8f;
    
    [Tooltip("目标区域移动速度")]
    public float targetMoveSpeed = 100f;
    
    [Header("鱼的设置")]
    [Tooltip("鱼向上移动的固定速度")]
    public float fishUpwardSpeed = 50f;
    
    [Tooltip("鱼上下摆动的最小速度")]
    public float fishBobMinSpeed = 20f;
    
    [Tooltip("鱼上下摆动的最大速度")]
    public float fishBobMaxSpeed = 60f;
    
    [Tooltip("鱼上下摆动的最大幅度")]
    public float fishBobAmplitude = 30f;
    
    [Header("交互设置")]
    [Tooltip("成功按键")]
    public KeyCode successKey = KeyCode.Space;
    
    [Header("事件")]
    [Tooltip("成功捕获鱼时触发")]
    public UnityEvent onFishCaught;
    
    [Tooltip("鱼逃脱时触发")]
    public UnityEvent onFishEscaped;

    
    // 私有变量
    private bool isMovingRight = true;
    private float targetCurrentPosition;
    private float fishBobSpeed;
    private float fishBobPhase;
    private Vector3 fishStartPosition;
    private float frameHeight = 10f; // 游戏区域高度
    private float fishBaseY; // 鱼的基础Y位置（不包括摆动）
    private float minY, maxY; // 游戏区域的Y轴边界
    
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
        
        // 初始化变量（使用世界坐标）
        frameHeight = 10f; // 默认游戏区域高度，可以在Inspector中调整
        fishStartPosition = fishImage.position;
        
        // 计算游戏区域边界
        Vector3 framePos = frameRect.position;
        float halfHeight = frameHeight / 2f;
        // 假设框架底部为游戏区域底部，顶部为游戏区域顶部
        // 您可以根据实际需要调整这些值
        
        // 游戏开始时不激活
        isGameActive = false;
    }
    
    /// <summary>
    /// 开始钓鱼小游戏
    /// </summary>
    public void StartGame()
    {
        if (isGameActive) return;
        
        // 重置位置
        fishImage.position = fishStartPosition;
        fishBaseY = fishStartPosition.y;
        targetCurrentPosition = (targetMinRange + targetMaxRange) / 2f;
        UpdateTargetPosition();
        
        // 随机生成鱼的摆动速度
        fishBobSpeed = Random.Range(fishBobMinSpeed, fishBobMaxSpeed);
        fishBobPhase = 0f;
        
        isGameActive = true;
    }
    
    /// <summary>
    /// 游戏主循环
    /// </summary>
    private void Update()
    {
        if (!isGameActive) return;
        
        // 移动目标区域
        MoveTargetArea();
        
        // 移动鱼
        MoveFish();
        
        // 检查玩家输入
        CheckPlayerInput();
    }
    
    /// <summary>
    /// 移动目标区域
    /// </summary>
    private void MoveTargetArea()
    {
        // 计算目标区域的移动
        float moveAmount = targetMoveSpeed * Time.deltaTime / frameHeight;
        if (isMovingRight)
        {
            targetCurrentPosition += moveAmount;
            if (targetCurrentPosition >= targetMaxRange)
            {
                targetCurrentPosition = targetMaxRange;
                isMovingRight = false;
            }
        }
        else
        {
            targetCurrentPosition -= moveAmount;
            if (targetCurrentPosition <= targetMinRange)
            {
                targetCurrentPosition = targetMinRange;
                isMovingRight = true;
            }
        }
        
        // 更新目标区域位置
        UpdateTargetPosition();
    }
    
    /// <summary>
    /// 更新目标区域的位置
    /// </summary>
    private void UpdateTargetPosition()
    {
        float yPos = Mathf.Lerp(-frameHeight/2, frameHeight/2, targetCurrentPosition);
        Vector3 newPos = targetArea.position;
        newPos.y = yPos;
        targetArea.position = newPos;
    }
    
    /// <summary>
    /// 移动鱼
    /// </summary>
    private void MoveFish()
    {
        // 计算鱼的上升移动（基础Y位置的变化）
        fishBaseY += fishUpwardSpeed * Time.deltaTime;
        
        // 计算鱼的上下摆动
        fishBobPhase += fishBobSpeed * Time.deltaTime;
        float bobOffset = Mathf.Sin(fishBobPhase) * fishBobAmplitude;
        
        // 更新鱼的位置（基础Y位置 + 摆动偏移）
        Vector3 newPos = fishImage.position;
        newPos.y = fishBaseY + bobOffset;
        fishImage.position = newPos;
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
