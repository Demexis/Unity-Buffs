using System;
using UnityEngine;

namespace Demegraunt.Framework {
    /// <summary>
    /// Serializable buff-field. Combines the inspector's field for the original value and the buff instance allocated for this field.<br/><br/>
    /// Can be used directly with any serializable type, e.g. <c>BuffField&lt;Vector3&gt;</c>.
    /// </summary>
    /// <typeparam name="T">The type of buff value.</typeparam>
    [Serializable]
    public class BuffField<T> : ISerializationCallbackReceiver {
        // The serialized name of this property ("<OriginalValue>k__BackingField") must stay unchanged
        // to keep the data of existing projects.
        /// <summary>
        /// Original value that can be changed in the inspector or with <see cref="SetOriginalValue"/>.
        /// </summary>
        [field: SerializeField] public T OriginalValue { get; private set; }

        /// <summary>
        /// Guaranteed buff instance.
        /// </summary>
        public Buff<T> Buff {
            get {
                buff ??= new Buff<T>(() => OriginalValue);
                return buff;
            }
        }

        private Buff<T> buff;

        public BuffField() { }

        public BuffField(T originalValue) {
            OriginalValue = originalValue;
        }

        /// <summary>
        /// Change the original value and notify the buff about it.
        /// </summary>
        public void SetOriginalValue(T value) {
            OriginalValue = value;
            buff?.NotifyChanged();
        }

        /// <summary>
        /// Apply all processors to the <see cref="OriginalValue"/> and return result.
        /// </summary>
        /// <returns>Result value.</returns>
        public T Calculate() {
            return Buff.Calculate();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            // The original value may be changed in the inspector. Unity API and events
            // must not be used here, so only the cache is invalidated.
            buff?.InvalidateCache();
        }
    }
}
