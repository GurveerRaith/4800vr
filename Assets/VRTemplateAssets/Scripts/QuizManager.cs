using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class QuizManager : MonoBehaviour {
    [Header("Content")]
    public QuestionContent questionContent;
    public QuestionData[] questions;

    [Header("Score Screen")]
    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;

    [Header("Video Setup")]
    public VideoPlayer introVideo;
    public GameObject videoDisplayObject;

    private int _currentIndex = 0;
    private int _score = 0;

    void Start() {
        questionContent.onChoiceSelected += OnChoiceSelected;
        questionContent.onFeedbackComplete += OnFeedbackComplete;
        scorePanel.SetActive(false);
        
        questionContent.Hide(); 

        if (videoDisplayObject != null) videoDisplayObject.SetActive(true);
        introVideo.loopPointReached += OnVideoFinished;
        if (!introVideo.isPlaying) introVideo.Play();

    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        introVideo.loopPointReached -= OnVideoFinished;

        if (videoDisplayObject != null) videoDisplayObject.SetActive(false);
        
        StartQuiz();
    }

    private void StartQuiz()
    {
        questionContent.Show();
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
        questionContent.HideElements(); 
        
        scorePanel.SetActive(true);
        scoreText.text = $"Score: {_score} / {questions.Length}";
    }

    void OnDestroy() {
        questionContent.onChoiceSelected -= OnChoiceSelected;
        questionContent.onFeedbackComplete -= OnFeedbackComplete;
    }
}