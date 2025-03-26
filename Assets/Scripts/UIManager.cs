using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This class is to be defined in the game manager, and the game manager can use its methods
// to update the UI.

public class UIManager : MonoBehaviour
{

    [Header("Start Screen")]
    public GameObject overlayUI;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameStartText;
    public Button menuButton;

    [Header("Pause Screen")]
    public GameObject pauseScreenUI;
    public Button keepGoing;
    public Button tryAgain;

    [Header("Game Over Screen")]
    public GameObject gameOverScreenUI;
    public TextMeshProUGUI finalScoreText;
    public Button tryAgainGameOver;


    bool stopped_time = false;


    
    private void Start()
    {
        // Add listeners to our buttons
        menuButton.onClick.AddListener(OnMenuButton);
        keepGoing.onClick.AddListener(OnKeepGoing);
        tryAgain.onClick.AddListener(OnTryAgain);
        tryAgainGameOver.onClick.AddListener(OnTryAgain);
    }

    public void GameOverScreen(int score)
    {
        finalScoreText.text = score + "";
        gameOverScreenUI.SetActive(true);
        overlayUI.SetActive(false);

    }

    private void OnMenuButton()
    {

        //Debug.Log("Menu Opened");
        // Pause the game and open the menu.
        // Resume time if time was moving before.
        if (Time.timeScale != 0)
        {
            Time.timeScale = 0;
            stopped_time = true;
        }

        pauseScreenUI.SetActive(true);
        overlayUI.SetActive(false);

    }

    private void OnKeepGoing()
    {
        // Resume the game:
        pauseScreenUI.SetActive(false);
        overlayUI.SetActive(true);
        if (stopped_time)
        {
            Time.timeScale = 1; // Resume time.
        }

    }

    private void OnTryAgain()
    {
        // Reload the game.
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = score + "";
    }

}
