using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 钓鱼游戏管理器
/// 管理整个钓鱼游戏的流程，包括投出鱼竿、等待、上钩、收竿、小游戏和获得鱼等阶段
/// </summary>
public class FishingGameManager : MonoBehaviour
{
    // 钓鱼状态枚举
    public enum FishingState
    {
        Idle,           // 空闲状态
        Casting,        // 投出鱼竿
        Waiting,        // 等待鱼上钩
        Biting,         // 鱼上钩
        Minigame,       // 钓鱼小游戏
        Rewarding,      // 获得奖励
        Failed          // 失败状态
    }
    
    [Header("组件引用")]
    [Tooltip("钓鱼小游戏控制器")]
    public FishingMinigame minigameController;
    
    [Tooltip("鱼类管理器")]
    public FishesManager fishesManager;
    
    [Tooltip("钓竿管理器")]
    public PoleManager poleManager;
    
    [Tooltip("鱼获得面板")]
    public GameObject fishRewardPanel;
    
    [Tooltip("鱼溜了提示")]
    public GameObject fishEscapedNotice;
    
    [Tooltip("上钩提示UI")]
    public GameObject bitingNoticeUI;
    
    [Header("音频设置")]
    [Tooltip("上钩音效")]
    public AudioClip bitingSound;
    
    [Tooltip("音频源")]
    public AudioSource audioSource;
    
    [Header("时间设置")]
    [Tooltip("等待阶段最短时间（秒）")]
    public float minWaitTime = 3f;
    
    [Tooltip("等待阶段最长时间（秒）")]
    public float maxWaitTime = 10f;
    
    [Tooltip("上钩持续时间（秒）")]
    public float bitingDuration = 3f;
    
    [Header("交互设置")]
    [Tooltip("交互按键")]
    public KeyCode interactKey = KeyCode.Space;
    
    [Header("状态事件")]
    [Tooltip("进入空闲状态时触发")]
    public UnityEvent onIdleState;
    
    [Tooltip("进入投竿状态时触发")]
    public UnityEvent onCastingState;
    
    [Tooltip("进入等待状态时触发")]
    public UnityEvent onWaitingState;
    
    [Tooltip("进入上钩状态时触发")]
    public UnityEvent onBitingState;
    
    [Tooltip("进入小游戏状态时触发")]
    public UnityEvent onMinigameState;
    
    [Tooltip("进入奖励状态时触发")]
    public UnityEvent onRewardingState;
    
    [Tooltip("进入失败状态时触发")]
    public UnityEvent onFailedState;
    
    [Header("行为事件")]
    [Tooltip("成功获得鱼时触发")]
    public UnityEvent onFishCaught;
    
    [Tooltip("鱼溜走时触发")]
    public UnityEvent onFishEscaped;
    
    [Tooltip("投出鱼竿时触发")]
    public UnityEvent onCastRod;
    
    [Tooltip("收回鱼竿时触发")]
    public UnityEvent onReelRod;
    
    // 私有变量
    private FishingState currentState = FishingState.Idle;
    private float stateTimer = 0f;
    private float waitTime = 0f;
    
    /// <summary>
    /// 初始化
    /// </summary>
    private void Start()
    {
        // 确保所有必要的组件都已分配
        if (minigameController == null)
        {
            Debug.LogError("钓鱼游戏管理器缺少必要的小游戏控制器引用！");
            enabled = false;
            return;
        }
        
        // 初始化UI
        if (fishRewardPanel != null) fishRewardPanel.SetActive(false);
        if (fishEscapedNotice != null) fishEscapedNotice.SetActive(false);
        if (bitingNoticeUI != null) bitingNoticeUI.SetActive(false);
        
        // 设置小游戏事件
        minigameController.onFishCaught.AddListener(OnMinigameSuccess);
        minigameController.onFishEscaped.AddListener(OnMinigameFailed);
        
        // 初始状态为空闲
        SetState(FishingState.Idle);
    }
    
    /// <summary>
    /// 游戏主循环
    /// </summary>
    private void Update()
    {
        // 根据当前状态更新游戏
        switch (currentState)
        {
            case FishingState.Idle:
                // 在空闲状态下，检查玩家是否按下交互键开始钓鱼
                if (Input.GetKeyDown(interactKey))
                {
                    StartFishing();
                }
                break;
                
            case FishingState.Waiting:
                // 在等待状态下，检查是否到达等待时间
                stateTimer += Time.deltaTime;
                if (stateTimer >= waitTime)
                {
                    // 鱼上钩了
                    SetState(FishingState.Biting);
                }
                
                // 如果玩家在等待过程中按下交互键，收回鱼竿
                if (Input.GetKeyDown(interactKey))
                {
                    ReelRod();
                }
                break;
                
            case FishingState.Biting:
                // 在上钩状态下，检查玩家是否按下交互键开始小游戏
                stateTimer += Time.deltaTime;
                
                if (Input.GetKeyDown(interactKey))
                {
                    // 玩家按下交互键，开始小游戏
                    SetState(FishingState.Minigame);
                }
                else if (stateTimer >= bitingDuration)
                {
                    // 上钩时间结束，鱼溜走了
                    SetState(FishingState.Failed);
                }
                break;
                
            case FishingState.Failed:
            case FishingState.Rewarding:
                // 在这些状态下，检查玩家是否按下交互键返回空闲状态
                if (Input.GetKeyDown(interactKey))
                {
                    SetState(FishingState.Idle);
                }
                break;
        }
    }
    
