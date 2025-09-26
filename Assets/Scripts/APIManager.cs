using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class APIManager : MonoBehaviour
{
    public TMP_Text questionText;
    public TMP_Text feedbackText;
    public TMP_Text timerText;
    public TMP_Text livesText;
    public TMP_Text scoreText;
    public GameObject gameOverPanel;
    public Button[] optionButtons;

    private string correctAnswer;
    private string[] allAnswers;
    private int correctIndex; // Index of the correct answer in the options
    private int currentCategory;
    private int lives = 3;
    private int score = 0;
    private float timeRemaining = 30f;
    private bool timerIsRunning = false;
    private Coroutine currentQuestionCoroutine;

    void Start()
    {
        currentCategory = PlayerPrefs.GetInt("SelectedCategory", 21);
        feedbackText.text = "";
        timerText.text = "";
        livesText.text = $"Lives: {lives}";
        scoreText.text = $"Score: {score}";
        gameOverPanel.SetActive(false);
        StartNewQuestion();
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                TimeOut();
            }
        }
    }

    void StartNewQuestion()
    {
        if (currentQuestionCoroutine != null)
        {
            StopCoroutine(currentQuestionCoroutine);
        }
        currentQuestionCoroutine = StartCoroutine(GetTriviaQuestion());
    }

    IEnumerator GetTriviaQuestion()
    {
        feedbackText.text = "Loading question...";
        timerText.text = "";

        string url = $"https://opentdb.com/api.php?amount=1&category={currentCategory}&type=multiple";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                feedbackText.text = "";
                OpenTriviaResponse response = JsonUtility.FromJson<OpenTriviaResponse>(request.downloadHandler.text);

                if (response.results.Length > 0)
                {
                    TriviaQuestion question = response.results[0];
                    questionText.text = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(question.question);
                    correctAnswer = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(question.correct_answer);

                    // Get all incorrect answers
                    List<string> answers = new List<string>();
                    foreach (string incorrectAnswer in question.incorrect_answers)
                    {
                        answers.Add(UnityEngine.Networking.UnityWebRequest.UnEscapeURL(incorrectAnswer));
                    }

                    // MODIFICATION: Only use 2 incorrect answers + 1 correct answer = 3 total
                    // Shuffle the incorrect answers and take only 2
                    Shuffle(answers);
                    List<string> selectedAnswers = answers.GetRange(0, 2); // Take only 2 incorrect answers
                    selectedAnswers.Add(correctAnswer);

                    // Shuffle the final 3 answers
                    Shuffle(selectedAnswers);

                    // Find the index of the correct answer after shuffling
                    for (int i = 0; i < selectedAnswers.Count; i++)
                    {
                        if (selectedAnswers[i] == correctAnswer)
                        {
                            correctIndex = i;
                            break;
                        }
                    }

                    allAnswers = selectedAnswers.ToArray();

                    // Assign to buttons (only first 3 buttons)
                    for (int i = 0; i < optionButtons.Length; i++)
                    {
                        if (i < allAnswers.Length) // Only assign to first 3 buttons
                        {
                            optionButtons[i].GetComponentInChildren<TMP_Text>().text = allAnswers[i];
                            int index = i;
                            optionButtons[i].onClick.RemoveAllListeners();
                            optionButtons[i].onClick.AddListener(() => CheckAnswer(index));
                            optionButtons[i].interactable = true;
                            optionButtons[i].gameObject.SetActive(true); // Ensure button is active
                        }
                        else
                        {
                            // Hide any extra buttons beyond the 3 we need
                            optionButtons[i].gameObject.SetActive(false);
                        }
                    }

                    // Start timer
                    timeRemaining = 30f;
                    timerIsRunning = true;
                    UpdateTimerDisplay();
                }
            }
            else
            {
                feedbackText.text = "Error loading question. Retrying...";
                yield return new WaitForSeconds(2f);
                StartNewQuestion();
            }
        }
    }

    void UpdateTimerDisplay()
    {
        timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}s";

        // Change color when time is running low
        if (timeRemaining < 10f)
        {
            timerText.color = Color.red;
        }
        else if (timeRemaining < 20f)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.green;
        }
    }

    void TimeOut()
    {
        timerIsRunning = false;
        feedbackText.text = "Oops! You took too long!";
        feedbackText.color = Color.yellow;

        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }

        LoseLife();
        Invoke(nameof(LoadNextQuestion), 2f);
    }

    void CheckAnswer(int selectedIndex)
    {
        timerIsRunning = false;

        // Disable all buttons
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }

        // Check if the chosen answer is the correct one
        if (selectedIndex == correctIndex)
        {
            feedbackText.text = "Correct! +1 point";
            feedbackText.color = Color.green;
            score++;
            scoreText.text = $"Score: {score}";
        }
        else
        {
            feedbackText.text = $"Wrong! The correct answer was: {correctAnswer}";
            feedbackText.color = Color.red;
            LoseLife();
        }

        Invoke(nameof(LoadNextQuestion), 2f);
    }

    void LoseLife()
    {
        lives--;
        livesText.text = $"Lives: {lives}";

        if (lives <= 0)
        {
            GameOver();
        }
    }

    void LoadNextQuestion()
    {
        if (lives > 0)
        {
            feedbackText.text = "Loading next question...";
            feedbackText.color = Color.white;
            StartNewQuestion();
        }
    }

    void GameOver()
    {
        timerIsRunning = false;
        gameOverPanel.SetActive(true);

        // Disable all buttons
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }

        // Display final score
        TMP_Text gameOverText = gameOverPanel.GetComponentInChildren<TMP_Text>();
        if (gameOverText != null)
        {
            gameOverText.text = $"Game Over!\nFinal Score: {score}";
        }
    }

    void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            string temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Button methods for Game Over panel
    public void RestartGame()
    {
        lives = 3;
        score = 0;
        gameOverPanel.SetActive(false);
        livesText.text = $"Lives: {lives}";
        scoreText.text = $"Score: {score}";
        StartNewQuestion();
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("CategorySelectionScene");
    }

    public void ReturnToCategorySelection()
    {
        SceneManager.LoadScene("CategorySelectionScene");
    }
}

[System.Serializable]
public class OpenTriviaResponse
{
    public TriviaQuestion[] results;
}

[System.Serializable]
public class TriviaQuestion
{
    public string category;
    public string type;
    public string difficulty;
    public string question;
    public string correct_answer;
    public string[] incorrect_answers;
}