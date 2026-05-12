using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestion", menuName = "Whiteboard/Question")]
public class QuestionData : ScriptableObject {
    public string questionText;
    public string[] choices;
    public int correctAnswerIndex;
}