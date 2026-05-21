using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class QuestionContent : MonoBehaviour, IWhiteboardContent {
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button[] choiceButtons;

    [Header("Feedback")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 1.5f;

    public Action<int> onChoiceSelected;
    public Action onFeedbackComplete;

    private QuestionData _currentQuestion;

    public void LoadQuestion(QuestionData data) {
        _currentQuestion = data;
        questionText.text = data.questionText;
        feedbackPanel.SetActive(false);

        foreach (var btn in choiceButtons)
            btn.interactable = true;

        for (int i = 0; i < choiceButtons.Length; i++) {
            var label = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.text = data.choices[i];

            int index = i;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
        }
    }

    private void OnChoiceSelected(int index) {
        foreach (var btn in choiceButtons)
            btn.interactable = false;

        onChoiceSelected?.Invoke(index);
    }

    public void ShowFeedback(bool correct) {
        feedbackPanel.SetActive(true);
        feedbackText.text = correct ? "✓" : "✗";
        feedbackText.color = correct ? Color.green : Color.red;
        StartCoroutine(FeedbackRoutine());
    }

    private IEnumerator FeedbackRoutine() {
        yield return new WaitForSeconds(feedbackDuration);
        feedbackPanel.SetActive(false);
        onFeedbackComplete?.Invoke();
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}