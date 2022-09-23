using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    public static Screenshot instance;
    public KeyCode captureBinding = KeyCode.F2;

    private void Awake()
    {
        if(instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(captureBinding))
        {
            Capture("screenshot");
        }
    }
    public void Capture(string fileName)
    {
        ScreenCapture.CaptureScreenshot(fileName + ".png");
    }
}
