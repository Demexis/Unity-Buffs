using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Integer processor that multiplies the original value with the processor's value.
    /// </summary>
    public sealed class IntBuffMultiplier : BuffProcessor<int> {
        public Func<int> GetProcessorValue { get; set; }

        public IntBuffMultiplier(int multiplier) {
            GetProcessorValue = () => multiplier;
        }

        public IntBuffMultiplier(Func<int> getMultiplier) {
            GetProcessorValue = getMultiplier;
        }

        public override int Process(int value) {
            return value * GetProcessorValue.Invoke();
        }
    }
}