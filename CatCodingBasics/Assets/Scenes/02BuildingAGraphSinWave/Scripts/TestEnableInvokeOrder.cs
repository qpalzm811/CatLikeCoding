using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnableInvokeOrder : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("awake");
    }

    private void OnEnable()
    {
        Debug.Log("enable");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
