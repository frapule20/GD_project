using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonTextEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color disabledColor = Color.gray;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        UpdateColor();
    }

    void Update()
    {
        if (button != null)
        {
            UpdateColor();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
        {
            buttonText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable)
        {
            buttonText.color = normalColor;
        }
    }

    private void UpdateColor()
    {
        if (button.interactable)
        {
            buttonText.color = normalColor;
            buttonText.fontStyle &= ~FontStyles.Strikethrough;
        }
        else
        {
            buttonText.color = disabledColor;
            buttonText.fontStyle |= FontStyles.Strikethrough;
        }
    }
}
