using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_Text;

    public int levelIndex;

    private void Start()
    {
        m_Text.text = (levelIndex + 1).ToString();
    }
    public void LoadLevel()
    {
        GameManager.Instance.SetCurrentLevelIndex(levelIndex);
    }
}
