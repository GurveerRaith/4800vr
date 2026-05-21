using UnityEngine;
using TMPro;

public class QuizManager : MonoBehaviour {
    [Header("Content")]
    public QuestionContent questionContent;
    public QuestionData[] questions;

    [Header("Score Screen")]
    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;

    private int _currentIndex = 0;
    private int _score = 0;

    void Start() {
        questionContent.onChoiceSelected += OnChoiceSelected;
        questionContent.onFeedbackComplete += OnFeedbackComplete;
        scorePanel.SetActive(false);
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion() {
        questionContent.LoadQuestion(questions[_currentIndex]);
        questionContent.Show();
    }

    private void OnChoiceSelected(int index) {
        bool correct = index == questions[_currentIndex].correctAnswerIndex;
        if (correct) _score++;
        questionContent.ShowFeedback(correct);
    }

    private void OnFeedbackComplete() {
        _currentIndex++;
        if (_currentIndex < questions.Length)
            ShowCurrentQuestion();
        else
            ShowScore();
    }

    private void ShowScore() {
        questionContent.Hide();
        scorePanel.SetActive(true);
        scoreText.text = $"Score: {_score} / {questions.Length}";
    }

    void OnDestroy() {
        questionContent.onChoiceSelected -= OnChoiceSelected;
        questionContent.onFeedbackComplete -= OnFeedbackComplete;
    }
}