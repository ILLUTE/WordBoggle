using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class Objective
{
    public float timeTocomplete;
    public int words;
    public int score;

    public Objectives levelObjective = Objectives.None;

    public Objective(float time, int _words, int _score)
    {
        timeTocomplete = time;
        words = _words;
        score = _score;

        if (timeTocomplete == 0)
        {
            levelObjective = words == 0 ? Objectives.OnlyScore : Objectives.OnlyWords;
        }
        else
        {
            levelObjective = words != 0 ? Objectives.WordsInTime : Objectives.ScoreInTime;
        }
    }

    public string GetObjectiveAsString()
    {
        switch(levelObjective)
        {
            case Objectives.OnlyScore:
                return string.Format("Get a Total Score of {0}", score);
            case Objectives.OnlyWords:
                return string.Format("Make {0} Words", words);
            case Objectives.ScoreInTime:
                return string.Format("Get a Total Score of {0} in {1} Seconds", score, timeTocomplete);
            case Objectives.WordsInTime:
                return string.Format("Make {0} words in {1} Seconds", words, timeTocomplete);
        }
        return string.Empty;
    }
    public Conditions IsObjectiveComplete(float time, int _words, int _score)
    {
        Conditions m = new Conditions();

        switch (levelObjective)
        {
            case Objectives.OnlyScore:
                m.m_ConditionMet = score <= _score;
                break;
            case Objectives.OnlyWords:
                m.m_ConditionMet = words <= _words;
                break;
            case Objectives.WordsInTime:
                m.m_ConditionMet = words <= _words;
                m.m_TimeElapsed = time >= timeTocomplete;
                break;
            case Objectives.ScoreInTime:
                m.m_ConditionMet = score <= _score;
                m.m_TimeElapsed = time >= timeTocomplete;
                break;

        }

        return m;
    }
}

public struct Conditions
{
    public bool m_TimeElapsed;
    public bool m_ConditionMet;
}
