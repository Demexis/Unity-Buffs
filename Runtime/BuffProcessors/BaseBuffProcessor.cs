using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Basic non-generic abstraction for value processing.
    /// </summary>
    public abstract class BaseBuffProcessor {
        /// <summary>
        /// The type of buff value.
        /// </summary>
        public readonly Type processorType;
        
        protected BaseBuffProcessor(Type processorType) {
            this.processorType = processorType;
        }

        public abstract object ProcessObject(object value);
    }
}