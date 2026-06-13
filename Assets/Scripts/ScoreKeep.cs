using UnityEngine;
using TMPro;
using System.Collections;

public class VictoryScreen : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        StartCoroutine(ShowScoreAfterDelay());
    }

    IEnumerator ShowScoreAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        if (PersistentScore.Instance != null)
        {
            int score = PersistentScore.Instance.finalScore;
            scoreText.text = $"Score: {score}/6";
            Debug.Log($"Displaying final score: {score}");

            PersistentScore.Instance.Clear();
        }
        else
        {
            scoreText.text = "Score: 0/6";
            Debug.Log("No PersistentScore found.");
        }
    }
}
