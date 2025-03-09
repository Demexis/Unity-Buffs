using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Float processor that sums the original value with the processor's value.
    /// </summary>
    public sealed class FloatBuffAdder : BuffProcessor<float> {
        public FloatBuffAdder(float increment) : base(value => value + increment) { }
        public FloatBuffAdder(Func<float> getIncrement) : base(value => value + getIncrement.Invoke()) { }
    }
}