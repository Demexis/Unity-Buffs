using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Integer processor that multiplies the original value with the processor's value.
    /// </summary>
    public sealed class IntBuffMultiplier : BuffProcessor<int> {
        public IntBuffMultiplier(int multiplier) : base(value => value * multiplier) { }
        public IntBuffMultiplier(Func<int> getMultiplier) : base(value => value * getMultiplier.Invoke()) { }
    }
}