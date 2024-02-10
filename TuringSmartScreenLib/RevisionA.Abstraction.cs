namespace TuringSmartScreenLib;

internal sealed class ScreenWrapperRevisionA : ScreenBase
{
    private readonly TuringSmartScreenRevisionA screen;

    public ScreenWrapperRevisionA(TuringSmartScreenRevisionA screen, int width, int height)
        : base(width, height)
    {
        this.screen = screen;
    }

    public override void Dispose() => screen.Dispose();

    public override void Reset() => screen.Reset();

    public override void Clear() => screen.Clear();

    public override void ScreenOff() => screen.ScreenOff();

    public override void ScreenOn() => screen.ScreenOn();

    public override void SetBrightness(byte level) => screen.SetBrightness(255 - (byte)((float)level / 100 * 255));

    protected override bool SetOrientation(ScreenOrientation orientation)
    {
        switch (orientation)
        {
            case ScreenOrientation.Portrait:
                screen.SetOrientation(TuringSmartScreenRevisionA.Orientation.Portrait, Width, Height);
                return true;
            case ScreenOrientation.ReversePortrait:
                screen.SetOrientation(TuringSmartScreenRevisionA.Orientation.ReversePortrait, Width, Height);
                return true;
            case ScreenOrientation.Landscape:
                screen.SetOrientation(TuringSmartScreenRevisionA.Orientation.Landscape, Width, Height);
                return true;
            case ScreenOrientation.ReverseLandscape:
                screen.SetOrientation(TuringSmartScreenRevisionA.Orientation.ReverseLandscape, Width, Height);
                return true;
        }

        return false;
    }

    public override IScreenBuffer CreateBuffer(int width, int height) => new TuringSmartScreenBufferA(width, height);

    public override void DisplayBitmap(int x, int y, int width, int height, IScreenBuffer buffer) => screen.DisplayBitmap(x, y, width, height, ((TuringSmartScreenBufferA)buffer).Buffer);
}