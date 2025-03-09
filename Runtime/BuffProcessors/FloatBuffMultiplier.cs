using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Float processor that multiplies the original value with the processor's value.
    /// </summary>
    public sealed class FloatBuffMultiplier : BuffProcessor<float> {
        public FloatBuffMultiplier(float multiplier) : base(value => value * multiplier) { }
        public FloatBuffMultiplier(Func<float> getMultiplier) : base(value => value * getMultiplier.Invoke()) { }
    }
}