    /// <summary>
    /// 开始钓鱼
    /// </summary>
    public void StartFishing()
    {
        if (currentState != FishingState.Idle) return;
        
        // 投出鱼竿
        SetState(FishingState.Casting);
        
        // 触发投出鱼竿事件
        onCastRod?.Invoke();
        
        // 延迟一小段时间后进入等待状态
        StartCoroutine(DelayedStateChange(FishingState.Waiting, 1f));
    }
    
    /// <summary>
    /// 收回鱼竿
    /// </summary>
    public void ReelRod()
    {
        if (currentState != FishingState.Waiting && currentState != FishingState.Biting) return;
        
        // 触发收回鱼竿事件
        onReelRod?.Invoke();
        
        // 返回空闲状态
        SetState(FishingState.Idle);
    }
    
    /// <summary>
    /// 小游戏成功回调
    /// </summary>
    private void OnMinigameSuccess()
    {
        // 显示获得鱼的面板
        if (fishRewardPanel != null) fishRewardPanel.SetActive(true);
        
        // 触发获得鱼事件
        onFishCaught?.Invoke();
        
        // 设置为奖励状态
        SetState(FishingState.Rewarding);
    }
    
    /// <summary>
    /// 小游戏失败回调
    /// </summary>
    private void OnMinigameFailed()
    {
        // 显示鱼溜了提示
        if (fishEscapedNotice != null) fishEscapedNotice.SetActive(true);
        
        // 触发鱼溜走事件
        onFishEscaped?.Invoke();
        
        // 设置为失败状态
        SetState(FishingState.Failed);
    }
    
    /// <summary>
    /// 设置游戏状态
    /// </summary>
    private void SetState(FishingState newState)
    {
        // 退出当前状态
        switch (currentState)
        {
            case FishingState.Biting:
                // 退出上钩状态，隐藏上钩提示
                if (bitingNoticeUI != null) bitingNoticeUI.SetActive(false);
                break;
                
            case FishingState.Minigame:
                // 退出小游戏状态，停止小游戏
                minigameController.StopGame();
                break;
                
            case FishingState.Rewarding:
                // 退出奖励状态，隐藏奖励面板
                if (fishRewardPanel != null) fishRewardPanel.SetActive(false);
                break;
                
            case FishingState.Failed:
                // 退出失败状态，隐藏失败提示
                if (fishEscapedNotice != null) fishEscapedNotice.SetActive(false);
                break;
        }
        
        // 更新当前状态
        currentState = newState;
        stateTimer = 0f;
        
        // 进入新状态并触发对应的事件
        switch (newState)
        {
            case FishingState.Idle:
                onIdleState?.Invoke();
                break;
                
            case FishingState.Casting:
                onCastingState?.Invoke();
                break;
                
            case FishingState.Waiting:
                // 进入等待状态，随机生成等待时间
                waitTime = Random.Range(minWaitTime, maxWaitTime);
                onWaitingState?.Invoke();
                break;
                
            case FishingState.Biting:
                // 进入上钩状态，选择鱼并准备小游戏
                PrepareFishForMinigame();
                
                // 播放上钩音效和显示上钩提示
                if (audioSource != null && bitingSound != null)
                {
                    audioSource.PlayOneShot(bitingSound);
                }
                
                if (bitingNoticeUI != null) bitingNoticeUI.SetActive(true);
                onBitingState?.Invoke();
                break;
                
            case FishingState.Minigame:
                // 进入小游戏状态，启动小游戏
                //minigameController.StartGame();
                onMinigameState?.Invoke();
                break;
                
            case FishingState.Rewarding:
                onRewardingState?.Invoke();
                break;
                
            case FishingState.Failed:
                onFailedState?.Invoke();
                break;
        }
    }
    
    /// <summary>
    /// 延迟改变状态的协程
    /// </summary>
    private IEnumerator DelayedStateChange(FishingState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetState(newState);
    }
    
    /// <summary>
    /// 为小游戏准备鱼
    /// </summary>
    private void PrepareFishForMinigame()
    {
        if (fishesManager != null)
        {
            // 让鱼类管理器为小游戏准备鱼
            fishesManager.PrepareFishForMinigame();
        }
        else
        {
            Debug.LogWarning("FishingGameManager: FishesManager引用为空，无法准备鱼类数据！");
        }
    }
    
    /// <summary>
    /// 应用钓竿加成到等待时间
    /// </summary>
    private void ApplyPoleBonus()
    {
        if (poleManager != null)
        {
            // 应用钓竿的等待时间减少
            float waitTimeReduction = poleManager.GetWaitTimeReduction();
            minWaitTime = Mathf.Max(1f, minWaitTime - waitTimeReduction);
            maxWaitTime = Mathf.Max(2f, maxWaitTime - waitTimeReduction);
            
            // 应用钓竿的上钩时间减少
            float bitingTimeReduction = poleManager.GetBitingTimeReduction();
            bitingDuration = Mathf.Max(1f, bitingDuration - bitingTimeReduction);
        }
    }
    
    /// <summary>
    /// 获取当前选中的鱼数据
    /// </summary>
    /// <returns>当前选中的鱼数据</returns>
    public FishData GetCurrentSelectedFishData()
    {
        if (fishesManager != null)
        {
            return fishesManager.GetCurrentSelectedFishData();
        }
        return null;
    }
    
    /// <summary>
    /// 获取当前钓鱼状态
    /// </summary>
    public FishingState GetCurrentState()
    {
        return currentState;
    }
}
