using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }

            return instance;
        }
    }

    private Tries m_TrieStructure; // Form of AVL Trie, Used for Word Searches Specifically.. Forms all the Words that were in the wordlist.txt

    private LevelInfo m_LevelInfos = new LevelInfo(); // Just the levelData from Json

    private int currentLevelIndex = -1; // CurrentLevelData --> Not sure if needed, can just parse It and remove( will save memory)

    private Objective currentObjective; // Objective Checker is in GameManager.. Can be moved to a different Class which solely check for Objective. Todo if I have time (Also Scriptable Objects but levelData in json.. so..)

    private Conditions m_Condition; // Checks whehter a condition is met, since Running in Update, Better to have a variable here..

    private int foundWords = 0, score = 0; // Keeping Track here

    private float elapsedTime, lastCheckedTime = 0;

    private bool canUpdate;

    private GameState m_CurrentState = GameState.None;

    #region Events
    public static event Action OnGameStarted;
    public static event Action<string, Vector2Int[], int> OnWordMade; // The Indices made + score made
    public static event Action<LevelTile> OnTileSelected;
    public static event Action SearchWord;
    public static event Action<int, int, GameModes, Objective, List<GridData>, int> LoadGame;
    public static event Action<ButtonType> OnButtonPressed;
    public static event Action<float> OnTimerChanged;
    public static event Action<GameState> SwitchState;
    public static event Action<Objective, Conditions> OnObjectiveResolved;
    public static event Action<InventoryItem> OnInventoryItemValueChanged;
    #endregion


    #region Private Methods
    private void Awake()
    {
        LoadGame += GameStarted;
        OnGameStarted += LoadingFinished;
    }

    private void LoadingFinished()
    {
        SwitchGameState(GameState.MainMenu);
    }

    private void ResetValues() // Resetting Values
    {
        elapsedTime = lastCheckedTime = 0;
        foundWords = score = 0;
        canUpdate = false;
        m_Condition = new Conditions();
        currentObjective = null;
    }

    private void GameStarted(int arg1, int arg2, GameModes mode, Objective objective, List<GridData> arg4, int bonusIndex)
    {
        SwitchGameState(GameState.GameRunning);

        if (mode == GameModes.Levels)
        {
            canUpdate = true;
        }
    }

    private IEnumerator Start()
    {
        bool IsTextAssetLoaded = false;

        MyFileSystem.LoadTextAsset((result) =>
        {
            m_TrieStructure = result;

            IsTextAssetLoaded = true;
        });

        yield return new WaitUntil(() => IsTextAssetLoaded);

        bool IsLevelDataParsed = false;

        MyFileSystem.LoadTextLevel((result) =>
        {
            m_LevelInfos = result;
            IsLevelDataParsed = true;
        });

        yield return new WaitUntil(() => IsLevelDataParsed);

        OnGameStarted?.Invoke();
    }

    private void RefineLevelData(LevelData data)
    {
        m_Condition = new Conditions();

        currentObjective = new Objective(data.timeSec, data.wordCount, data.totalScore);
    }

    private void RestartLevel()
    {
        ResetValues();

        if (currentLevelIndex >= 0)
        {
            GenerateTilesFromLevel(currentLevelIndex);
        }
        else
        {
            GenerateTilesForEndless();
        }
    }

    #endregion

    #region Public Methods
    public void GenerateTilesFromLevel(int level)
    {
        ResetValues();

        int x = m_LevelInfos.data[level].gridSize.x;
        int y = m_LevelInfos.data[level].gridSize.y;

        LevelData currentLevelData = m_LevelInfos.data[level];

        RefineLevelData(currentLevelData);

        LoadGame?.Invoke(x, y, GameModes.Levels, currentObjective, currentLevelData.gridData, currentLevelData.bugCount);
    }

    public void SetCurrentLevelIndex(int index)
    {
        currentLevelIndex = index;
    }

    public void GenerateTilesForEndless()
    {
        ResetValues();
        currentLevelIndex = -1;
        LoadGame?.Invoke(4, 4, GameModes.Endless, null, null, -1);
    }

    public bool SearchForWord(string word)
    {
        return m_TrieStructure.FindWord(word.ToLower().ToCharArray());
    }

    public void SelectTile(LevelTile t)
    {
        OnTileSelected?.Invoke(t);
    }

    public void CheckForWord()
    {
        SearchWord?.Invoke();
    }

    public void WordFormedSuccessfully(string word, Vector2Int[] indices, int _score)
    {
        foundWords++;
        score += _score;

        OnWordMade?.Invoke(word, indices, _score);
    }

    public void ButtonPressed(ButtonType button)
    {
        switch (button)
        {
            case ButtonType.Back:
                SwitchGameState(GameState.MainMenu);
                break;
            case ButtonType.Restart:
                RestartLevel();
                break;
            case ButtonType.EndlessGame:
                GenerateTilesForEndless();
                break;
            case ButtonType.LevelsGame:
                GenerateTilesFromLevel(currentLevelIndex);
                break;
            case ButtonType.LevelSelection:
                SwitchGameState(GameState.LevelSelection);
                // Switch State to LevelSelection.
                break;
        }
        OnButtonPressed?.Invoke(button);
    }

    public void SwitchGameState(GameState state)
    {
        m_CurrentState = state;

        SwitchState?.Invoke(m_CurrentState);
    }
    public bool IsPrefix(string characters)
    {
        return m_TrieStructure.IsStartingWith(characters.ToLower().ToCharArray());
    }
    public List<string> GetAListOfStrings(string prefix)
    {
        return m_TrieStructure.StartsWith(prefix.ToLower().ToCharArray());
    }

    public void InventoryItemValueChange(InventoryItemType type, float value)
    {
        InventoryItem item = ResourceManager.Instance.InventoryItem[type];
        item.Amount += value;
        OnInventoryItemValueChanged?.Invoke(item);
    }
    #endregion


    #region UpdateForObjective
    private void Update()
    {
        if (!canUpdate)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= lastCheckedTime + 1) // Gave 1 as 1 second.. so that this is goes through in once per second.
        {
            lastCheckedTime = elapsedTime;

            OnTimerChanged?.Invoke(elapsedTime);

            m_Condition = currentObjective.IsObjectiveComplete(elapsedTime, foundWords, score);

            if (!m_Condition.m_ConditionMet && !m_Condition.m_TimeElapsed)
            {
                return;
            }
            else if (m_Condition.m_ConditionMet)
            {
                Debug.Log("Objective - Met");

                OnObjectiveResolved?.Invoke(currentObjective, m_Condition);

                canUpdate = false;
            }
            else if (m_Condition.m_TimeElapsed && currentObjective.timeTocomplete > 0)
            {
                Debug.Log("Time Elapsed");

                OnObjectiveResolved?.Invoke(currentObjective, m_Condition);

                canUpdate = false;
            }
        }
    }
    #endregion
}
