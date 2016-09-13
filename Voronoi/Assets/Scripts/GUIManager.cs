using UnityEngine;

public sealed class GUIManager : MonoBehaviour
{
    public GraphManager m_GraphManager;
    public GameObject m_StartPanel;
    public GameObject m_RedPanel;
    public GameObject m_BluePanel;
    public GameObject m_ExplanationPanel;
    public UnityEngine.UI.Text m_BlueScoreText;
    public UnityEngine.UI.Text m_RedScoreText;

    public void OnExplanationAcceptedClick()
    {
        m_ExplanationPanel.SetActive(false);
        m_StartPanel.SetActive(true);
    }

    public void OnStartClicked()
    {
        m_GraphManager.AddVoronoiPoint(Input.mousePosition);
        m_StartPanel.SetActive(false);
    }

    public void OnRedTurnStart()
    {
        m_BluePanel.SetActive(false);
        m_RedPanel.SetActive(true);
    }

    public void OnBlueTurnStart()
    {
        m_RedPanel.SetActive(false);
        m_BluePanel.SetActive(true);
    }

    public void SetPlayerAreaOwned(float a_Player1Area, float a_Player2Area)
    {
        float totalArea = a_Player1Area + a_Player2Area;
        int player1Percentage = Mathf.RoundToInt((a_Player1Area / totalArea) * 100);
        int player2Percentage = Mathf.RoundToInt((a_Player2Area / totalArea) * 100);
        m_BlueScoreText.text = player1Percentage.ToString() + "%";
        m_RedScoreText.text = player2Percentage.ToString() + "%";
    }
}
