using System.Linq;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    
    [SerializeField] private string musicsFolderPath = "Musics";
    [SerializeField] private AudioClip[] audioClips;
    private AudioSource source;
    private int current_clip = 0;

    void Start()
    {
        source = GetComponent<AudioSource>();
        audioClips = Resources.LoadAll(musicsFolderPath,typeof(AudioClip)).Cast<AudioClip>().ToArray();
        if(!PlayerPrefs.HasKey("MusicVolume"))
        {
            source.volume = 0.5f;
        }
        else
        {
            source.volume = PlayerPrefs.GetFloat("MusicVolume");
        }
    }

    private void Update()
    {
        if(!source.isPlaying)
        {
            Next();
        }
    }

    private void Next()
    {
        current_clip++;
        if(current_clip >= audioClips.Length)
        {
            current_clip = 0;
        }
        source.PlayOneShot(audioClips[current_clip]);
    }

}
