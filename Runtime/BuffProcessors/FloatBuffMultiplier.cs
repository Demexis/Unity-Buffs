using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Float processor that multiplies the value by the processor's value.<br/>
    /// Default order: <see cref="BuffOrder.Multiplicative"/>.
    /// </summary>
    public sealed class FloatBuffMultiplier : BuffProcessor<float> {
        public override int DefaultOrder => BuffOrder.Multiplicative;

        public FloatBuffMultiplier(float multiplier) : base(value => value * multiplier) { }
        public FloatBuffMultiplier(Func<float> getMultiplier) : base(value => value * getMultiplier.Invoke()) { }
    }
}
