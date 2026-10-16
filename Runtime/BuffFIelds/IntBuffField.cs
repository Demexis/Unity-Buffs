using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Integer buff-field. Same as <c>BuffField&lt;int&gt;</c>, kept for compatibility and brevity.
    /// </summary>
    [Serializable]
    public sealed class IntBuffField : BuffField<int> {
        public IntBuffField() { }
        public IntBuffField(int originalValue) : base(originalValue) { }
    }
}
