using UnityEngine;

public sealed class GUIManager : MonoBehaviour {

	public GameObject m_StartPanel;
	public GameObject m_RedPanel;
	public GameObject m_BluePanel;

	public void OnStartClicked()
	{
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
}
