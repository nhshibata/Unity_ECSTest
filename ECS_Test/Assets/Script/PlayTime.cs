
/// <summary>
/// ƒQ[ƒ€ƒvƒŒƒCŠÔŠÇ—
/// floatŒ^‚Ìtime‚ğŠ
/// </summary>
public class PlayTime
{
    float time = 120.0f;

    public float Time { get => time; private set => time = value; }

    public bool IsPlaying()
    {
        return time > 0.0f;
    }

    public void DecTime(float deltaTime)
    {
        time -= deltaTime;
    }

    public void AddTime(float add)
    {
        time += Time;
    }
}
