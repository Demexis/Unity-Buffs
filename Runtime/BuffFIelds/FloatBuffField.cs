using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Float buff-field. This is done to combine the inspector's float-field and the buff instance.
    /// </summary>
    [Serializable]
    public sealed class FloatBuffField : BuffField<float> {
        public FloatBuffField(float originalValue) : base(originalValue) { }
    }
}