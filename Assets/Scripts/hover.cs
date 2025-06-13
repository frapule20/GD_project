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
    private bool wasInteractable;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            wasInteractable = button.interactable;
            UpdateColor();
        }
    }

    void Update()
    {
        // Controlla solo se lo stato interactable è cambiato
        if (button != null && button.interactable != wasInteractable)
        {
            wasInteractable = button.interactable;
            UpdateColor();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            buttonText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            buttonText.color = normalColor;
        }
    }

    private void UpdateColor()
    {
        if (buttonText == null) return;

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