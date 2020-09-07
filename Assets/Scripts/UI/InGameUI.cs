using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUI : MonoBehaviour
{
    internal static bool IsGamePaused = false;
    internal static bool IsFormSelectOpened = false;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject formSelectUI;

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (IsGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (IsFormSelectOpened)
            {
                CloseInv();
            }
            else
            {
                OpenInv();
            }
        }
    }

    private void OpenInv()
    {
        formSelectUI.SetActive(true);
        IsFormSelectOpened = true;
    }

    private void CloseInv()
    {
        formSelectUI.SetActive(false);
        IsFormSelectOpened = false;
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsGamePaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}
