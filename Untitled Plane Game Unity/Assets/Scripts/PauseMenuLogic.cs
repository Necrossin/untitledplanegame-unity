using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuLogic : MonoBehaviour
{

    private GameBehaviour game;

    [SerializeField]
    private GameObject optionsMenu;

    void Start()
    {
        game = GameBehaviour.Instance;
    }

    public void ResumeGame()
    {
        game.pauseGame(false);
    }

    public void OpenOptions()
    {
        gameObject.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
