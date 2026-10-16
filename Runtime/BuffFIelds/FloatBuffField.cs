using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Float buff-field. Same as <c>BuffField&lt;float&gt;</c>, kept for compatibility and brevity.
    /// </summary>
    [Serializable]
    public sealed class FloatBuffField : BuffField<float> {
        public FloatBuffField() { }
        public FloatBuffField(float originalValue) : base(originalValue) { }
    }
}
