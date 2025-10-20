using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    [SerializeField] private AudioSource source;

    public static SettingsManager Instance;

    private void Awake()
    {
        Instance = this;
        if (globalVolume == null) globalVolume = FindFirstObjectByType<Volume>();
        if (source == null) source = FindFirstObjectByType<AudioSource>();
    }
    public void HandleMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        source.volume = volume;
    }
    public void SetBloom(Scrollbar scrollbar)
    {
        float value = scrollbar.value;
        Bloom bloom = globalVolume.profile.components.OfType<Bloom>().First();
        bloom.intensity.value = value;
        PlayerPrefs.SetFloat("Bloom", value);
    }

    public void SetMotionBlur(Scrollbar scrollbar)
    {
        float value = scrollbar.value;
        MotionBlur motion = globalVolume.profile.components.OfType<MotionBlur>().First();
        motion.intensity.value = value;
        PlayerPrefs.SetFloat("MotionBlur", value);
    }

    public void SetToneMapping(Toggle toggle)
    {
        bool value = toggle.isOn;
        Tonemapping tonemapping = globalVolume.profile.components.OfType<Tonemapping>().First();
        if (value) tonemapping.mode.value = TonemappingMode.Neutral;
        else tonemapping.mode.value = TonemappingMode.None;
        PlayerPrefs.SetString("Tonemapping", value.ToString());
    }

}
