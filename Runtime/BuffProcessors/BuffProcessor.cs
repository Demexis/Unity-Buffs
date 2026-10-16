using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Value processor of a <see cref="Buff{T}"/>.<br/>
    /// You can inherit from this class when adding new buff processor types:
    /// either pass the callback to the base constructor, or override <see cref="Process"/>.
    /// </summary>
    /// <typeparam name="T">Buff processor value type.</typeparam>
    public class BuffProcessor<T> {
        /// <summary>
        /// Is called when the processing logic changes. Buffs that contain this processor
        /// invalidate their cache and raise their own <see cref="Buff{T}.Changed"/> event.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Logic of processing the value. Setting it raises <see cref="Changed"/>.<br/><br/>
        /// Has no effect if a derived class overrides <see cref="Process"/> without calling the base method.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value is null.</exception>
        public Func<T, T> ProcessCallback {
            get => processCallback;
            set {
                processCallback = value ?? throw new ArgumentNullException(nameof(value));
                NotifyChanged();
            }
        }

        /// <summary>
        /// Order used when the processor is added to a buff without an explicit order.
        /// </summary>
        public virtual int DefaultOrder => BuffOrder.Default;

        private Func<T, T> processCallback;

        /// <exception cref="ArgumentNullException"><paramref name="processCallback"/> is null.</exception>
        public BuffProcessor(Func<T, T> processCallback) {
            this.processCallback = processCallback ?? throw new ArgumentNullException(nameof(processCallback));
        }

        /// <summary>
        /// Constructor for derived classes that override <see cref="Process"/>.
        /// </summary>
        protected BuffProcessor() {
            processCallback = value => value;
        }

        /// <summary>
        /// Process the value.
        /// </summary>
        /// <param name="value">Value from the previous processor (or the original value).</param>
        /// <returns>Processed value.</returns>
        public virtual T Process(T value) {
            return processCallback.Invoke(value);
        }

        /// <summary>
        /// Raises <see cref="Changed"/>.<br/><br/>
        /// Call it when an external state used by the processor changes,
        /// so the buffs with enabled cache recalculate their result.
        /// </summary>
        public void NotifyChanged() {
            Changed?.Invoke();
        }
    }
}
