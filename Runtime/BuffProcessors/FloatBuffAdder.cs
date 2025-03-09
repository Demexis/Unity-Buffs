using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Float processor that sums the original value with the processor's value.
    /// </summary>
    public sealed class FloatBuffAdder : BuffProcessor<float> {
        public Func<float> GetProcessorValue { get; set; }

        public FloatBuffAdder(float increment) {
            GetProcessorValue = () => increment;
        }

        public FloatBuffAdder(Func<float> getIncrement) {
            GetProcessorValue = getIncrement;
        }

        public override float Process(float value) {
            return value + GetProcessorValue.Invoke();
        }
    }
}
