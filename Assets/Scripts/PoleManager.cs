using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 钓竿管理器
/// 管理所有类型的钓竿，根据玩家装备的钓竿获取对应等级和参数
/// </summary>
public class PoleManager : MonoBehaviour
{
    [System.Serializable]
    public class PoleData
    {
        [Tooltip("钓竿名称")]
        public string poleName;
        
        [Tooltip("钓竿等级")]
        [Range(1, 5)]
        public int poleLevel = 1;
        
        [Tooltip("钓竿游戏对象")]
        public GameObject poleObject;
        
        [Tooltip("成功率加成（0-1）")]
        [Range(0f, 1f)]
        public float successBonus = 0f;
        
        [Tooltip("鱼上钩时间减少（秒）")]
        public float bitingTimeReduction = 0f;
        
        [Tooltip("等待时间减少（秒）")]
        public float waitTimeReduction = 0f;
    }
    
    [Header("钓竿配置")]
    [Tooltip("所有可用的钓竿")]
    public PoleData[] availablePoles;
    
    [Header("当前装备")]
    [Tooltip("当前装备的钓竿索引")]
    public int currentPoleIndex = 0;
    
    // 私有变量
    private PoleData currentPole;
    
    /// <summary>
    /// 初始化
    /// </summary>
    private void Start()
    {
        // 设置初始钓竿
        if (availablePoles.Length > 0)
        {
            SetCurrentPole(currentPoleIndex);
        }
        else
        {
            Debug.LogWarning("PoleManager: 没有配置任何钓竿！");
        }
    }
    
    /// <summary>
    /// 设置当前使用的钓竿
    /// </summary>
    /// <param name="poleIndex">钓竿索引</param>
    public void SetCurrentPole(int poleIndex)
    {
        if (poleIndex >= 0 && poleIndex < availablePoles.Length)
        {
            currentPoleIndex = poleIndex;
            currentPole = availablePoles[poleIndex];
            
            // 激活当前钓竿，隐藏其他钓竿
            ActivateCurrentPole();
            
            Debug.Log($"切换到钓竿: {currentPole.poleName} (等级 {currentPole.poleLevel})");
        }
        else
        {
            Debug.LogWarning($"PoleManager: 无效的钓竿索引 {poleIndex}");
        }
    }
    
    /// <summary>
    /// 激活当前钓竿，隐藏其他钓竿
    /// </summary>
    private void ActivateCurrentPole()
    {
        for (int i = 0; i < availablePoles.Length; i++)
        {
            if (availablePoles[i].poleObject != null)
            {
                availablePoles[i].poleObject.SetActive(i == currentPoleIndex);
            }
        }
    }
    
    /// <summary>
    /// 获取当前钓竿数据
    /// </summary>
    /// <returns>当前钓竿数据</returns>
    public PoleData GetCurrentPole()
    {
        return currentPole;
    }
    
    /// <summary>
    /// 获取当前钓竿等级
    /// </summary>
    /// <returns>钓竿等级</returns>
    public int GetCurrentPoleLevel()
    {
        return currentPole != null ? currentPole.poleLevel : 1;
    }
    
    /// <summary>
    /// 获取当前钓竿名称
    /// </summary>
    /// <returns>钓竿名称</returns>
    public string GetCurrentPoleName()
    {
        return currentPole != null ? currentPole.poleName : "无钓竿";
    }
    
    /// <summary>
    /// 获取成功率加成
    /// </summary>
    /// <returns>成功率加成</returns>
    public float GetSuccessBonus()
    {
        return currentPole != null ? currentPole.successBonus : 0f;
    }
    
    /// <summary>
    /// 获取上钩时间减少
    /// </summary>
    /// <returns>上钩时间减少（秒）</returns>
    public float GetBitingTimeReduction()
    {
        return currentPole != null ? currentPole.bitingTimeReduction : 0f;
    }
    
    /// <summary>
    /// 获取等待时间减少
    /// </summary>
    /// <returns>等待时间减少（秒）</returns>
    public float GetWaitTimeReduction()
    {
        return currentPole != null ? currentPole.waitTimeReduction : 0f;
    }
    
    /// <summary>
    /// 切换到下一个钓竿
    /// </summary>
    public void SwitchToNextPole()
    {
        if (availablePoles.Length > 1)
        {
            int nextIndex = (currentPoleIndex + 1) % availablePoles.Length;
            SetCurrentPole(nextIndex);
        }
    }
    
    /// <summary>
    /// 切换到上一个钓竿
    /// </summary>
    public void SwitchToPreviousPole()
    {
        if (availablePoles.Length > 1)
        {
            int prevIndex = (currentPoleIndex - 1 + availablePoles.Length) % availablePoles.Length;
            SetCurrentPole(prevIndex);
        }
    }
    
    /// <summary>
    /// 检查是否有指定等级的钓竿
    /// </summary>
    /// <param name="level">钓竿等级</param>
    /// <returns>是否有该等级的钓竿</returns>
    public bool HasPoleOfLevel(int level)
    {
        foreach (var pole in availablePoles)
        {
            if (pole.poleLevel == level)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 获取所有可用钓竿的名称列表
    /// </summary>
    /// <returns>钓竿名称列表</returns>
    public string[] GetAllPoleNames()
    {
        string[] names = new string[availablePoles.Length];
        for (int i = 0; i < availablePoles.Length; i++)
        {
            names[i] = availablePoles[i].poleName;
        }
        return names;
    }
}
