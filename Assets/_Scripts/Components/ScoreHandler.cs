using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    public int totalScore = 0;
    public int wordsAmount = 0;
    public float m_ElapsedTime = 0;

    [Header("Textmesh Refrences")]
    public TextMeshProUGUI m_TotalScoreText;
    public TextMeshProUGUI m_ObjectiveText;
    public TextMeshProUGUI m_ElapsedTimeText;
    public TextMeshProUGUI m_ScorePerWordText;

    [Header("Rectransforms")] //  Todo If have time, maybe Tween these at start?
    public RectTransform m_TimerFrame;
    public RectTransform m_ObjectiveFrame;

    private void Awake()
    {
        GameManager.OnWordMade += OnUpdateScore;
        GameManager.LoadGame += ResetUI;
        GameManager.OnObjectiveResolved += ObjectiveResolved;
        GameManager.OnTimerChanged += TimerChanged;
    }

    private void TimerChanged(float elapsedTime)
    {
        m_ElapsedTime = elapsedTime;
        UpdateUI();
    }

    private void ObjectiveResolved(Objective objective, Conditions condition)
    {
        if (condition.m_ConditionMet)
        {
            m_ObjectiveText.text = ("<color=green>Objective Met </color>");
        }
        else
        {
            m_ObjectiveText.text = "<color=red> Objective Failed </color>";
        }
    }

    private void ResetUI(int arg1, int arg2, GameModes mode, Objective objective, List<GridData> arg4, int bonusTileIndex)
    {
        m_ElapsedTime = totalScore = wordsAmount = 0;

        bool IsLevels = mode == GameModes.Levels;

        m_TimerFrame.gameObject.SetActive(IsLevels && objective.timeTocomplete > 0);

        m_ObjectiveFrame.gameObject.SetActive(IsLevels);

        m_ObjectiveText.text = objective == null ? string.Empty : objective.GetObjectiveAsString();

        UpdateUI();
    }

    private void OnUpdateScore(string word, Vector2Int[] indices, int _score)
    {
        totalScore += _score;
        wordsAmount++;

        UpdateUI();
    }

    private void UpdateUI()
    {
        m_TotalScoreText.text = string.Format("Total Score : {0}", totalScore);
        m_ElapsedTimeText.text = m_ElapsedTime.ToString("0");
        m_ScorePerWordText.text = string.Format("AVG Per Word : {0}", wordsAmount > 0 ? (totalScore / wordsAmount) : 0);
        
    }

    private void OnDestroy()
    {
        GameManager.OnWordMade -= OnUpdateScore;
        GameManager.LoadGame -= ResetUI;
        GameManager.OnObjectiveResolved -= ObjectiveResolved;
        GameManager.OnTimerChanged -= TimerChanged;
    }
}
