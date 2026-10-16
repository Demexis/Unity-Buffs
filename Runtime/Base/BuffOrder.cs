namespace Demegraunt.Framework {
    /// <summary>
    /// Predefined processing orders.<br/><br/>
    /// Processors with a lower order are applied first. Processors with the same order
    /// are applied in the order they were added. Any <see cref="int"/> can be used as an order,
    /// so you can insert your own stages between these values.
    /// </summary>
    public static class BuffOrder {
        /// <summary>
        /// Default order of <see cref="BuffProcessor{T}"/>. Applied before additive processors.
        /// </summary>
        public const int Default = 0;

        /// <summary>
        /// Default order of adders (<see cref="FloatBuffAdder"/>, <see cref="IntBuffAdder"/>).
        /// </summary>
        public const int Additive = 100;

        /// <summary>
        /// Default order of multipliers (<see cref="FloatBuffMultiplier"/>, <see cref="IntBuffMultiplier"/>).
        /// </summary>
        public const int Multiplicative = 200;

        /// <summary>
        /// Order for processors that should be applied last, e.g. clamping.
        /// </summary>
        public const int Final = 1000;
    }
}
