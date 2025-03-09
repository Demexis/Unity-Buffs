namespace Demegraunt.Framework {
    /// <summary>
    /// Generic version of <see cref="BaseBuffProcessor"/>.<br/>
    /// You should inherit from this class when adding new buff processor types.
    /// </summary>
    /// <typeparam name="T">Buff processor value type.</typeparam>
    public abstract class BuffProcessor<T> : BaseBuffProcessor {
        protected BuffProcessor() : base(typeof(T)) { }

        public override object ProcessObject(object value) {
            return Process((T)value);
        }

        public abstract T Process(T value);
    }
}