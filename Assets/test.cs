using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log(other.name);
    }
}
