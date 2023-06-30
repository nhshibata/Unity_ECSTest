/// <summary>
/// ƒQ[ƒ€ƒvƒŒƒCŠÔŠÇ—
/// </summary>
public class PlayTime
{
    float time = 120.0f;

    public float Time { get => time; private set => time = value; }

    public bool IsPlaying()
    {
        return time > 0.0f;
    }

    public void DelTime(float deltaTime)
    {
        time -= deltaTime;
    }
}
