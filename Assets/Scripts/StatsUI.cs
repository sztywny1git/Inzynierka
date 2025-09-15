using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StatsUI : MonoBehaviour
{
    public GameObject[] statsSlots;
    public CanvasGroup statsCanvas;

    private bool statsOpen = true;

    private void Start()
    {
        UpdateAllStats();
    }

    private void Update()
    {
        if (Input.GetButtonDown("ToggleStats"))
        {
            if (statsOpen)
            {
                //Time.timeScale = 1; //pauzuje gre
                statsCanvas.alpha = 0;
                statsOpen = false;
            }
            else
            {
                //Time.timeScale = 1; //odpauzuje gre
                statsCanvas.alpha = 1;
                statsOpen = true;
            }
        }
    }

    public void UpdateSpeed()
    {
        statsSlots[0].GetComponentInChildren<TMP_Text>().text = "Move speed: " + StatsManager.Instance.moveSpeed;
    }
    public void UpdateFireRate()
    {
        statsSlots[1].GetComponentInChildren<TMP_Text>().text = "Fire rate: " + StatsManager.Instance.fireRate;
    }

    public void UpdateAllStats()
    {
        UpdateSpeed();
        UpdateFireRate();
    }
}
