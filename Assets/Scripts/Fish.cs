using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 鱼类
/// 定义鱼的基本属性
/// </summary>
public class Fish : MonoBehaviour
{
    [Header("基本属性")]
    [Tooltip("鱼的名称")]
    public string fishName = "普通鱼";
    
    [Tooltip("鱼的图像")]
    public Sprite fishSprite;
    
    [Tooltip("鱼的饱腹度")]
    public int satiety = 20;
    
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
