using System;
using UnityEngine;

namespace Demegraunt.Framework {
    /// <summary>
    /// Generic buff-field. This is done to combine the inspector's field and the buff instance allocated for this field.<br/><br/>
    /// You should inherit from this class if you want the original value to be displayed in the inspector.
    /// </summary>
    /// <typeparam name="T">The type of buff value.</typeparam>
    [Serializable]
    public class BuffField<T> {
        /// <summary>
        /// Original value that can be changed in the inspector.
        /// </summary>
        [field: SerializeField] private T OriginalValue { get; set; }

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

        public BuffField(T originalValue) {
            OriginalValue = originalValue;
        }

        /// <summary>
        /// Apply all processors to the <see cref="OriginalValue"/> and return result. 
        /// </summary>
        /// <returns>Result value.</returns>
        public T Calculate() {
            return Buff.Calculate();
        }
    }
}