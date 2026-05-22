using UnityEngine;
using TMPro;

/// Drives the quiz on the tablet UI in front of the seated player.
/// Listens for the TakeQuiz game state, then walks through `questions[]` one at a
/// time on the wired `questionContent`. Shows a final score panel when done.
public class QuizManager : MonoBehaviour
{
    [Header("Tablet Quiz")]
    [Tooltip("The QuestionContent on the tablet UI (the panel in front of the seated player). " +
             "Should NOT be the question_panel on the big screen.")]
    public QuestionContent questionContent;

    [Tooltip("ScriptableObjects defining each question. Order = display order.")]
    public QuestionData[] questions;

    [Header("Score Screen")]
    [Tooltip("Optional. Panel that displays the final score. Should live on the tablet.")]
    public GameObject scorePanel;

    [Tooltip("Optional. TMP text used to display the final score.")]
    public TextMeshProUGUI scoreText;

    [Header("End Screen")]
    [Tooltip("How long to show the score before transitioning to EndScreen state.")]
    public float endScreenDelaySeconds = 3f;

    private int _currentIndex = 0;
    private int _score = 0;
    private bool _started = false;
    private bool _subscribed = false;

    void Start()
    {
        if (questionContent != null)
        {
            questionContent.onChoiceSelected += OnChoiceSelected;
            questionContent.onFeedbackComplete += OnFeedbackComplete;
            questionContent.Hide();
        }
        else
        {
            Debug.LogError($"[{nameof(QuizManager)}] questionContent is not wired.", this);
        }

        if (scorePanel != null) scorePanel.SetActive(false);

        TrySubscribe();
    }

    void OnEnable() => TrySubscribe();

    void OnDisable()
    {
        if (_subscribed && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged.RemoveListener(OnStateChanged);
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;
        if (GameStateManager.Instance == null) return;
        GameStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
        _subscribed = true;
        Debug.Log($"[{nameof(QuizManager)}] Subscribed to OnStateChanged.", this);
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.TakeQuiz && !_started)
        {
            _started = true;
            StartQuiz();
        }
    }

    private void StartQuiz()
    {
        if (questionContent == null || questions == null || questions.Length == 0)
        {
            Debug.LogError($"[{nameof(QuizManager)}] Missing questionContent or questions — cannot start.", this);
            return;
        }

        _currentIndex = 0;
        _score = 0;
        if (scorePanel != null) scorePanel.SetActive(false);
        questionContent.Show();
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        questionContent.LoadQuestion(questions[_currentIndex]);
        questionContent.Show();
    }

    private void OnChoiceSelected(int index)
    {
        bool correct = index == questions[_currentIndex].correctAnswerIndex;
        if (correct) _score++;
        questionContent.ShowFeedback(correct);
    }

    private void OnFeedbackComplete()
    {
        _currentIndex++;
        if (_currentIndex < questions.Length)
            ShowCurrentQuestion();
        else
            ShowScore();
    }

    private void ShowScore()
    {
        questionContent.HideElements();
        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
            if (scoreText != null) scoreText.text = $"Score: {_score} / {questions.Length}";
        }
        Debug.Log($"[{nameof(QuizManager)}] Final score: {_score} / {questions.Length}.", this);
        StartCoroutine(TransitionToEndScreen());
    }

    private System.Collections.IEnumerator TransitionToEndScreen()
    {
        yield return new WaitForSeconds(endScreenDelaySeconds);
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.EndScreen);
    }

    void OnDestroy()
    {
        if (questionContent != null)
        {
            questionContent.onChoiceSelected -= OnChoiceSelected;
            questionContent.onFeedbackComplete -= OnFeedbackComplete;
        }
    }
}
