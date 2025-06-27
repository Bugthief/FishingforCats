using UnityEngine;

/// <summary>
/// 鱼类数据ScriptableObject
/// 存储鱼的基本属性，可以被多个鱼实例共享
/// </summary>
[CreateAssetMenu(fileName = "New Fish Data", menuName = "Fishing Game/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("鱼的名称")]
    public string fishName = "普通鱼";
    
    [Tooltip("鱼的描述")]
    [TextArea(2, 4)]
    public string description = "一条普通的鱼";
    
    [Tooltip("鱼的图像")]
    public Sprite fishSprite;
    
    [Header("游戏属性")]
    [Tooltip("鱼的饱腹度")]
    [Range(1, 100)]
    public int satiety = 20;
    
    [Tooltip("鱼的难度等级（1-5）")]
    [Range(1, 5)]
    public int difficultyLevel = 1;
    
    [Tooltip("鱼的稀有度")]
    public FishRarity rarity = FishRarity.Common;
    
    [Header("出现条件")]
    [Tooltip("出现概率权重")]
    [Range(1, 100)]
    public int spawnWeight = 10;
    
    [Tooltip("最低钓竿等级要求")]
    [Range(1, 5)]
    public int minPoleLevel = 1;
    
    [Tooltip("最高钓竿等级要求（0表示无限制）")]
    [Range(0, 5)]
    public int maxPoleLevel = 0;
    
    [Header("视觉效果")]
    [Tooltip("鱼的颜色")]
    public Color fishColor = Color.white;
    
    [Tooltip("鱼的大小缩放")]
    [Range(0.5f, 2f)]
    public float sizeScale = 1f;
    
    [Header("音效")]
    [Tooltip("钓到这条鱼时的音效")]
    public AudioClip catchSound;
    
    /// <summary>
    /// 检查是否满足钓竿等级要求
    /// </summary>
    /// <param name="poleLevel">钓竿等级</param>
    /// <returns>是否满足要求</returns>
    public bool MeetsPoleRequirement(int poleLevel)
    {
        if (poleLevel < minPoleLevel)
            return false;
            
        if (maxPoleLevel > 0 && poleLevel > maxPoleLevel)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// 获取稀有度颜色
    /// </summary>
    /// <returns>稀有度对应的颜色</returns>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case FishRarity.Common:
                return Color.white;
            case FishRarity.Uncommon:
                return Color.green;
            case FishRarity.Rare:
                return Color.blue;
            case FishRarity.Epic:
                return Color.magenta;
            case FishRarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
}

/// <summary>
/// 鱼的稀有度枚举
/// </summary>
public enum FishRarity
{
    Common,     // 普通
    Uncommon,   // 不常见
    Rare,       // 稀有
    Epic,       // 史诗
    Legendary   // 传说
}
