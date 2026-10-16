using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Non-generic view of a <see cref="Buff{T}"/>.<br/><br/>
    /// Use it when you need to store or manage buffs of different value types together.
    /// Prefer <see cref="Buff{T}.Calculate"/> over <see cref="CalculateObject"/> when the type is known,
    /// since the latter boxes value types.
    /// </summary>
    public interface IBuff {
        /// <summary>
        /// Is called when the result of the buff may have changed:
        /// processors were added, removed or replaced, a processor notified about its change,
        /// the original value was set, or <see cref="NotifyChanged"/> was called.
        /// </summary>
        event Action Changed;

        /// <summary>
        /// The type of buff value.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Number of registered processors.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Whether the result of the calculation is cached until the next change.<br/><br/>
        /// Enable it only if the original value and all processors are deterministic,
        /// or if you call <see cref="NotifyChanged"/> every time their external state changes.
        /// </summary>
        bool CacheEnabled { get; set; }

        /// <summary>
        /// Apply all processors to the original value and return the boxed result.
        /// </summary>
        /// <returns>Object that should be cast to <see cref="ValueType"/>.</returns>
        object CalculateObject();

        /// <summary>
        /// Determines whether the buff contains the specified processor ID.
        /// </summary>
        /// <param name="processorId">Registered GUID.</param>
        /// <returns>true if the buff contains a processor with the specified GUID; otherwise, false.</returns>
        bool Contains(Guid processorId);

        /// <summary>
        /// Remove value processor by GUID.
        /// </summary>
        /// <param name="processorId">Registered GUID.</param>
        /// <returns>true if the processor was found and removed; otherwise, false.</returns>
        bool Remove(Guid processorId);

        /// <summary>
        /// Remove all processors registered with the specified source.
        /// </summary>
        /// <param name="source">Source object passed when adding processors.</param>
        /// <returns>Number of removed processors.</returns>
        int RemoveBySource(object source);

        /// <summary>
        /// Remove all processors.
        /// </summary>
        void Clear();

        /// <summary>
        /// Invalidates the cached result and raises <see cref="Changed"/>.<br/><br/>
        /// Call it when an external state used by the original value or by processors changes.
        /// </summary>
        void NotifyChanged();
    }
}
