using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuObject;
    public GameObject settingsMenuObject;
    public GameObject playMenuObject;

    public TextMeshProUGUI musicTextButton;
    public TextMeshProUGUI soundsTextButton;

    public TextMeshProUGUI levelTextTitle;
    public Image levelImage;

    SfxManager sfxManager;
    int levelNumero;
    public Sprite[] maps;

    void Awake(){
        GameObject sfxManagerObject = GameObject.Find("SfxManager");
        if (sfxManagerObject != null) sfxManager = sfxManagerObject.GetComponent<SfxManager>();
        if (sfxManager != null) sfxManager.RefreshVolume();
        levelNumero = 1;
    }

    public void UnactiveAll(GameObject objMenu){
        mainMenuObject.SetActive(false);
        settingsMenuObject.SetActive(false);
        playMenuObject.SetActive(false);

        objMenu.SetActive(true);

        if (sfxManager != null) sfxManager.AddSoundsSource("notif");
    }

    public void ActiveMainMenu(){
        UnactiveAll(mainMenuObject);
    }

    public void ActivePlayMenu(){
        UnactiveAll(playMenuObject);

        levelTextTitle.text = "LEVEL " + levelNumero + "/" + maps.Length + "\nSCORE " + PlayerPrefs.GetInt("level_" + levelNumero, 0);
        levelImage.sprite = maps[levelNumero-1];
    }
    public void LoadLevel(){
        SceneManager.LoadScene("level_" + levelNumero);
    }
    public void NextLevel(){
        levelNumero++;
        if (levelNumero > maps.Length) levelNumero = 1;
        ActivePlayMenu();
    }
    public void PastLevel(){
        levelNumero--;
        if (levelNumero < 1) levelNumero = maps.Length;
        ActivePlayMenu();
    }

    public void ActiveSettingsMenu(){
        UnactiveAll(settingsMenuObject);
        RefreshSettings();
    }
    public void RefreshSettings(){
        musicTextButton.text = "MUSIC\n" + PlayerPrefs.GetInt("Music", 50);
        soundsTextButton.text = "SOUNDS\n" + PlayerPrefs.GetInt("Sounds", 50);
        Awake();
    }
    public void AddMusic(){
        ModifyVolume("Music", 5);
    }
    public void LessMusic(){
        ModifyVolume("Music", -5);
    }
    public void AddSounds(){
        ModifyVolume("Sounds", 5);
    }
    public void LessSounds(){
        ModifyVolume("Sounds", -5);
    }
    void ModifyVolume(string type, int amount){
        PlayerPrefs.SetInt(type, PlayerPrefs.GetInt(type, 50) + amount);
        if (PlayerPrefs.GetInt(type, 50) > 100) PlayerPrefs.SetInt(type, 100);
        if (PlayerPrefs.GetInt(type, 50) < 0) PlayerPrefs.SetInt(type, 0);
        PlayerPrefs.Save();

        RefreshSettings();
    }

    public void Quit(){
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
