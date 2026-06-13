using UnityEngine;

[CreateAssetMenu(fileName = "New Quiz", menuName = "Quiz/Quiz Data")]
public class QuizData : ScriptableObject
{
    public QuizQuestion[] questions;
}
