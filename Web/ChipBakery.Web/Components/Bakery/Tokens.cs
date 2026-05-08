namespace ChipBakery.Web.Components.Bakery;

/// <summary>Semantic colour palette for UI primitives. Maps to CSS tokens defined in app.css.</summary>
public enum BakeryColor
{
    Chocolate,
    Kraft,
    Raspberry,
    Basil,
    Blueberry,
    Lemon,
    Ink,
    Paper,
}

public enum BakerySize { Sm, Md, Lg }

/// <summary>Decorative tape colour. Used by &lt;Tape&gt; and &lt;PaperCard Tape="..."/&gt;.</summary>
public enum TapeColor { Default, Lemon, Basil, Raspberry, Kraft }

/// <summary>Optional torn-paper bottom edge on a card. None = clean rectangle.</summary>
public enum DeckleVariant { None, V1, V2, V3, V4 }

/// <summary>Button visual variant.</summary>
public enum ButtonVariant { Chocolate, Kraft, Outline, Ghost, Danger }

/// <summary>Status taxonomy across the app — Order, Production job, Inventory level, etc.</summary>
public enum BakeryStatus
{
    // Orders
    Placed,
    Processing,
    Completed,
    Cancelled,
    // Production jobs
    Scheduled,
    AwaitingIngredients,
    Baking,
    Ready,
    // Generic
    Queued,
    Low,
    InStock,
    OutOfStock,
}

internal static class TokenExtensions
{
    internal static string CssName(this BakeryColor c) => c switch
    {
        BakeryColor.Chocolate => "chocolate",
        BakeryColor.Kraft => "kraft",
        BakeryColor.Raspberry => "raspberry",
        BakeryColor.Basil => "basil",
        BakeryColor.Blueberry => "blueberry",
        BakeryColor.Lemon => "lemon",
        BakeryColor.Ink => "ink",
        BakeryColor.Paper => "paper",
        _ => "ink",
    };

    internal static string CssName(this TapeColor c) => c switch
    {
        TapeColor.Lemon => "lemon",
        TapeColor.Basil => "basil",
        TapeColor.Raspberry => "raspberry",
        TapeColor.Kraft => "kraft",
        _ => "default",
    };
}

/// <summary>
/// Helpers for producing stable, deterministic visual variation (rotation, deckle pick)
/// from an entity id. Critical: must NOT use Random — pages refresh on a 2 s timer and
/// rotation must stay stable across renders.
/// </summary>
public static class BakeryRandom
{
    public static int RotationSeed(object? key)
    {
        if (key is null) return 0;
        return Math.Abs(HashCode.Combine(key)) % 7;
    }

    public static string Wobble(object? key) => $"wobble-{RotationSeed(key)}";

    public static DeckleVariant Deckle(object? key)
    {
        if (key is null) return DeckleVariant.None;
        return (Math.Abs(HashCode.Combine(key, "deckle")) % 4) switch
        {
            0 => DeckleVariant.V1,
            1 => DeckleVariant.V2,
            2 => DeckleVariant.V3,
            _ => DeckleVariant.V4,
        };
    }
}

/// <summary>Maps a BakeryStatus to its display label, stamp colour, and glyph id in the sprite.</summary>
public static class StatusVisuals
{
    public record Visual(string Label, BakeryColor Color, string Glyph);

    public static Visual For(BakeryStatus s) => s switch
    {
        BakeryStatus.Placed              => new("Placed",     BakeryColor.Blueberry, "glyph-clock"),
        BakeryStatus.Processing          => new("Processing", BakeryColor.Lemon,     "glyph-clock"),
        BakeryStatus.Completed           => new("Completed",  BakeryColor.Basil,     "glyph-check"),
        BakeryStatus.Cancelled           => new("Cancelled",  BakeryColor.Raspberry, "glyph-x"),
        BakeryStatus.Scheduled           => new("Scheduled",  BakeryColor.Blueberry, "glyph-clock"),
        BakeryStatus.AwaitingIngredients => new("Awaiting",   BakeryColor.Raspberry, "glyph-clock"),
        BakeryStatus.Baking              => new("Baking",     BakeryColor.Lemon,     "glyph-fire"),
        BakeryStatus.Ready               => new("Ready",      BakeryColor.Basil,     "glyph-check"),
        BakeryStatus.Queued              => new("Queued",     BakeryColor.Blueberry, "glyph-clock"),
        BakeryStatus.Low                 => new("Low",        BakeryColor.Raspberry, "glyph-clock"),
        BakeryStatus.InStock             => new("In Stock",   BakeryColor.Basil,     "glyph-check"),
        BakeryStatus.OutOfStock          => new("Sold Out",   BakeryColor.Raspberry, "glyph-x"),
        _                                => new(s.ToString(), BakeryColor.Kraft,     "glyph-clock"),
    };

    /// <summary>Best-effort string parse for raw status strings coming from API responses.</summary>
    public static BakeryStatus Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return BakeryStatus.Queued;
        return raw.Trim().ToLowerInvariant() switch
        {
            "placed"               => BakeryStatus.Placed,
            "processing"           => BakeryStatus.Processing,
            "completed"            => BakeryStatus.Completed,
            "cancelled" or "canceled" => BakeryStatus.Cancelled,
            "scheduled"            => BakeryStatus.Scheduled,
            "awaitingingredients"  => BakeryStatus.AwaitingIngredients,
            "awaiting ingredients" => BakeryStatus.AwaitingIngredients,
            "awaiting"             => BakeryStatus.AwaitingIngredients,
            "baking"               => BakeryStatus.Baking,
            "ready"                => BakeryStatus.Ready,
            "queued"               => BakeryStatus.Queued,
            "low"                  => BakeryStatus.Low,
            "instock" or "in stock" => BakeryStatus.InStock,
            "outofstock" or "out of stock" or "soldout" or "sold out" => BakeryStatus.OutOfStock,
            _ => Enum.TryParse<BakeryStatus>(raw, true, out var parsed) ? parsed : BakeryStatus.Queued,
        };
    }
}
