using UnityEngine;
using TMPro;

public class TaskUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI taskText;

    void OnEnable()
    {
        GameStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
    }

    void OnDisable()
    {
        GameStateManager.Instance.OnStateChanged.RemoveListener(OnStateChanged);
    }

    void OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.GiveMicrophone:
                taskText.text = "Objective:\n Give the professor your FM microphone";
                break;
            case GameState.SitDown:
                taskText.text = "Objective:\n Sit down in your seat";
                break;
            case GameState.WatchingVideo:
                taskText.text = "Objective:\n Watch the lecture";
                break;
            case GameState.TakeQuiz:
                taskText.text = "Objective:\n Answer the quiz on your phone";
                break;
            default:
                taskText.text = "";
                break;
        }
    }
}