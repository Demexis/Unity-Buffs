using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Buff of a value: the original value is sequentially transformed by registered processors.<br/><br/>
    /// Processors are applied in ascending <see cref="BuffOrder">order</see>;
    /// processors with the same order are applied in the order they were added.<br/>
    /// The buff may be safely modified while it is being calculated (e.g. from a processor or
    /// a <see cref="Changed"/> handler): changes take effect starting from the next calculation.
    /// </summary>
    /// <typeparam name="T">The type of buff value.</typeparam>
    public class Buff<T> : IBuff {
        private readonly struct Entry {
            public readonly Guid id;
            public readonly BuffProcessor<T> processor;
            public readonly int order;
            public readonly object source;

            public Entry(Guid id, BuffProcessor<T> processor, int order, object source) {
                this.id = id;
                this.processor = processor;
                this.order = order;
                this.source = source;
            }

            public Entry WithProcessor(BuffProcessor<T> newProcessor) {
                return new Entry(id, newProcessor, order, source);
            }
        }

        /// <inheritdoc/>
        public event Action Changed;

        /// <inheritdoc/>
        public Type ValueType => typeof(T);

        /// <inheritdoc/>
        public int Count => entries.Length;

        /// <inheritdoc/>
        public bool CacheEnabled {
            get => cacheEnabled;
            set {
                cacheEnabled = value;
                isCacheValid = false;
            }
        }

        /// <summary>
        /// Current original value (before processing).
        /// </summary>
        public T OriginalValue => getOriginalValue != null ? getOriginalValue.Invoke() : originalValue;

        /// <summary>
        /// Registered processors sorted by order. The array is replaced (never mutated) on every change,
        /// so iteration over a stale reference is always safe.
        /// </summary>
        private Entry[] entries = Array.Empty<Entry>();

        private readonly Action processorChangedHandler;

        private Func<T> getOriginalValue;
        private T originalValue;

        private bool cacheEnabled;
        private bool isCacheValid;
        private T cachedValue;
        private int version;
        private bool isCalculating;

        /// <summary>
        /// Create a buff with a constant original value. It can be changed with <see cref="SetOriginalValue(T)"/>.
        /// </summary>
        public Buff(T originalValue) {
            this.originalValue = originalValue;
            processorChangedHandler = NotifyChanged;
        }

        /// <summary>
        /// Create a buff with an original value that is requested on every calculation.
        /// </summary>
        public Buff(Func<T> getOriginalValue) {
            this.getOriginalValue = getOriginalValue ?? throw new ArgumentNullException(nameof(getOriginalValue));
            processorChangedHandler = NotifyChanged;
        }

        /// <summary>
        /// Replace the original value with a constant and raise <see cref="Changed"/>.
        /// </summary>
        public void SetOriginalValue(T value) {
            getOriginalValue = null;
            originalValue = value;
            NotifyChanged();
        }

        /// <summary>
        /// Replace the original value callback and raise <see cref="Changed"/>.
        /// </summary>
        public void SetOriginalValue(Func<T> getValue) {
            getOriginalValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
            originalValue = default;
            NotifyChanged();
        }

        /// <summary>
        /// Apply all processors to the original value and return result.
        /// </summary>
        /// <returns>Result value.</returns>
        /// <exception cref="InvalidOperationException">The buff is calculated recursively (e.g. a processor of this buff calls its <see cref="Calculate"/>).</exception>
        public T Calculate() {
            if (cacheEnabled && isCacheValid) {
                return cachedValue;
            }

            if (isCalculating) {
                throw new InvalidOperationException($"Recursive calculation of {nameof(Buff<T>)}<{typeof(T).Name}> detected.");
            }

            var startVersion = version;
            var snapshot = entries;
            T value;

            isCalculating = true;
            try {
                value = OriginalValue;
                for (var i = 0; i < snapshot.Length; i++) {
                    value = snapshot[i].processor.Process(value);
                }
            } finally {
                isCalculating = false;
            }

            if (cacheEnabled && startVersion == version) {
                cachedValue = value;
                isCacheValid = true;
            }

            return value;
        }

        /// <inheritdoc/>
        public object CalculateObject() {
            return Calculate();
        }

        /// <summary>
        /// Register a new processor with an auto-generated ID.
        /// </summary>
        /// <param name="buffProcessor">Buff processor implementation.</param>
        /// <param name="order">Processing order. If null, <see cref="BuffProcessor{T}.DefaultOrder"/> is used.</param>
        /// <param name="source">Optional owner of the processor, used by <see cref="RemoveBySource"/>.</param>
        /// <returns>Handle that removes the processor when disposed.</returns>
        public BuffHandle Add(BuffProcessor<T> buffProcessor, int? order = null, object source = null) {
            return Add(Guid.NewGuid(), buffProcessor, order, source);
        }

        /// <summary>
        /// Register a new processing callback with an auto-generated ID.
        /// </summary>
        /// <param name="processCallback">Buff value processing callback.</param>
        /// <param name="order">Processing order. If null, <see cref="BuffOrder.Default"/> is used.</param>
        /// <param name="source">Optional owner of the processor, used by <see cref="RemoveBySource"/>.</param>
        /// <returns>Handle that removes the processor when disposed.</returns>
        public BuffHandle Add(Func<T, T> processCallback, int? order = null, object source = null) {
            return Add(new BuffProcessor<T>(processCallback), order, source);
        }

        /// <summary>
        /// Register a new processor.<br/>
        /// You can't add the same processor ID twice. Generate a new GUID or replace the old processor by using <see cref="Replace(Guid, BuffProcessor{T})"/>.
        /// </summary>
        /// <param name="processorId">Unique GUID. You can use <see cref="Guid.NewGuid()"/>.</param>
        /// <param name="buffProcessor">Buff processor implementation.</param>
        /// <param name="order">Processing order. If null, <see cref="BuffProcessor{T}.DefaultOrder"/> is used.</param>
        /// <param name="source">Optional owner of the processor, used by <see cref="RemoveBySource"/>.</param>
        /// <returns>Handle that removes the processor when disposed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffProcessor"/> is null.</exception>
        /// <exception cref="ArgumentException">A processor with the same ID is already registered.</exception>
        public BuffHandle Add(Guid processorId, BuffProcessor<T> buffProcessor, int? order = null, object source = null) {
            if (!TryAdd(processorId, buffProcessor, order, source)) {
                throw new ArgumentException($"A processor with ID {processorId} is already registered.", nameof(processorId));
            }

            return new BuffHandle(this, processorId);
        }

        /// <summary>
        /// Register a new processing callback.<br/>
        /// You can't add the same processor ID twice. Generate a new GUID or replace the old processor by using <see cref="Replace(Guid, Func{T, T})"/>.
        /// </summary>
        /// <param name="processorId">Unique GUID. You can use <see cref="Guid.NewGuid()"/>.</param>
        /// <param name="processCallback">Buff value processing callback.</param>
        /// <param name="order">Processing order. If null, <see cref="BuffOrder.Default"/> is used.</param>
        /// <param name="source">Optional owner of the processor, used by <see cref="RemoveBySource"/>.</param>
        /// <returns>Handle that removes the processor when disposed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="processCallback"/> is null.</exception>
        /// <exception cref="ArgumentException">A processor with the same ID is already registered.</exception>
        public BuffHandle Add(Guid processorId, Func<T, T> processCallback, int? order = null, object source = null) {
            return Add(processorId, new BuffProcessor<T>(processCallback), order, source);
        }

        /// <summary>
        /// Try to register a new processor.<br/><br/>
        /// Will return false if already contains a processor with the same ID.
        /// </summary>
        /// <param name="processorId">Unique GUID. You can use <see cref="Guid.NewGuid()"/>.</param>
        /// <param name="buffProcessor">Buff processor implementation.</param>
        /// <param name="order">Processing order. If null, <see cref="BuffProcessor{T}.DefaultOrder"/> is used.</param>
        /// <param name="source">Optional owner of the processor, used by <see cref="RemoveBySource"/>.</param>
        /// <returns>true if the processor was registered successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffProcessor"/> is null.</exception>
        public bool TryAdd(Guid processorId, BuffProcessor<T> buffProcessor, int? order = null, object source = null) {
            if (buffProcessor == null) {
                throw new ArgumentNullException(nameof(buffProcessor));
            }

            if (IndexOf(processorId) >= 0) {
                return false;
            }

            Insert(new Entry(processorId, buffProcessor, order ?? buffProcessor.DefaultOrder, source));
            buffProcessor.Changed += processorChangedHandler;
            NotifyChanged();
            return true;
        }

        /// <summary>
        /// Try to register a new processing callback.<br/><br/>
        /// Will return false if already contains a processor with the same ID.
        /// </summary>
        /// <param name="processorId">Unique GUID. You can use <see cref="Guid.NewGuid()"/>.</param>
        /// <param name="processCallback">Buff value processing callback.</param>
        /// <param name="order">Processing order. If null, <see cref="BuffOrder.Default"/> is used.</param>
        /// <param name="source">Optional owner of the processor, used by <see cref="RemoveBySource"/>.</param>
        /// <returns>true if the processor was registered successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="processCallback"/> is null.</exception>
        public bool TryAdd(Guid processorId, Func<T, T> processCallback, int? order = null, object source = null) {
            return TryAdd(processorId, new BuffProcessor<T>(processCallback), order, source);
        }

        /// <summary>
        /// Replace the processor, keeping its position, order and source.<br/>
        /// If the ID is not registered, the processor is added with its <see cref="BuffProcessor{T}.DefaultOrder"/>.
        /// </summary>
        /// <param name="processorId">GUID.</param>
        /// <param name="buffProcessor">New buff processor implementation.</param>
        /// <returns>Handle that removes the processor when disposed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffProcessor"/> is null.</exception>
        public BuffHandle Replace(Guid processorId, BuffProcessor<T> buffProcessor) {
            if (buffProcessor == null) {
                throw new ArgumentNullException(nameof(buffProcessor));
            }

            var index = IndexOf(processorId);
            if (index < 0) {
                return Add(processorId, buffProcessor);
            }

            var oldProcessor = entries[index].processor;
            var newEntries = (Entry[])entries.Clone();
            newEntries[index] = newEntries[index].WithProcessor(buffProcessor);
            entries = newEntries;

            oldProcessor.Changed -= processorChangedHandler;
            buffProcessor.Changed += processorChangedHandler;
            NotifyChanged();
            return new BuffHandle(this, processorId);
        }

        /// <summary>
        /// Replace the processor with a new processing callback, keeping its position, order and source.<br/>
        /// If the ID is not registered, the callback is added with <see cref="BuffOrder.Default"/> order.
        /// </summary>
        /// <param name="processorId">GUID.</param>
        /// <param name="processCallback">New buff value processing callback.</param>
        /// <returns>Handle that removes the processor when disposed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="processCallback"/> is null.</exception>
        public BuffHandle Replace(Guid processorId, Func<T, T> processCallback) {
            return Replace(processorId, new BuffProcessor<T>(processCallback));
        }

        /// <inheritdoc/>
        public bool Remove(Guid processorId) {
            return RemoveWhere(entry => entry.id == processorId) > 0;
        }

        /// <summary>
        /// Remove all registrations of the processor instance.
        /// </summary>
        /// <param name="buffProcessor">Registered processor instance.</param>
        /// <returns>Number of removed registrations.</returns>
        public int RemoveProcessor(BuffProcessor<T> buffProcessor) {
            if (buffProcessor == null) {
                return 0;
            }

            return RemoveWhere(entry => ReferenceEquals(entry.processor, buffProcessor));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public int RemoveBySource(object source) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            return RemoveWhere(entry => Equals(entry.source, source));
        }

        /// <inheritdoc/>
        public void Clear() {
            if (entries.Length == 0) {
                return;
            }

            RemoveWhere(_ => true);
        }

        /// <inheritdoc/>
        public bool Contains(Guid processorId) {
            return IndexOf(processorId) >= 0;
        }

        /// <summary>
        /// Get the processor registered with the specified ID.
        /// </summary>
        /// <param name="processorId">Registered GUID.</param>
        /// <param name="buffProcessor">Registered processor, or null if not found.</param>
        /// <returns>true if the processor was found; otherwise, false.</returns>
        public bool TryGetProcessor(Guid processorId, out BuffProcessor<T> buffProcessor) {
            var index = IndexOf(processorId);
            buffProcessor = index >= 0 ? entries[index].processor : null;
            return index >= 0;
        }

        /// <inheritdoc/>
        public void NotifyChanged() {
            InvalidateCache();
            Changed?.Invoke();
        }

        /// <summary>
        /// Invalidate the cached result without raising <see cref="Changed"/>.
        /// Safe to call from Unity serialization callbacks.
        /// </summary>
        internal void InvalidateCache() {
            unchecked {
                version++;
            }

            isCacheValid = false;
        }

        private int IndexOf(Guid processorId) {
            var current = entries;
            for (var i = 0; i < current.Length; i++) {
                if (current[i].id == processorId) {
                    return i;
                }
            }

            return -1;
        }

        private void Insert(Entry entry) {
            var current = entries;

            // Stable: insert after all entries with the same or lower order.
            var index = current.Length;
            while (index > 0 && current[index - 1].order > entry.order) {
                index--;
            }

            var newEntries = new Entry[current.Length + 1];
            Array.Copy(current, 0, newEntries, 0, index);
            newEntries[index] = entry;
            Array.Copy(current, index, newEntries, index + 1, current.Length - index);
            entries = newEntries;
        }

        private int RemoveWhere(Predicate<Entry> match) {
            var current = entries;
            var kept = new Entry[current.Length];
            var keptCount = 0;

            for (var i = 0; i < current.Length; i++) {
                if (match(current[i])) {
                    current[i].processor.Changed -= processorChangedHandler;
                } else {
                    kept[keptCount++] = current[i];
                }
            }

            var removedCount = current.Length - keptCount;
            if (removedCount == 0) {
                return 0;
            }

            Array.Resize(ref kept, keptCount);
            entries = kept;
            NotifyChanged();
            return removedCount;
        }
    }
}
