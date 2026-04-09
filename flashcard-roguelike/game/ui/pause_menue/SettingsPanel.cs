using Godot;

public partial class SettingsPanel : Control
{
    private HSlider _masterSlider;
    private HSlider _musicSlider;
    private HSlider _sfxSlider;

    public override void _Ready()
    {
        const string basePath = "SettingsPanel/MarginContainer/SliderContainer/";

        _masterSlider = GetNode<HSlider>(basePath + "MasterRow/MasterSlider");
        _musicSlider = GetNode<HSlider>(basePath + "MusicRow/MusicSlider");
        _sfxSlider   = GetNode<HSlider>(basePath + "SFXRow/SFXSlider");

        AudioManager.Instance?.RegisterButton(GetNodeOrNull<Button>(basePath + "Back"));

        // Set up event handlers and use lambda to convert from double to float via AudioManager
        _masterSlider.ValueChanged += v => AudioManager.Instance?.SetBusVolume("Master", (float)v);
        _musicSlider.ValueChanged += v => AudioManager.Instance?.SetBusVolume("Music", (float)v);
        _sfxSlider.ValueChanged += v => AudioManager.Instance?.SetBusVolume("SFX", (float)v);
    }


    // Call when showing panel to sync
    public void SyncFromAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        _masterSlider.SetValueNoSignal(AudioManager.Instance.GetBusVolume("Master"));
        _musicSlider.SetValueNoSignal(AudioManager.Instance.GetBusVolume("Music"));
        _sfxSlider.SetValueNoSignal(AudioManager.Instance.GetBusVolume("SFX"));
    }
}
