using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Integer processor that multiplies the value by the processor's value.<br/>
    /// Fractional multipliers are supported; the result is rounded according to <see cref="BuffRounding"/>.<br/>
    /// Default order: <see cref="BuffOrder.Multiplicative"/>.
    /// </summary>
    public sealed class IntBuffMultiplier : BuffProcessor<int> {
        public override int DefaultOrder => BuffOrder.Multiplicative;

        public IntBuffMultiplier(int multiplier) : base(value => value * multiplier) { }
        public IntBuffMultiplier(Func<int> getMultiplier) : base(value => value * getMultiplier.Invoke()) { }

        public IntBuffMultiplier(float multiplier, BuffRounding rounding = BuffRounding.Nearest)
            : base(value => Round((double)value * multiplier, rounding)) { }

        public IntBuffMultiplier(Func<float> getMultiplier, BuffRounding rounding = BuffRounding.Nearest)
            : base(value => Round((double)value * getMultiplier.Invoke(), rounding)) { }

        private static int Round(double value, BuffRounding rounding) {
            return rounding switch {
                BuffRounding.Floor => (int)Math.Floor(value),
                BuffRounding.Ceiling => (int)Math.Ceiling(value),
                BuffRounding.Truncate => (int)Math.Truncate(value),
                _ => (int)Math.Round(value, MidpointRounding.AwayFromZero)
            };
        }
    }
}
