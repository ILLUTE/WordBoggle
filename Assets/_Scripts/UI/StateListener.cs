using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StateListener : MonoBehaviour
{
    public List<GameState> m_State = new List<GameState>();
    [SerializeField]
    private Canvas m_Canvas;

    private void Awake()
    {
        if (m_Canvas == null)
        {
            m_Canvas = GetComponent<Canvas>();
        }
        GameManager.SwitchState += OnButtonPressed;
    }

    private void OnButtonPressed(GameState state)
    {
        m_Canvas.enabled = m_State.Contains(state);
    }

    private void OnDestroy()
    {
        GameManager.SwitchState -= OnButtonPressed;
    }
}
