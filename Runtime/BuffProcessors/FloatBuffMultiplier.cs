using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Float processor that multiplies the original value with the processor's value.
    /// </summary>
    public sealed class FloatBuffMultiplier : BuffProcessor<float> {
        public Func<float> GetProcessorValue { get; set; }

        public FloatBuffMultiplier(float multiplier) {
            GetProcessorValue = () => multiplier;
        }

        public FloatBuffMultiplier(Func<float> getMultiplier) {
            GetProcessorValue = getMultiplier;
        }

        public override float Process(float value) {
            return value * GetProcessorValue.Invoke();
        }
    }
}
