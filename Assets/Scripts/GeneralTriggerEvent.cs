using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* 通用触发器事件 */
// 情况1：识别特定对象
// 情况2：识别对应Tag

[System.Serializable]
public class TriggerEventStruct
{
    public string otherTag;
    public UnityEvent triggerEvents;
}

public class GeneralTriggerEvent : MonoBehaviour
{
    [Header("按Tag识别目标对象")]
    public string defaultTag = "Player";

    [Header("识别指定对象")]
    public bool specificObject = false;
    public GameObject targetObj;

    [Header("触发的事件")]
    public TriggerEventStruct[] enterEvents;
    public TriggerEventStruct[] stayEvents;
    public TriggerEventStruct[] exitEvents;


    private void OnTriggerEnter(Collider other)
    {

        if (specificObject)
        {
            if (other.gameObject == targetObj)
            {
                foreach (TriggerEventStruct triggerEventStruct in enterEvents)
                {
                    triggerEventStruct.triggerEvents?.Invoke();
                }
            }
        }
        else
        {
            foreach (TriggerEventStruct triggerEventStruct in enterEvents)
            {
                string targetTag;

                if (triggerEventStruct.otherTag.Length == 0)
                    targetTag = defaultTag;
                else
                    targetTag = triggerEventStruct.otherTag;

                if (other.CompareTag(targetTag))
                {
                    triggerEventStruct.triggerEvents?.Invoke();
                }
            }
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (specificObject)
        {
            // 情况1：识别特定对象
            if (other.gameObject == targetObj)
            {
                foreach (TriggerEventStruct triggerEventStruct in stayEvents)
                {
                    triggerEventStruct.triggerEvents?.Invoke();
                }
            }
        }
        else
        {
            foreach (TriggerEventStruct triggerEventStruct in stayEvents)
            {
                string targetTag;

                if (triggerEventStruct.otherTag.Length == 0)
                    targetTag = defaultTag;
                else
                    targetTag = triggerEventStruct.otherTag;

                if (other.CompareTag(targetTag))
                {
                    triggerEventStruct.triggerEvents?.Invoke();
                }
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (specificObject)
        {
            if (other.gameObject == targetObj)
            {
                foreach (TriggerEventStruct triggerEventStruct in exitEvents)
                {
                    triggerEventStruct.triggerEvents?.Invoke();
                }
            }
        }
        else
        {
            foreach (TriggerEventStruct triggerEventStruct in exitEvents)
            {
                string targetTag;

                if (triggerEventStruct.otherTag.Length == 0)
                    targetTag = defaultTag;
                else
                    targetTag = triggerEventStruct.otherTag;

                if (other.CompareTag(targetTag))
                {
                    triggerEventStruct.triggerEvents?.Invoke();
                }
            }
        }

    }

}
