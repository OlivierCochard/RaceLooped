using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public AudioSource soundsSource;
    public AudioSource musicSource;

    public AudioClip colliderProjectile;
    public AudioClip notif;
    public AudioClip lose;
    public AudioClip finishBot;
    public AudioClip win;

    void Awake(){
        RefreshVolume();
    }
    public void RefreshVolume(){
        musicSource.volume = (float)PlayerPrefs.GetInt("Music", 50)/100f;
        soundsSource.volume = (float)PlayerPrefs.GetInt("Sounds", 50)/100f;
    }
    public void RefreshVolumeCar(RaceManager raceManager){
        if (raceManager == null || raceManager.GetBotObjects() == null) return;

        float volume = (float)PlayerPrefs.GetInt("Sounds", 50)/100f;
        foreach (GameObject go in raceManager.GetBotObjects()){
            go.GetComponent<AudioSource>().volume = volume;
        }
        if (raceManager.GetPlayerObject() != null) raceManager.GetPlayerObject().GetComponent<AudioSource>().volume = volume;
    }

    public void PlayMusicSource(){
        musicSource.Play();
    }
    public void StopMusicSource(){
        musicSource.Stop();
    }

    public void AddSoundsSource(string nom){
        AudioClip clipToPlay = null;

        switch (nom.ToLower()){
            case "colliderprojectile":
                clipToPlay = colliderProjectile;
                break;
            case "notif":
                clipToPlay = notif;
                break;
            case "lose":
                clipToPlay = lose;
                break;
            case "finishbot":
                clipToPlay = finishBot;
                break;
            case "win":
                clipToPlay = win;
                break;

            default:
                Debug.LogWarning("SFX introuvable : " + nom);
                return;
        }

        if (clipToPlay != null){
            soundsSource.PlayOneShot(clipToPlay);
        }
    }
}
