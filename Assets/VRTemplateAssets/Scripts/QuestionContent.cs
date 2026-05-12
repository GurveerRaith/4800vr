using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionContent : MonoBehaviour, IWhiteboardContent {
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceLabels;

    private QuestionData _currentQuestion;

    public void LoadQuestion(QuestionData data) {
        _currentQuestion = data;
        questionText.text = data.questionText;

        for (int i = 0; i < choiceButtons.Length; i++) {
            var label = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.text = data.choices[i];

            int index = i;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
        }
    }

    private void OnChoiceSelected(int index) {
        bool correct = index == _currentQuestion.correctAnswerIndex;
        Debug.Log(correct ? "Correct" : "Wrong");
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}