using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

/// <summary>
/// 猫基类
/// 定义猫的基本属性和行为，可被不同种类的猫继承
/// </summary>
public class Cat : MonoBehaviour
{
    // 猫的状态枚举
    public enum CatState
    {
        Appearing,      // 出现
        Waiting,        // 等待
        Moving,         // 移动
        Eating,         // 进食
        Grooming,       // 整理（进食完成）
        Leaving         // 离开
    }
    
    [Header("基本属性")]
    [Tooltip("猫的品种")]
    public string breed = "普通猫";
    
    [Tooltip("胃口（0-100）")]
    [Range(0, 100)]
    public float appetite = 0f;
    
    [Tooltip("耐心（100-0，初始为10）")]
    [Range(0, 100)]
    public float patience = 10f;
    
    [Header("游戏设置")]
    [Tooltip("耐心下降速率（每秒）")]
    public float patienceDecreaseRate = 1f;
    
    [Tooltip("检测鱼的范围")]
    public float fishDetectionRange = 5f;
    
    [Header("组件引用")]
    [Tooltip("NavMeshAgent组件")]
    public NavMeshAgent navAgent;
    
    [Tooltip("动画控制器")]
    public Animator animator;
    
    [Tooltip("猫的信息面板")]
    public GameObject infoPanel;
    
    [Header("事件")]
    [Tooltip("喂食后触发的事件")]
    public UnityEvent onFeedingComplete;
    
    [Tooltip("猫离开时触发的事件")]
    public UnityEvent onCatLeave;
    
    [Tooltip("猫出现时触发的事件")]
    public UnityEvent onCatAppear;
    
    // 受保护的变量，供子类访问
    protected CatState currentState = CatState.Appearing;
    protected Fish targetFish = null;
    protected bool isInfoPanelVisible = false;
    
