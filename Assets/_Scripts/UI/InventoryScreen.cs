using TMPro;
using UnityEngine;
using DG.Tweening;

public class InventoryScreen : MonoBehaviour
{
    public InventoryItem item;
    public TextMeshProUGUI m_Score;

    public RectTransform m_Rect;
    private Sequence m_Sequence;

    private void Awake()
    {
        GameManager.OnInventoryItemValueChanged += OnInventoryValueChanged;
    }

    private void OnInventoryValueChanged(InventoryItem item)
    {
        if(m_Sequence != null)
        {
            m_Sequence.Kill();
        }

        m_Sequence = DOTween.Sequence();

        m_Sequence.Append(m_Rect.DOAnchorPosY(0, 0.3f)).AppendInterval(1.2f).Append(m_Rect.DOAnchorPosY(-400, 0.3f));

        UpdateUI();
    }

    private void UpdateUI()
    {
        m_Score.text = item.Amount.ToString();
    }
}
