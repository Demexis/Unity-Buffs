using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Integer processor that sums the original value with the processor's value.
    /// </summary>
    public sealed class IntBuffAdder : BuffProcessor<int> {
        public Func<int> GetProcessorValue { get; set; }

        public IntBuffAdder(int increment) {
            GetProcessorValue = () => increment;
        }

        public IntBuffAdder(Func<int> getIncrement) {
            GetProcessorValue = getIncrement;
        }

        public override int Process(int value) {
            return value + GetProcessorValue.Invoke();
        }
    }
}