using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeviceManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("zzzzz Init Device, position: " + transform.position);
        //===== Turn the note to point at the camera
        
        Camera userCamera = Camera.main;
        Vector3 targetPosition = new Vector3(userCamera.transform.position.x,
            userCamera.transform.position.y,
            userCamera.transform.position.z);
        transform.LookAt(targetPosition);
        transform.Rotate(0f, -180f, 0f);
        direction.text = "Sensor distance: " + Vector3.Distance(transform.position, userCamera.transform.position) * 2; //112 расстояние между ближней позицией
    }

    public string id;
    public Transform root;
    
    public Text h1;
    public Text t1;
    public Text type;
    public Text direction;

    public void ShowValue(float newH1, float newT1, string newType)
    {
        h1.text = "T1: " + newH1;
        t1.text = "H1: " + newT1;
        type.text = newType;
    }

}
