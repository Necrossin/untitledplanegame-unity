using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class OptionsMenuLogic : MonoBehaviour
{
    private GameBehaviour game;

    [SerializeField]
    private GameObject mainMenu;
    //[SerializeField]
    //private AudioMixer mainMixer;
    [SerializeField]
    private GameObject volumeText;
    [SerializeField]
    private GameObject volumeSlider;

    void Start()
    {
        game = GameBehaviour.Instance;
        float vol = PlayerPrefs.GetFloat("MasterVolume", 0);
        volumeSlider.GetComponent<Slider>().value = vol;
        setVolume(vol);
    }

    public void ReturnToMenu()
    {
        gameObject.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void setVolume( float vol )
    {
        game.mainMixer.SetFloat("masterVolume", vol);
        int volume = Mathf.RoundToInt( volumeSlider.GetComponent<Slider>().normalizedValue * 100 );
        setVolumeNumber( volume );
        PlayerPrefs.SetFloat("MasterVolume", vol);
    }

    private void setVolumeNumber( int num )
    {
        volumeText.GetComponent<TextMeshProUGUI>().text = "VOLUME: " + num.ToString();
    }
}
