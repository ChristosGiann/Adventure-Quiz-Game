using UnityEngine;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")]
public class QuizQuestion : ScriptableObject
{
    [TextArea] public string question;
    public string[] answers;
    public int correctAnswerIndex;
}
