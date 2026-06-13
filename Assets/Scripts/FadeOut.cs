using UnityEngine;
using TMPro;
using System.Collections;

public class FadeOut : MonoBehaviour
{
    public CanvasGroup panelCanvasGroup;
    public TextMeshProUGUI textComponent;

    private void Start()
    {
        StartCoroutine(FadeOutPanelAndText());
    }

    private IEnumerator FadeOutPanelAndText()
    {
        panelCanvasGroup.alpha = 1f;
        Color textColor = textComponent.color;
        textComponent.color = new Color(textColor.r, textColor.g, textColor.b, 1f);

        float fadeDuration = 3f;
        float startTime = Time.time;

        while (Time.time < startTime + fadeDuration)
        {
            float t = (Time.time - startTime) / fadeDuration;
            panelCanvasGroup.alpha = 1f - t;
            textComponent.color = new Color(textColor.r, textColor.g, textColor.b, 1f - t);
            yield return null;
        }

        panelCanvasGroup.alpha = 0f;
        textComponent.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
    }
}
