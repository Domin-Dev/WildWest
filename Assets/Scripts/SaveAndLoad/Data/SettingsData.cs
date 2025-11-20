

[System.Serializable]
public class SettingsData
{
    public int resolutionWidth;
    public int resolutionHeight;
    public bool fullScreen;
    public int fpsLimit;
    public int fontIndex;

    public float soundsVolume;
    public float musicVolume;

    public string language;

    public override string ToString()
    {
        return  $"{resolutionWidth} x {resolutionHeight} , FullScreen: {fullScreen} , FPS Limit: {fpsLimit}";
    }
}