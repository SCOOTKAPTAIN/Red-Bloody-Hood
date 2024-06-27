using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    int progressAmount;
    public Slider ProgressSlider;
    public GameObject player;
    public GameObject LoadCanvas;
    public List<GameObject> levels;
    private int currentLevelIndex = 0;
    public GameObject gameOverScreen;
    public TMP_Text SurvivedText;

    public static event Action OnReset;

    void Start()
    {
        progressAmount = 0;
        ProgressSlider.value = 0;
        Flower.OnFlowerCollect += IncreaseProgressAmount;
        Hold2Load.OnHoldComplete += LoadNextLevel;
        PlayerHealth.OnPlayerDied += GameOverScreen;
        LoadCanvas.SetActive(false);
        gameOverScreen.SetActive(false);

    }

    void GameOverScreen()
    {
        gameOverScreen.SetActive(true);
        MusicManager.PauseBackgroundMusic();
        Time.timeScale = 0;

    }

    public void ResetGame()
    {
        gameOverScreen.SetActive(false);
        MusicManager.PlayBackgroundMusic(true);
        LoadLevel(0);
        OnReset.Invoke();
        Time.timeScale=1;
    }

    void IncreaseProgressAmount(int Amount)
    {
        progressAmount += Amount;
        ProgressSlider.value = progressAmount;

        if(progressAmount >= 100)
        {
            LoadCanvas.SetActive(true);
            Debug.Log("Level Done");
        }

    }

    void LoadLevel(int level)
    {
         LoadCanvas.SetActive(false);

        levels[currentLevelIndex].gameObject.SetActive(false);
        levels[level].gameObject.SetActive(true);

        player.transform.position = new Vector3(0,0,0);

        currentLevelIndex = level;
        progressAmount = 0;
        ProgressSlider.value = 0;

    }

    void LoadNextLevel()
    {
        int nextLevelIndex = (currentLevelIndex == levels.Count - 1) ? 0 : currentLevelIndex + 1;
        LoadLevel(nextLevelIndex);
    }
}
