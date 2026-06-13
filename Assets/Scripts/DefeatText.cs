using TMPro;
using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI textElement;

    void Start()
    {
        StartCoroutine(FadeInText(textElement, 1f));
    }

    IEnumerator FadeInText(TextMeshProUGUI text, float duration)
    {
        Color startColor = text.color;
        startColor.a = 0f; 

        text.color = startColor; 

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);

            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha); 
            yield return null; 
        }

        text.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
    }
}
