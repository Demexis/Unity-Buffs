using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Float processor that sums the value with the processor's value.<br/>
    /// Default order: <see cref="BuffOrder.Additive"/>.
    /// </summary>
    public sealed class FloatBuffAdder : BuffProcessor<float> {
        public override int DefaultOrder => BuffOrder.Additive;

        public FloatBuffAdder(float increment) : base(value => value + increment) { }
        public FloatBuffAdder(Func<float> getIncrement) : base(value => value + getIncrement.Invoke()) { }
    }
}
