using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 鱼类
/// 使用FishData ScriptableObject存储鱼的属性数据
/// </summary>
public class Fish : MonoBehaviour
{
    [Header("鱼类数据")]
    [Tooltip("鱼的数据资源")]
    public FishData fishData;
    
    [Header("运行时组件")]
    [Tooltip("鱼的SpriteRenderer组件")]
    public SpriteRenderer spriteRenderer;
    
    [Tooltip("鱼的AudioSource组件")]
    public AudioSource audioSource;
    
    // 属性访问器，方便其他脚本获取数据
    public string fishName => fishData != null ? fishData.fishName : "未知鱼类";
    public Sprite fishSprite => fishData != null ? fishData.fishSprite : null;
    public int satiety => fishData != null ? fishData.satiety : 0;
    public int difficultyLevel => fishData != null ? fishData.difficultyLevel : 1;
    public FishRarity rarity => fishData != null ? fishData.rarity : FishRarity.Common;
    public string description => fishData != null ? fishData.description : "";
    public Color fishColor => fishData != null ? fishData.fishColor : Color.white;
    public float sizeScale => fishData != null ? fishData.sizeScale : 1f;
    public AudioClip catchSound => fishData != null ? fishData.catchSound : null;
    
    /// <summary>
    /// 初始化鱼的外观和属性
    /// </summary>
    private void Start()
    {
        InitializeFish();
    }
    
    /// <summary>
    /// 根据FishData初始化鱼的外观
    /// </summary>
    public void InitializeFish()
    {
        if (fishData == null)
        {
            Debug.LogWarning($"Fish {gameObject.name}: 没有分配FishData！");
            return;
        }
        
        // 设置精灵图像
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null && fishData.fishSprite != null)
        {
            spriteRenderer.sprite = fishData.fishSprite;
            spriteRenderer.color = fishData.fishColor;
        }
        
        // 设置大小
        transform.localScale = Vector3.one * fishData.sizeScale;
        
        // 设置音效
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// 设置鱼的数据（用于运行时创建鱼实例）
    /// </summary>
    /// <param name="data">鱼的数据</param>
    public void SetFishData(FishData data)
    {
        fishData = data;
        InitializeFish();
    }
    
    /// <summary>
    /// 播放钓到鱼的音效
    /// </summary>
    public void PlayCatchSound()
    {
        if (audioSource != null && catchSound != null)
        {
            audioSource.PlayOneShot(catchSound);
        }
    }
    
    /// <summary>
    /// 被猫吃掉
    /// </summary>
    public void BeEatenByCat(Cat cat)
    {
        // 增加猫的胃口
        if (cat != null)
        {
            cat.IncreaseAppetite(satiety);
        }
        
        // 销毁自己
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 回收到鱼框
    /// </summary>
    public void ReturnToBasket()
    {
        // 这里可以添加回收到鱼框的逻辑
        // 具体实现可能需要与鱼篮系统配合
        Debug.Log($"{fishName} 被回收到鱼框");
    }
}
