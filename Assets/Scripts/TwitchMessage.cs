using UnityEngine;
using UnityEngine.UI;

public class TwitchMessage : MonoBehaviour
{
    private Image _messageBackground = null;
    private Text _messageText = null;

    private void Awake()
    {
        _messageBackground = GetComponent<Image>();
        _messageText = GetComponentInChildren<Text>();
    }

	public void SetMessage(string text)
    {
        _messageText.text = text;
    }
}
