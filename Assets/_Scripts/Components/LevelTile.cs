using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class LevelTile : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler,IPointerExitHandler,IPointerDownHandler
{
    [SerializeField]
    private TextMeshPro m_Text;

    [SerializeField]
    private SpriteRenderer m_Renderer;

    [SerializeField]
    private GameObject bugBonus;

    public Sprite[] blocked = new Sprite[5];

    public Sprite normalSprite;

    public Vector2Int m_Index = Vector2Int.zero;

    private char m_Letter;

    public int tileType = 0;

    public float hValue, gValue, fValue = 0;

    public bool IsBugTile = false;

    public LevelTile parentTile;

    public void SetUp(char c, Vector2Int index, int _tileType = 0, bool bugTile = false)
    {
        if (m_Letter != c) // Tween only is letter Changes
        {
            m_Text.rectTransform.anchoredPosition = new Vector2(0, 1);

            m_Text.rectTransform.DOAnchorPosY(0, 0.2f);
        }

        m_Letter = c;

        m_Text.text = string.Format("{0}", m_Letter);

        m_Index = index;

        tileType = _tileType;

        IsBugTile = bugTile;

        UpdateSprite(tileType-1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Add this to the word.
        if (tileType != 0)
        {
            return;
        }
        GameManager.Instance.SelectTile(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.Instance.CheckForWord();
    }

    public void SelectTile()
    {
        m_Renderer.color = Color.green;
    }

    public void DeselectTile()
    {
        m_Renderer.color = Color.white;
    }

    public char GetCharacter()
    {
        return m_Letter;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
     
    }

    public void OnPointerDown(PointerEventData eventData)
    {
     
    }

    public bool IsTileBlocked()
    {
        return tileType > 0;
    }

    public void UpdateTileType()
    {
        tileType--;

        tileType = Mathf.Clamp(tileType, 0, 6);

        int index = tileType - 1;

        UpdateSprite(index);
    }

    private void UpdateSprite(int index)
    {
        if (index >= 0)
        {
            m_Renderer.sprite = blocked[index];
        }
        else
        {
            m_Renderer.sprite = normalSprite;
        }

        bugBonus.gameObject.SetActive(IsBugTile);
    }

    public void SetCharacter(char c)
    {
        SetUp(c, m_Index);
    }

    public bool IsNeighbour(LevelTile guestTile)
    {
        if(guestTile.m_Index == m_Index)
        {
            return false;
        }

        if (Mathf.Abs(m_Index.x - guestTile.m_Index.x) <= 1 && Mathf.Abs(m_Index.y - guestTile.m_Index.y) <= 1)
        {
            return true;
        }
       
        return false;
    }
}
