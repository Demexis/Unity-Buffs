using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Integer processor that sums the original value with the processor's value.
    /// </summary>
    public sealed class IntBuffAdder : BuffProcessor<int> {
        public IntBuffAdder(int increment) : base(value => value + increment) { }
        public IntBuffAdder(Func<int> getIncrement) : base(value => value + getIncrement.Invoke()) { }
    }
}