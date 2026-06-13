using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets; 
using Cinemachine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    public QuizData QuizData;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public GameObject quizPanel;
    public GameObject player;
    public ThirdPersonController playerController;
    public StarterAssetsInputs playerInput;
    public CinemachineVirtualCamera virtualCamera;

    private Transform initialCameraFollow;
    private Vector3 savedCamPosition;
    private Quaternion savedCamRotation;
    private Camera mainCam;
    private HashSet<GameObject> usedTriggers = new HashSet<GameObject>();
    private List<QuizQuestion> remainingQuestions;

    private GameObject uiCanvas;
    private bool quizActive = false;
    public TextMeshProUGUI feedbackText;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioSource audioSource;
    private int score = 0;
    public TextMeshProUGUI scoreText; 
    public GameObject helpCanvas; 
    public TextMeshProUGUI hintTextToHide; 
    public Slider mistakeSlider;
    private int mistakes = 0;
    private const int maxMistakes = 3;
    private const int maxQuestions = 6;
    public int requiredCorrectAnswers = 4;


    void Start()
    {
        quizPanel.SetActive(false);
        SetCursorState(true);

        uiCanvas = GameObject.Find("HUD");

        if (virtualCamera != null)
            initialCameraFollow = virtualCamera.Follow;

        mainCam = Camera.main;

        remainingQuestions = new List<QuizQuestion>(QuizData.questions);
        ShuffleList(remainingQuestions);
        if (remainingQuestions.Count > maxQuestions)
            remainingQuestions = remainingQuestions.GetRange(0, maxQuestions);


        if (mistakeSlider != null)
        {
            mistakeSlider.maxValue = maxMistakes;
            mistakeSlider.value = maxMistakes;
        }

        UpdateScoreText();
    }

    void Update()
    {
        if (quizActive || player == null) return;

        if (score >= requiredCorrectAnswers && remainingQuestions.Count == 0)
        {   
            Debug.Log($"Saving final score (from Update): {score}");

            if (PersistentScore.Instance == null)
            {
                GameObject scoreObj = new GameObject("PersistentScore");
                DontDestroyOnLoad(scoreObj);
                var ps = scoreObj.AddComponent<PersistentScore>();
                ps.finalScore = score;
            }
            else
            {
                PersistentScore.Instance.finalScore = score;
            }

            SceneManager.LoadScene("Victory");
            return;
        }


        GameObject[] triggers = GameObject.FindGameObjectsWithTag("QuizTrigger");

        if (triggers.Length > 0)
        {
            GameObject closestTrigger = null;
            float closestDistance = Mathf.Infinity;

            foreach (GameObject trigger in triggers)
            {
                if (usedTriggers.Contains(trigger)) continue;

                float distance = Vector3.Distance(player.transform.position, trigger.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTrigger = trigger;
                }
            }

            if (closestTrigger != null && closestDistance <= 5f)
            {
                usedTriggers.Add(closestTrigger);
                StartQuiz();
            }
        }

    

        if (Input.GetKeyDown(KeyCode.H))
            {
                if (helpCanvas != null)
                    helpCanvas.SetActive(!helpCanvas.activeSelf);

                if (hintTextToHide != null)
                    hintTextToHide.gameObject.SetActive(!helpCanvas.activeSelf);
            }    


        GameObject[] particles = GameObject.FindGameObjectsWithTag("Particles");
        foreach (GameObject particleObj in particles)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, particleObj.transform.position);
            ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
            if (ps != null && distanceToPlayer <= 10f && ps.isPlaying)
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        if (!quizActive)
        {
            if (Input.GetKey(KeyCode.Tab))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (playerInput != null)
                    playerInput.cursorInputForLook = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (playerInput != null)
                    playerInput.cursorInputForLook = true;
            }
        }

    }


    void StartQuiz()
    {
        quizActive = true;
        quizPanel.SetActive(true);

        if (uiCanvas != null)
            uiCanvas.SetActive(false);

        if (playerController != null)
            playerController.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Animator anim = player.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetFloat("Speed", 0);
        }

        if (mainCam != null)
        {
            savedCamPosition = mainCam.transform.position;
            savedCamRotation = mainCam.transform.rotation;
        }

        if (virtualCamera != null)
            virtualCamera.enabled = false;

        mainCam.transform.position = savedCamPosition;
        mainCam.transform.rotation = savedCamRotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
            playerInput.cursorInputForLook = false;

        if (remainingQuestions.Count > 0)
        {
            QuizQuestion selected = remainingQuestions[0];
            remainingQuestions.RemoveAt(0);
            ShowQuestion(selected);
        }
        else
        {
            if (score >= requiredCorrectAnswers)
                {
                    Debug.Log($"Saving final score: {score}");

                    if (PersistentScore.Instance == null)
                    {
                        GameObject scoreObj = new GameObject("PersistentScore");
                        DontDestroyOnLoad(scoreObj);
                        var ps = scoreObj.AddComponent<PersistentScore>();
                        ps.finalScore = score;
                    }
                    else
                    {
                        PersistentScore.Instance.finalScore = score;
                    }

                    SceneManager.LoadScene("Victory");
                }


        }
    }

    void EndQuiz()
    {
        quizPanel.SetActive(false);
        quizActive = false;
        if (uiCanvas != null)
            uiCanvas.SetActive(true);

        if (playerController != null)
            playerController.enabled = true;

        if (virtualCamera != null)
            virtualCamera.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
            playerInput.cursorInputForLook = true;
    }

    void ShowQuestion(QuizQuestion q)
    {
        questionText.text = q.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool active = i < q.answers.Length;
            answerButtons[i].gameObject.SetActive(active);

            if (active)
            {
                answerButtons[i].interactable = true;

                TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = q.answers[i];

                int capturedIndex = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => CheckAnswer(q, capturedIndex));
            }
        }
    }

    void CheckAnswer(QuizQuestion q, int selectedIndex)
    {
        foreach (Button btn in answerButtons)
            btn.interactable = false;

        bool isCorrect = selectedIndex == q.correctAnswerIndex;

        if (!isCorrect)
        {
            mistakes++;

            if (mistakeSlider != null)
            {
                mistakeSlider.value = maxMistakes - mistakes;

                Image fillImage = mistakeSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (mistakes == 0)
                        fillImage.color = Color.green;
                    else if (mistakes == 1)
                        fillImage.color = new Color(1f, 0.65f, 0f); // πορτοκαλί
                    else
                        fillImage.color = Color.red;
                }
            }

            if (mistakes >= maxMistakes)
            {
                SceneManager.LoadScene("Defeat");
                return;
            }
        }

        if (isCorrect)
        {
            score++;
            UpdateScoreText();
        }

        if (feedbackText != null)
        {
            feedbackText.text = isCorrect ? "Σωστή Απάντηση!" : "Λάθος Απάντηση!";
            feedbackText.color = isCorrect ? Color.green : Color.red;
            feedbackText.gameObject.SetActive(true);
            feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, 1f);
        }

        if (audioSource != null)
        {
            AudioClip clipToPlay = isCorrect ? correctSound : wrongSound;
            if (clipToPlay != null)
                audioSource.PlayOneShot(clipToPlay);
        }

        Invoke(nameof(EndQuiz), 1f);
        StartCoroutine(FadeOutFeedback(2f, 1f));
    }

    IEnumerator FadeOutFeedback(float delayAfterQuizClosed = 2f, float fadeDuration = 1f)
    {
        yield return new WaitForSeconds(delayAfterQuizClosed);

        float elapsed = 0f;
        Color startColor = feedbackText.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            feedbackText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
        feedbackText.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}/6";
    }

    void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
