using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameButtons : MonoBehaviour
{
    public ButtonType buttonType;

    public void OnButtonPressed()
    {
        GameManager.Instance.ButtonPressed(buttonType);
    }
}
