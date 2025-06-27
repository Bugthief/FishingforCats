using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 橘猫类
/// 继承自Cat基类，具有橘猫的特殊行为特征
/// </summary>
public class OrangeCat : Cat
{
    [Header("橘猫特殊属性")]
    [Tooltip("橘猫的贪吃程度（影响检测范围和移动速度）")]
    [Range(1f, 3f)]
    public float greediness = 2f;
    
    [Tooltip("橘猫的懒惰程度（影响整理时间）")]
    [Range(1f, 2f)]
    public float laziness = 1.5f;
    
    /// <summary>
    /// 初始化组件 - 重写基类方法
    /// </summary>
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // 设置橘猫的默认属性
        breed = "橘猫";
        
        // 橘猫检测范围更大（因为贪吃）
        fishDetectionRange *= greediness;
        
        // 橘猫移动速度更快（因为对食物更积极）
        if (navAgent != null)
        {
            navAgent.speed *= greediness;
        }
    }
    
    /// <summary>
    /// 检测附近的鱼 - 重写基类方法，橘猫更积极地寻找食物
    /// </summary>
    protected override void DetectNearbyFish()
    {
        // 橘猫会检测更大范围内的鱼
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, fishDetectionRange);
        
        Fish closestFish = null;
        float closestDistance = float.MaxValue;
        
        // 橘猫会选择最近的鱼（更贪吃）
        foreach (Collider col in nearbyObjects)
        {
            Fish fish = col.GetComponent<Fish>();
            if (fish != null)
            {
                float distance = Vector3.Distance(transform.position, fish.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFish = fish;
                }
            }
        }
        
        if (closestFish != null && targetFish == null)
        {
            targetFish = closestFish;
            MoveToFish(closestFish);
        }
    }
    
    /// <summary>
    /// 处理进食逻辑 - 重写基类方法，橘猫吃得更多
    /// </summary>
    protected override void HandleEating()
    {
        if (targetFish != null)
        {
            // 橘猫从鱼中获得更多饱腹度
            int bonusAppetite = Mathf.RoundToInt(targetFish.satiety * 0.5f);
            
            // 吃掉鱼
            targetFish.BeEatenByCat(this);
            
            // 额外增加饱腹度（橘猫特殊能力）
            IncreaseAppetite(bonusAppetite);
            
            targetFish = null;
            
            // 触发喂食完成事件
            onFeedingComplete?.Invoke();
            
            // 进食完成后，进入整理状态
            SetState(CatState.Grooming);
            
            // 橘猫整理时间更长（因为懒惰）
            StartCoroutine(DelayedStateChange(CatState.Waiting, GetGroomingDuration()));
        }
        else
        {
            // 没有鱼了，返回等待状态
            SetState(CatState.Waiting);
        }
    }
    
    /// <summary>
    /// 获取整理动画持续时间 - 重写基类方法，橘猫更懒
    /// </summary>
    protected override float GetGroomingDuration()
    {
        return base.GetGroomingDuration() * laziness;
    }
    
    /// <summary>
    /// 更新耐心值 - 重写基类方法，橘猫在有食物时耐心更好
    /// </summary>
    protected override void UpdatePatience()
    {
        if (currentState != CatState.Eating && currentState != CatState.Grooming && currentState != CatState.Leaving)
        {
            // 如果附近有鱼，橘猫的耐心下降得更慢
            float patienceRate = patienceDecreaseRate;
            if (HasNearbyFish())
            {
                patienceRate *= 0.5f; // 耐心下降速度减半
            }
            
            patience -= patienceRate * Time.deltaTime;
            patience = Mathf.Clamp(patience, 0, 100);
        }
    }
    
    /// <summary>
    /// 检查附近是否有鱼
    /// </summary>
    private bool HasNearbyFish()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, fishDetectionRange);
        
        foreach (Collider col in nearbyObjects)
        {
            if (col.GetComponent<Fish>() != null)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 更新信息面板 - 重写基类方法，显示橘猫特殊信息
    /// </summary>
    protected override void UpdateInfoPanel()
    {
        Debug.Log($"品种: {breed}, 胃口: {appetite}/100, 耐心: {patience}/100, 贪吃程度: {greediness}, 懒惰程度: {laziness}");
    }
    
    /// <summary>
    /// 在Scene视图中绘制检测范围 - 重写基类方法，橘猫用橙色显示
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // 橘猫用橙红色显示检测范围
        Gizmos.DrawWireSphere(transform.position, fishDetectionRange);
    }
}
