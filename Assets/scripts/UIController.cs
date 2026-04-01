using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    [SerializeField] private Scrollbar strike_force_scroll_bar;
    [SerializeField] private TextMeshProUGUI camera_zoom_precents_text;
    [SerializeField] private Scrollbar music_volume_scroll_bar;
    [SerializeField] private GameObject winning_team_panel;
    [SerializeField] private TextMeshProUGUI winning_team_text;
    [SerializeField] private Color min_color = Color.green;
    [SerializeField] private Color max_color = Color.red;

    [SerializeField] private Scrollbar bloom_intensity;
    [SerializeField] private Scrollbar motionblur_intensity;
    [SerializeField] private Toggle tonemapping_enabled;

    [SerializeField] private TextMeshProUGUI current_player_text;
    [SerializeField] private GameObject current_player_panel;

    private void Awake()
    {
        Instance = this;
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            music_volume_scroll_bar.value = PlayerPrefs.GetFloat("MusicVolume");
        }

        LoadVolumeComponentsValues();

        if (current_player_panel != null)
        {
            current_player_panel.SetActive(false);
        }
    }

    public void ShowCurrentPlayer(string teamName, int abilityPoints, int teamScore)
    {
        if (current_player_text != null && current_player_panel != null)
        {
            current_player_text.text = $"{teamName} score: " + abilityPoints.ToString() + $" AP: {abilityPoints}: ";
            current_player_panel.SetActive(true);
        }
    }

    private void HidePlayerPanel()
    {
        if (current_player_panel != null)
            current_player_panel.SetActive(false);
    }

    private void LoadVolumeComponentsValues()
    {
        if (PlayerPrefs.HasKey("Bloom"))
            bloom_intensity.value = PlayerPrefs.GetFloat("Bloom");
        if (PlayerPrefs.HasKey("MotionBlur"))
            motionblur_intensity.value = PlayerPrefs.GetFloat("MotionBlur");
        if (PlayerPrefs.HasKey("Tonemapping"))
            tonemapping_enabled.isOn = bool.Parse(PlayerPrefs.GetString("Tonemapping"));
    }

    public void UpdateScrollBar(float value)
    {
        strike_force_scroll_bar.value = value;
        UpdateScrollBarColor(value);
    }

    private void UpdateScrollBarColor(float value)
    {
        ColorBlock colorBlock = strike_force_scroll_bar.colors;
        colorBlock.normalColor = Color.Lerp(min_color, max_color, value);
        strike_force_scroll_bar.gameObject.GetComponent<Image>().color = Color.Lerp(min_color, max_color, value);
        strike_force_scroll_bar.colors = colorBlock;
    }

    public void UpdateCameraZoomPrecents(float value)
    {
        camera_zoom_precents_text.text = ((int)(value * 100)).ToString() + "%";
    }

    public void SetWinningTeamText(TeamType team_type)
    {
        winning_team_panel.SetActive(true);
        string team_type_string;
        if (team_type == TeamType.Strip)
        {
            team_type_string = "strip";
        }
        else
        {
            team_type_string = "solid";
        }

        winning_team_text.text = "Winning Team : " + team_type_string;
    }
}
