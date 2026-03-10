namespace bcd
{
    /// <summary>Box and separator line drawing style.</summary>
    public enum LineStyle
    {
        Dotted = 1,
        Dashed = 2,
        Single = 3,
        Double = 4
    }

    /// <summary>Horizontal text alignment within a bordered box.</summary>
    public enum TextPosition
    {
        Left   = 1,
        Center = 2,
        Right  = 3
    }

    /// <summary>Text transformation applied before rendering.</summary>
    public enum TextStyle
    {
        None       = 0,
        Spaced     = 1,
        Caps       = 2,
        SpacedCaps = 3
    }
}
