using System.Collections;
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

    [Header("Current Player UI")]
    [SerializeField] private TextMeshProUGUI teamNameText;      // TeamText
    [SerializeField] private TextMeshProUGUI scoreLabelText;    // ScoreLabel (НОВЫЙ)
    [SerializeField] private TextMeshProUGUI scoreText;         // ScoreValue
    [SerializeField] private TextMeshProUGUI abilityLabelText;  // AbilityLabel
    [SerializeField] private TextMeshProUGUI abilityPointsText; // AbilityValue
    [SerializeField] private GameObject current_player_panel;
    
    [SerializeField] private Canvas mainCanvas;
    
    [Header("Modifier Notification")]
    [SerializeField] private GameObject modifierNotificationPanel;
    [SerializeField] private TextMeshProUGUI modifierTitleText;
    [SerializeField] private TextMeshProUGUI modifierDescriptionText;
    [SerializeField] private float notificationDuration = 2f;
    private Coroutine notificationCoroutine;
    
    [Header("NotEnoughAP")]
    public GameObject notEnoughPointsText; // Перетащите текстовый объект в инспекторе
    public float notEnoughApNotificationDuration = 2f;
    private Coroutine notEnoughPointsCoroutine;


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
        if (current_player_panel != null)
        {
            if (teamNameText != null)
            {
                string displayTeamName = teamName == "Strip" ? "ПОЛОСАТЫЕ" : 
                    teamName == "Solid" ? "ЦЕЛЬНЫЕ" : 
                    "НЕТ КОМАНДЫ";
                teamNameText.text = $"{displayTeamName}";
            }
            
            if (scoreLabelText != null)
            {
                scoreLabelText.text = "СЧЕТ:";
            }
        
            if (scoreText != null)
            {
                scoreText.text = $"{teamScore}";
            }
        
            if (abilityLabelText != null)
            {
                abilityLabelText.text = "ОЧКИ СПОСОБНОСТЕЙ:";
            }
            
            if (abilityPointsText != null)
            {
                abilityPointsText.text = $"{abilityPoints}";
            }
        
            current_player_panel.SetActive(true);
        }
    }
    
    public void UpdateAbilityPoints(int newPoints)
    {
        if (abilityPointsText != null)
        {
            abilityPointsText.text = $"{newPoints}";
        }
    }
    
    public void UpdateScore(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{newScore}";
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
            team_type_string = "ПОЛОСАТЫХ";
        }
        else
        {
            team_type_string = "ЦЕЛЬНЫХ";
        }

        winning_team_text.text = "Победила команда : " + team_type_string;
    }
    
    public void HideAll()
    {
        if (mainCanvas != null)
            mainCanvas.enabled = false;
    }

    public void ShowAll()
    {
        if (mainCanvas != null)
            mainCanvas.enabled = true;
    }
    
    public void ShowModifierNotification(string title, string description)
    {
        if (modifierNotificationPanel == null || modifierTitleText == null || modifierDescriptionText == null)
        {
            Debug.LogWarning("Modifier notification UI elements not assigned!");
            return;
        }
  
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        
        modifierTitleText.text = title;
        modifierDescriptionText.text = description;

        notificationCoroutine = StartCoroutine(ShowNotificationCoroutine());
    }

    private IEnumerator ShowNotificationCoroutine()
    {
        CanvasGroup canvasGroup = modifierNotificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = modifierNotificationPanel.AddComponent<CanvasGroup>();
        }

        modifierNotificationPanel.SetActive(true);

        float elapsedTime = 0f;
        float fadeInDuration = 0.2f;
        while (elapsedTime < fadeInDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeInDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
        
        yield return new WaitForSeconds(notificationDuration);

        elapsedTime = 0f;
        float fadeOutDuration = 0.2f;
        while (elapsedTime < fadeOutDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;

        modifierNotificationPanel.SetActive(false);
    }
    
    public void ShowNotEnoughPointsNotification()
    {
        if (notEnoughPointsText == null)
        {
            Debug.LogWarning("Not enough points text object not assigned!");
            return;
        }
        
        if (notEnoughPointsCoroutine != null)
        {
            StopCoroutine(notEnoughPointsCoroutine);
            notEnoughPointsText.SetActive(false);
        }
        
        notEnoughPointsText.SetActive(true);
        notEnoughPointsCoroutine = StartCoroutine(HideNotificationAfterDelay());
    }

    private System.Collections.IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        notEnoughPointsText.SetActive(false);
        notEnoughPointsCoroutine = null;
    }
}
