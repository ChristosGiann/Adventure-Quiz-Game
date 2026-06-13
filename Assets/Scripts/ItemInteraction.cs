using UnityEngine;
using TMPro;
using System.Collections;

public class ChestOpener : MonoBehaviour
{
    public Transform player;
    public Animator animator;
    public float openDistance = 3f;

    public TextMeshProUGUI interactionPromptText; 
    public TextMeshProUGUI messageText;         
    [TextArea] public string messageToShow;

    public AudioSource openSound;
    public GameObject particleToEnable;

    private bool isOpened = false;
    private bool isPromptVisible = false;

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (isOpened)
        {
            if (isPromptVisible && interactionPromptText != null)
            {
                interactionPromptText.text = "";
                isPromptVisible = false;
            }
            return;
        }

        if (distance <= openDistance)
        {
            if (!isPromptVisible && interactionPromptText != null)
            {
                interactionPromptText.text = "Πάτησε E";
                SetTextAlpha(interactionPromptText, 1f);
                isPromptVisible = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                animator.SetTrigger("Open");
                isOpened = true;

                if (openSound != null)
                    openSound.Play();

                if (interactionPromptText != null)
                {
                    interactionPromptText.text = "";
                    isPromptVisible = false;
                }

                if (messageText != null)
                {
                    messageText.text = messageToShow;
                    SetTextAlpha(messageText, 1f);
                    StartCoroutine(FadeOutText(messageText, 2f, 1f));
                }

                if (particleToEnable != null)
                    StartCoroutine(EnableParticleAfterDelay(3f));
            }
        }
        else
        {
            if (isPromptVisible && interactionPromptText != null)
            {
                interactionPromptText.text = "";
                isPromptVisible = false;
            }
        }
    }

    void SetTextAlpha(TextMeshProUGUI textMesh, float alpha)
    {
        Color color = textMesh.color;
        color.a = alpha;
        textMesh.color = color;
    }

    IEnumerator FadeOutText(TextMeshProUGUI textMesh, float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        Color originalColor = textMesh.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    IEnumerator EnableParticleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        particleToEnable.SetActive(true);
    }
}
