using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Integer processor that sums the value with the processor's value.<br/>
    /// Default order: <see cref="BuffOrder.Additive"/>.
    /// </summary>
    public sealed class IntBuffAdder : BuffProcessor<int> {
        public override int DefaultOrder => BuffOrder.Additive;

        public IntBuffAdder(int increment) : base(value => value + increment) { }
        public IntBuffAdder(Func<int> getIncrement) : base(value => value + getIncrement.Invoke()) { }
    }
}
