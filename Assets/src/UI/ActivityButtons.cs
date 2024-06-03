using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivityButtons : MonoBehaviour
{
    private List<Button> buttons; 
    void Awake()
    {
        buttons = new List<Button>();
        foreach(Transform child in transform)
        {
            Button button = child.GetComponent<Button>();
            buttons.Add(button);
        }
    }

    public void SetButtonsInteractable(bool value)
    {
        foreach(Button button in buttons)
        {
            button.interactable = value;
        }
    }
}

