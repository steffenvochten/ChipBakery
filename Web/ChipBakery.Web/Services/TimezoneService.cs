namespace ChipBakery.Web.Services;

/// <summary>
/// Scoped service (one per Blazor circuit) that stores the browser's UTC offset
/// so server-side code can format UTC timestamps in the user's local time.
/// Populated via JS interop in MainLayout on first render.
/// </summary>
public sealed class TimezoneService
{
    // JS getTimezoneOffset() returns minutes to SUBTRACT from local to get UTC,
    // so UTC + (-offset minutes) = local. E.g. CEST = UTC+2 → jsOffset = -120.
    private int _jsOffsetMinutes = 0;

    public bool IsInitialized { get; private set; }

    public event Action? OnInitialized;

    public void Initialize(int jsOffsetMinutes)
    {
        _jsOffsetMinutes = jsOffsetMinutes;
        IsInitialized = true;
        OnInitialized?.Invoke();
    }

    public DateTime ToLocal(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).AddMinutes(-_jsOffsetMinutes);

    public string FormatTime(DateTime? utc) =>
        utc.HasValue ? ToLocal(utc.Value).ToString("HH:mm:ss") : "—";

    public string FormatShort(DateTime? utc) =>
        utc.HasValue ? ToLocal(utc.Value).ToString("MMM dd, HH:mm") : "—";

    public string FormatLong(DateTime? utc) =>
        utc.HasValue ? ToLocal(utc.Value).ToString("MMM dd yyyy, HH:mm") : "—";
}
