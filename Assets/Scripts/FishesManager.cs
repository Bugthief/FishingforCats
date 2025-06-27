using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 鱼类管理器
/// 管理所有鱼类数据，能够根据钓竿等级和概率选择合适的鱼
/// </summary>
public class FishesManager : MonoBehaviour
{
    [Header("鱼类数据配置")]
    [Tooltip("所有可用的鱼类数据")]
    public FishData[] availableFishData;
    
    [Header("组件引用")]
    [Tooltip("钓鱼小游戏管理器")]
    public FishingMinigame minigameManager;
    
    [Tooltip("钓竿管理器")]
    public PoleManager poleManager;
    
    [Header("鱼实例管理")]
    [Tooltip("鱼的预制体（用于生成实体）")]
    public GameObject fishPrefab;
    
    [Tooltip("生成鱼的父对象")]
    public Transform fishParent;
    
    // 私有变量
    private FishData currentSelectedFishData;
    private Fish currentFishInstance;
    
    /// <summary>
    /// 初始化
    /// </summary>
    void Start()
    {
        // 自动查找组件引用
        if (minigameManager == null)
        {
            minigameManager = FindObjectOfType<FishingMinigame>();
        }
        
        if (poleManager == null)
        {
            poleManager = FindObjectOfType<PoleManager>();
        }
        
        // 验证配置
        if (availableFishData.Length == 0)
        {
            Debug.LogWarning("FishesManager: 没有配置任何鱼类数据！");
        }
        
        if (fishPrefab == null)
        {
            Debug.LogWarning("FishesManager: 没有配置鱼的预制体！");
        }
    }
    
    /// <summary>
    /// 选择一条鱼（根据钓竿等级和概率权重）
    /// </summary>
    /// <returns>选中的鱼数据</returns>
    public FishData SelectFishData()
    {
        if (availableFishData.Length == 0)
        {
            Debug.LogWarning("FishesManager: 没有可用的鱼类数据！");
            return null;
        }
        
        // 获取当前钓竿等级
        int currentPoleLevel = poleManager != null ? poleManager.GetCurrentPoleLevel() : 1;
        
        // 筛选符合钓竿等级要求的鱼
        List<FishData> eligibleFish = new List<FishData>();
        List<int> weights = new List<int>();
        
        foreach (FishData fishData in availableFishData)
        {
            if (fishData != null && fishData.MeetsPoleRequirement(currentPoleLevel))
            {
                eligibleFish.Add(fishData);
                weights.Add(fishData.spawnWeight);
            }
        }
        
        if (eligibleFish.Count == 0)
        {
            Debug.LogWarning($"FishesManager: 没有符合钓竿等级 {currentPoleLevel} 的鱼类！");
            return null;
        }
        
        // 根据权重随机选择
        FishData selectedFish = SelectWeightedRandom(eligibleFish, weights);
        currentSelectedFishData = selectedFish;
        
        Debug.Log($"选中鱼类: {selectedFish.fishName} (难度: {selectedFish.difficultyLevel}, 稀有度: {selectedFish.rarity})");
        
        return selectedFish;
    }
    
    /// <summary>
    /// 根据权重随机选择
    /// </summary>
    /// <param name="items">候选项列表</param>
    /// <param name="weights">权重列表</param>
    /// <returns>选中的项</returns>
    private FishData SelectWeightedRandom(List<FishData> items, List<int> weights)
    {
        if (items.Count == 0 || items.Count != weights.Count)
        {
            return null;
        }
        
        // 计算总权重
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }
        
        // 随机选择
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        for (int i = 0; i < items.Count; i++)
        {
            currentWeight += weights[i];
            if (randomValue < currentWeight)
            {
                return items[i];
            }
        }
        
        // 备用返回最后一个
        return items[items.Count - 1];
    }
    
    /// <summary>
    /// 创建鱼实例（用于钓鱼小游戏）
    /// </summary>
    /// <param name="fishData">鱼的数据</param>
    /// <param name="position">生成位置</param>
    /// <returns>创建的鱼实例</returns>
    public Fish CreateFishInstance(FishData fishData, Vector3 position)
    {
        if (fishPrefab == null || fishData == null)
        {
            Debug.LogError("FishesManager: 无法创建鱼实例，缺少预制体或数据！");
            return null;
        }
        
        // 实例化鱼对象
        GameObject fishObject = Instantiate(fishPrefab, position, Quaternion.identity);
        
        // 设置父对象
        if (fishParent != null)
        {
            fishObject.transform.SetParent(fishParent);
        }
        
        // 获取Fish组件并设置数据
        Fish fishComponent = fishObject.GetComponent<Fish>();
        if (fishComponent != null)
        {
            fishComponent.SetFishData(fishData);
            currentFishInstance = fishComponent;
        }
        else
        {
            Debug.LogError("FishesManager: 鱼预制体缺少Fish组件！");
        }
        
        return fishComponent;
    }
    
    /// <summary>
    /// 为钓鱼小游戏准备鱼
    /// </summary>
    public void PrepareFishForMinigame()
    {
        // 选择鱼数据
        FishData selectedFishData = SelectFishData();
        
        if (selectedFishData != null && minigameManager != null)
        {
            // 创建临时鱼实例用于小游戏（不显示，只用于数据）
            GameObject tempFishObject = new GameObject("TempFish");
            Fish tempFish = tempFishObject.AddComponent<Fish>();
            tempFish.SetFishData(selectedFishData);
            
            // 将鱼数据传递给小游戏
            minigameManager.fishScript = tempFish;
            
            Debug.Log($"为小游戏准备了鱼: {selectedFishData.fishName}");
        }
    }
    
    /// <summary>
    /// 获取当前选中的鱼数据
    /// </summary>
    /// <returns>当前选中的鱼数据</returns>
    public FishData GetCurrentSelectedFishData()
    {
        return currentSelectedFishData;
    }
    
    /// <summary>
    /// 获取当前鱼实例
    /// </summary>
    /// <returns>当前鱼实例</returns>
    public Fish GetCurrentFishInstance()
    {
        return currentFishInstance;
    }
    
    /// <summary>
    /// 清理当前鱼实例
    /// </summary>
    public void ClearCurrentFishInstance()
    {
        if (currentFishInstance != null)
        {
            Destroy(currentFishInstance.gameObject);
            currentFishInstance = null;
        }
        currentSelectedFishData = null;
    }
    
    /// <summary>
    /// 获取指定稀有度的鱼类数量
    /// </summary>
    /// <param name="rarity">稀有度</param>
    /// <returns>该稀有度的鱼类数量</returns>
    public int GetFishCountByRarity(FishRarity rarity)
    {
        int count = 0;
        foreach (FishData fishData in availableFishData)
        {
            if (fishData != null && fishData.rarity == rarity)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// 获取所有鱼类名称列表
    /// </summary>
    /// <returns>鱼类名称数组</returns>
    public string[] GetAllFishNames()
    {
        string[] names = new string[availableFishData.Length];
        for (int i = 0; i < availableFishData.Length; i++)
        {
            names[i] = availableFishData[i] != null ? availableFishData[i].fishName : "未知鱼类";
        }
        return names;
    }
}