    /// <summary>
    /// 初始化 - 虚方法，子类可重写
    /// </summary>
    protected virtual void Start()
    {
        InitializeComponents();
        InitializeUI();
        InitializeState();
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    protected virtual void InitializeComponents()
    {
        // 获取NavMeshAgent组件
        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        
        // 获取Animator组件
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    protected virtual void InitializeUI()
    {
        // 隐藏信息面板
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 初始化状态
    /// </summary>
    protected virtual void InitializeState()
    {
        // 初始状态为出现
        SetState(CatState.Appearing);
        
        // 触发猫出现事件
        onCatAppear?.Invoke();
        
        // 延迟后进入等待状态
        StartCoroutine(DelayedStateChange(CatState.Waiting, GetAppearDuration()));
    }
    
    /// <summary>
    /// 获取出现动画持续时间 - 虚方法，子类可重写
    /// </summary>
    protected virtual float GetAppearDuration()
    {
        return 2f;
    }
    
    /// <summary>
    /// 游戏主循环 - 虚方法，子类可重写
    /// </summary>
    protected virtual void Update()
    {
        // 更新耐心值
        UpdatePatience();
        
        // 根据当前状态更新行为
        UpdateStateBehavior();
        
        // 检查耐心是否耗尽
        CheckPatienceLimit();
    }
    
    /// <summary>
    /// 更新耐心值 - 虚方法，子类可重写
    /// </summary>
    protected virtual void UpdatePatience()
    {
        if (currentState != CatState.Eating && currentState != CatState.Grooming && currentState != CatState.Leaving)
        {
            patience -= patienceDecreaseRate * Time.deltaTime;
            patience = Mathf.Clamp(patience, 0, 100);
        }
    }
    
    /// <summary>
    /// 根据状态更新行为
    /// </summary>
    protected virtual void UpdateStateBehavior()
    {
        switch (currentState)
        {
            case CatState.Waiting:
                OnWaitingState();
                break;
                
            case CatState.Moving:
                OnMovingState();
                break;
                
            case CatState.Eating:
                OnEatingState();
                break;
        }
    }
    
    /// <summary>
    /// 等待状态行为 - 虚方法，子类可重写
    /// </summary>
    protected virtual void OnWaitingState()
    {
        DetectNearbyFish();
    }
    
    /// <summary>
    /// 移动状态行为 - 虚方法，子类可重写
    /// </summary>
    protected virtual void OnMovingState()
    {
        CheckMovementComplete();
    }
    
    /// <summary>
    /// 进食状态行为 - 虚方法，子类可重写
    /// </summary>
    protected virtual void OnEatingState()
    {
        HandleEating();
    }
    
    /// <summary>
    /// 检查耐心限制
    /// </summary>
    protected virtual void CheckPatienceLimit()
    {
        if (patience <= 0 && currentState != CatState.Leaving)
        {
            SetState(CatState.Leaving);
        }
    }
    
    /// <summary>
    /// 检测附近的鱼 - 虚方法，子类可重写
    /// </summary>
    protected virtual void DetectNearbyFish()
    {
        // 查找附近的鱼
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, fishDetectionRange);
        
        foreach (Collider col in nearbyObjects)
        {
            Fish fish = col.GetComponent<Fish>();
            if (fish != null && targetFish == null)
            {
                // 找到鱼，开始移动
                targetFish = fish;
                MoveToFish(fish);
                break;
            }
        }
    }
    
    /// <summary>
    /// 移动到鱼的位置 - 虚方法，子类可重写
    /// </summary>
    protected virtual void MoveToFish(Fish fish)
    {
        if (navAgent != null && fish != null)
        {
            navAgent.SetDestination(fish.transform.position);
            SetState(CatState.Moving);
        }
    }
    
    /// <summary>
    /// 检查移动是否完成
    /// </summary>
    protected virtual void CheckMovementComplete()
    {
        if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.5f)
        {
            // 到达目标位置，开始进食
            SetState(CatState.Eating);
        }
    }
    
    /// <summary>
    /// 处理进食逻辑 - 虚方法，子类可重写
    /// </summary>
    protected virtual void HandleEating()
    {
        if (targetFish != null)
        {
            // 吃掉鱼
            targetFish.BeEatenByCat(this);
            targetFish = null;
            
            // 触发喂食完成事件
            onFeedingComplete?.Invoke();
            
            // 进食完成后，进入整理状态
            SetState(CatState.Grooming);
            
            // 延迟后返回等待状态
            StartCoroutine(DelayedStateChange(CatState.Waiting, GetGroomingDuration()));
        }
        else
        {
            // 没有鱼了，返回等待状态
            SetState(CatState.Waiting);
        }
    }
    
    /// <summary>
    /// 获取整理动画持续时间 - 虚方法，子类可重写
    /// </summary>
    protected virtual float GetGroomingDuration()
    {
        return 3f;
    }
    
    /// <summary>
    /// 设置猫的状态 - 虚方法，子类可重写
    /// </summary>
    protected virtual void SetState(CatState newState)
    {
        currentState = newState;
        
        // 根据状态设置动画
        SetAnimationState(newState);
        
        // 处理特殊状态
        HandleSpecialStates(newState);
    }
    
    /// <summary>
    /// 设置动画状态 - 虚方法，子类可重写
    /// </summary>
    protected virtual void SetAnimationState(CatState state)
    {
        if (animator != null)
        {
            switch (state)
            {
                case CatState.Appearing:
                    animator.SetTrigger("Appear");
                    break;
                case CatState.Waiting:
                    animator.SetTrigger("Wait");
                    break;
                case CatState.Moving:
                    animator.SetTrigger("Move");
                    break;
                case CatState.Eating:
                    animator.SetTrigger("Eat");
                    break;
                case CatState.Grooming:
                    animator.SetTrigger("Groom");
                    break;
                case CatState.Leaving:
                    animator.SetTrigger("Leave");
                    break;
            }
        }
    }
    
    /// <summary>
    /// 处理特殊状态 - 虚方法，子类可重写
    /// </summary>
    protected virtual void HandleSpecialStates(CatState state)
    {
        if (state == CatState.Leaving)
        {
            // 延迟销毁猫对象
            StartCoroutine(DelayedDestroy(GetLeaveDuration()));
        }
    }
    
    /// <summary>
    /// 获取离开动画持续时间 - 虚方法，子类可重写
    /// </summary>
    protected virtual float GetLeaveDuration()
    {
        return 3f;
    }
    
    /// <summary>
    /// 增加胃口 - 虚方法，子类可重写
    /// </summary>
    public virtual void IncreaseAppetite(int amount)
    {
        appetite += amount;
        appetite = Mathf.Clamp(appetite, 0, 100);
    }
    
    /// <summary>
    /// 鼠标点击显示信息
    /// </summary>
    protected virtual void OnMouseDown()
    {
        ToggleInfoPanel();
    }
    
    /// <summary>
    /// 切换信息面板显示 - 虚方法，子类可重写
    /// </summary>
    protected virtual void ToggleInfoPanel()
    {
        if (infoPanel != null)
        {
            isInfoPanelVisible = !isInfoPanelVisible;
            infoPanel.SetActive(isInfoPanelVisible);
            
            if (isInfoPanelVisible)
            {
                UpdateInfoPanel();
            }
        }
    }
    
    /// <summary>
    /// 更新信息面板 - 虚方法，子类可重写
    /// </summary>
    protected virtual void UpdateInfoPanel()
    {
        // 这里可以更新信息面板的内容
        // 需要根据实际的UI组件来实现
        Debug.Log($"品种: {breed}, 胃口: {appetite}/100, 耐心: {patience}/100");
    }
    
    /// <summary>
    /// 延迟改变状态的协程
    /// </summary>
    protected IEnumerator DelayedStateChange(CatState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetState(newState);
    }
    
    /// <summary>
    /// 延迟销毁的协程
    /// </summary>
    protected IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        onCatLeave?.Invoke();
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 在Scene视图中绘制检测范围
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fishDetectionRange);
    }
    
    /// <summary>
    /// 获取当前状态 - 公共方法
    /// </summary>
    public CatState GetCurrentState()
    {
        return currentState;
    }
}
