using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    private const float hoursToDegrees = -30.0f, minuteToDegrees = -6.0f, secondToDegrees = -6.0f;
    
    [SerializeField]
    private Transform hoursPivot, minutesPivot, secondsPivot;
    void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // 一下一下走
        // DateTime time = DateTime.Now;
        // hoursPivot.localRotation = Quaternion.Euler(0f,0f,hoursToDegrees * time.Hour);
        // minutesPivot.localRotation = Quaternion.Euler(0f,0f,minuteToDegrees * time.Minute);
        // secondsPivot.localRotation = Quaternion.Euler(0f,0f,secondToDegrees * time.Second);
        
        // 连续走
        TimeSpan time = DateTime.Now.TimeOfDay;
        Debug.Log(DateTime.Now.TimeOfDay);
        hoursPivot.localRotation = Quaternion.Euler(0f,0f,hoursToDegrees * (float)time.TotalHours);
        minutesPivot.localRotation = Quaternion.Euler(0f,0f,minuteToDegrees * (float)time.TotalMinutes);
        secondsPivot.localRotation = Quaternion.Euler(0f,0f,secondToDegrees * (float)time.TotalSeconds);
        
    }
}
