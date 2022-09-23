using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    public static ProgressBar instance;
    public Slider progressSlider;
    public TextMeshProUGUI actionNameText;

    public bool doingAction = false;
    private int currentPriority = -1;

    private void Start()
    {
        progressSlider.gameObject.SetActive(false);
        actionNameText.gameObject.SetActive(false);
        instance = this;
    }

    private void OnEnable()
    {
        PlayerManager.OnLocalPlayerControllerDeSpawned += EndProgress;
    }

    private void OnDisable()
    {
        PlayerManager.OnLocalPlayerControllerDeSpawned -= EndProgress;
    }

    private void Update()
    {
        if (doingAction)
        {
            progressSlider.value -= Time.deltaTime;
            if(progressSlider.value <= 0)
            {
                EndProgress();
            }
        }
    }

    public void StartProgressBar(string actionName, float actionDuration, int priority)
    {
        //Reloading has priority of 0, planting/ defusing is 1 etc...

        if (priority >= currentPriority)
        {
            doingAction = true;
            progressSlider.gameObject.SetActive(true);
            actionNameText.gameObject.SetActive(true);
            progressSlider.maxValue = actionDuration;
            progressSlider.value = actionDuration;
            actionNameText.text = actionName;
        }
    }

    public void EndProgress()
    {
        doingAction = false;
        progressSlider.gameObject.SetActive(false);
        actionNameText.gameObject.SetActive(false);
        currentPriority = -1;
    }
}
