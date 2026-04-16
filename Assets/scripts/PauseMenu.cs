using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    private bool isPaused = false;

    void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);

                if (settingsPanel.activeSelf)
                {
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    isPaused = true;
                }
                else
                {
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    isPaused = false;
                }
            }
        }
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }
}