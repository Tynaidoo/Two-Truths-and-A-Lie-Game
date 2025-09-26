using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CategorySelectionManager : MonoBehaviour
{
    public Button sportsButton;
    public Button historyButton;
    public Button scienceButton;
    public Button popCultureButton;
    public Button geographyButton;

    void Start()
    {
        sportsButton.onClick.AddListener(() => LoadGameScene(21));     // Sports
        historyButton.onClick.AddListener(() => LoadGameScene(23));    // History
        scienceButton.onClick.AddListener(() => LoadGameScene(17));    // Science
        popCultureButton.onClick.AddListener(() => LoadGameScene(12)); // Music (Pop Culture)
        geographyButton.onClick.AddListener(() => LoadGameScene(22));  // Geography
    }

    void LoadGameScene(int categoryId)
    {
        PlayerPrefs.SetInt("SelectedCategory", categoryId);
        SceneManager.LoadScene("GameScene");
    }
}