using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UltimateEq : MonoBehaviour
{
    public CanvasGroup UltimateEqCanvas;
    private bool UltimateEqPanel = true;

    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetButtonDown("ToggleUltimateEq"))
        {
            if (UltimateEqPanel)
            {
                Time.timeScale = 1; //pazuje gre
                UltimateEqCanvas.alpha = 0;
                UltimateEqCanvas.blocksRaycasts = false;
                UltimateEqPanel = false;
            }
            else
            {
                Time.timeScale = 1; //odpauzuje gre
                UltimateEqCanvas.alpha = 1;
                UltimateEqCanvas.blocksRaycasts = true;
                UltimateEqPanel = true;
            }
        }
    }
}
