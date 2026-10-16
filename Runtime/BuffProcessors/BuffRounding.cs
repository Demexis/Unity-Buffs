namespace Demegraunt.Framework {
    /// <summary>
    /// Rounding mode for integer processors with fractional parameters.
    /// </summary>
    public enum BuffRounding {
        /// <summary>
        /// To the nearest integer; midpoint values are rounded away from zero (2.5 → 3, -2.5 → -3).
        /// </summary>
        Nearest,

        /// <summary>
        /// Down to the nearest integer (2.7 → 2, -2.3 → -3).
        /// </summary>
        Floor,

        /// <summary>
        /// Up to the nearest integer (2.3 → 3, -2.7 → -2).
        /// </summary>
        Ceiling,

        /// <summary>
        /// Towards zero (2.7 → 2, -2.7 → -2).
        /// </summary>
        Truncate
    }
}
