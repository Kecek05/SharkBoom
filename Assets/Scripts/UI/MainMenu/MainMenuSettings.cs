using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private AudioMixer sfxMixer;
    [SerializeField] private TextMeshProUGUI qualityText;

    [SerializeField] private Image musicImage;
    [SerializeField] private Image sfxImage;

    [SerializeField] private Sprite offIcon;
    [SerializeField] private Sprite onIcon;

    private const string musicVolume = "musicVolume";
    private const string sfxVolume = "sfxVolume";

    private const string musicKey = "musicEnabled";
    private const string sfxKey = "sfxEnabled";
    private const string qualityKey = "graphicsQuality";

    private bool isMusicEnabled;
    private bool isSFXEnabled;

    private void Start()
    {
        isMusicEnabled = PlayerPrefs.GetInt(musicKey, 1) == 1; // Compare with 1 for true
        ApplyMusicState();

        isSFXEnabled = PlayerPrefs.GetInt(sfxKey, 1) == 1;
        ApplySfxState();

        int savedQuality = PlayerPrefs.GetInt(qualityKey, QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(savedQuality);
        UpdateQualityText();
    }

    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;
        ApplyMusicState();

        PlayerPrefs.SetInt(musicKey, isMusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyMusicState()
    {
        musicMixer.SetFloat(musicVolume, isMusicEnabled ? 0f : -80f);
        musicImage.sprite = isMusicEnabled ? onIcon : offIcon;
    }


    public void ToggleSFX()
    {
        isSFXEnabled = !isSFXEnabled;
        ApplySfxState();

        PlayerPrefs.SetInt(sfxKey, isSFXEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplySfxState()
    {
        sfxMixer.SetFloat(sfxVolume, isSFXEnabled ? 0f : -80f); 
        sfxImage.sprite = isSFXEnabled ? onIcon : offIcon;
    }

    public void IncreaseGraphics()
    {
        int maxLevel = QualitySettings.names.Length - 1;

        if (QualitySettings.GetQualityLevel() < maxLevel)
        {
            QualitySettings.IncreaseLevel();
            UpdateQualityText();
            SaveGraphicsQuality();
        }
    }

    public void DecreaseGraphics()
    {
        if (QualitySettings.GetQualityLevel() > 0)
        {
            QualitySettings.DecreaseLevel();
            UpdateQualityText();
            SaveGraphicsQuality();
        }
    }

    private void UpdateQualityText()
    {
        string[] qualityNames = QualitySettings.names;
        int currentQualityLevel = QualitySettings.GetQualityLevel();
        qualityText.text = qualityNames[currentQualityLevel];
    }

    private void SaveGraphicsQuality()
    {
        PlayerPrefs.SetInt(qualityKey, QualitySettings.GetQualityLevel());
        PlayerPrefs.Save();
    }
}
