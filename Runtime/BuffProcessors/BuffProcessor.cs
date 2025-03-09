using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Generic version of <see cref="BaseBuffProcessor"/>.<br/>
    /// You should inherit from this class when adding new buff processor types.
    /// </summary>
    /// <typeparam name="T">Buff processor value type.</typeparam>
    public class BuffProcessor<T> : BaseBuffProcessor {
        /// <summary>
        /// Logic of processing the original value.
        /// </summary>
        public Func<T, T> ProcessCallback { get; set; }

        public BuffProcessor(Func<T, T> processCallback) : base(typeof(T)) {
            ProcessCallback = processCallback;
        }

        public override object ProcessObject(object value) {
            return ProcessCallback.Invoke((T)value);
        }
    }
}