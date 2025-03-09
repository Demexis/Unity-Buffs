using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Generic version of <see cref="BaseBuff"/>.
    /// </summary>
    /// <typeparam name="T">The type of buff value.</typeparam>
    public class Buff<T> : BaseBuff {
        public Buff(T originalValue) : base(typeof(T), () => originalValue) { }
        public Buff(Func<T> getOriginalValue) : base(typeof(T), () => getOriginalValue.Invoke()) { }

        /// <summary>
        /// Apply all processors to the original value and return result.<br/><br/>
        /// Processors are used in the order they were added. 
        /// </summary>
        /// <returns>Result value.</returns>
        public T Calculate() {
            return (T)CalculateObject();
        }

        /// <summary>
        /// Register new value processor.<br/>
        /// Logs an error if the types are mismatched.<br/>
        /// You can't add the same processor ID twice. Generate new GUID or replace old processor by using <see cref="ReplaceBaseProcessor"/>.
        /// </summary>
        /// <param name="processorId">Unique GUID. You can use <see cref="Guid.NewGuid()"/>.</param>
        /// <param name="buffProcessor">Buff processor implementation.</param>
        public void Add(Guid processorId, BuffProcessor<T> buffProcessor) {
            AddBase(processorId, buffProcessor);
        }

        /// <summary>
        /// Replace or add new value processor.<br/>
        /// Logs an error if the types are mismatched.
        /// </summary>
        /// <param name="processorId">GUID.</param>
        /// <param name="buffProcessor">New buff processor implementation.</param>
        public void Replace(Guid processorId, BuffProcessor<T> buffProcessor) {
            ReplaceBase(processorId, buffProcessor);
        }
    }
}