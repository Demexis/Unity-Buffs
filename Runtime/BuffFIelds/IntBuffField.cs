using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Integer buff-field. This is done to combine the inspector's integer-field and the buff instance.
    /// </summary>
    [Serializable]
    public sealed class IntBuffField : BuffField<int> {
        public IntBuffField(int originalValue) : base(originalValue) { }
    }
}