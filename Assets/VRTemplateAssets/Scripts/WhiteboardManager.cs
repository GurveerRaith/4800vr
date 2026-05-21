using UnityEngine;

public class WhiteboardManager : MonoBehaviour {
    private IWhiteboardContent _current;

    [Header("Content")]
    public QuestionContent questionContent;
    public QuestionData[] questions;
    private int _currentIndex = 0;

    void Start() {
        ShowCurrentQuestion();
    }

    public void NextQuestion() {
        if (_currentIndex < questions.Length - 1) {
            _currentIndex++;
            ShowCurrentQuestion();
        }
    }

    private void ShowCurrentQuestion() {
        Display(questionContent);
        questionContent.LoadQuestion(questions[_currentIndex]);
    }

    public void Display(IWhiteboardContent content) {
        _current?.Hide();
        _current = content;
        _current.Show();
    }
}