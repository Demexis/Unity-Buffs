using System;
using System.Collections.Generic;
using UnityEngine;

namespace Demegraunt.Framework {
    /// <summary>
    /// Basic non-generic implementation of buff with original value callback and value processors.
    /// </summary>
    public class BaseBuff {
        /// <summary>
        /// Is called when the list of processors changes.
        /// </summary>
        public event Action ProcessorsChanged;
        
        /// <summary>
        /// The type of buff value.
        /// </summary>
        public readonly Type buffType;
        
        /// <summary>
        /// List of all value processors.
        /// </summary>
        private readonly Dictionary<Guid, BaseBuffProcessor> buffProcessors = new();
        
        /// <summary>
        /// Callback to get original value for further processing.
        /// </summary>
        private readonly Func<object> getOriginalValue;

        protected BaseBuff(Type buffType, Func<object> getOriginalValue) {
            this.buffType = buffType;
            this.getOriginalValue = getOriginalValue;
        }

        /// <summary>
        /// Base implementation to apply all processors to the original value and return result.<br/><br/>
        /// Processors are used in the order they were added. 
        /// </summary>
        /// <returns>Object that should be casted in order to use.</returns>
        public object CalculateObject() {
            var originalValue = getOriginalValue.Invoke();
            
            foreach (var (_, buff) in buffProcessors) {
                originalValue = buff.ProcessObject(originalValue);
            }
            
            return originalValue;
        }

        /// <summary>
        /// Base implementation to register new value processor.<br/>
        /// Logs an error if the types are mismatched.<br/>
        /// You can't add the same processor ID twice. Generate new GUID or replace old processor by using <see cref="ReplaceBase"/>.
        /// </summary>
        /// <param name="processorId">Unique GUID. You can use <see cref="Guid.NewGuid()"/>.</param>
        /// <param name="buffProcessor">Buff processor implementation.</param>
        public void AddBase(Guid processorId, BaseBuffProcessor buffProcessor) {
            if (buffType != buffProcessor.processorType) {
                Debug.LogError($"Type mismatch between buff ({buffType}) and processor ({buffProcessor.processorType}).");
                return;
            }
            
            buffProcessors.Add(processorId, buffProcessor);
            ProcessorsChanged?.Invoke();
        }

        /// <summary>
        /// Base implementation to replace or add new value processor.<br/>
        /// Logs an error if the types are mismatched.
        /// </summary>
        /// <param name="processorId">GUID.</param>
        /// <param name="buffProcessor">New buff processor implementation.</param>
        public void ReplaceBase(Guid processorId, BaseBuffProcessor buffProcessor) {
            if (!buffProcessors.ContainsKey(processorId)) {
                AddBase(processorId, buffProcessor);
                return;
            }
            
            if (buffType != buffProcessor.processorType) {
                Debug.LogError($"Type mismatch between buff ({buffType}) and processor ({buffProcessor.processorType}).");
                return;
            }
            
            buffProcessors[processorId] = buffProcessor;
            ProcessorsChanged?.Invoke();
        }

        /// <summary>
        /// Remove value processor by GUID.
        /// </summary>
        /// <param name="processorId">Registered GUID.</param>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false.<br/><br/>
        /// This method returns false if key is not found in the dictionary.
        /// </returns>
        public bool Remove(Guid processorId) {
            if (!buffProcessors.ContainsKey(processorId)) {
                return false;
            }
            
            buffProcessors.Remove(processorId);
            ProcessorsChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// Determines whether the buff contains the specified processor ID.
        /// </summary>
        /// <param name="processorId">Registered GUID.</param>
        /// <returns>true if the buff contains a processor with the specified GUID; otherwise, false.</returns>
        public bool Contains(Guid processorId) {
            return buffProcessors.ContainsKey(processorId);
        }

        /// <summary>
        /// Raises an event intended for changes in processors.<br/><br/>
        /// You may want to use it when you change the state of the processor's instance.
        /// </summary>
        public void NotifyProcessorsChanged() {
            ProcessorsChanged?.Invoke();
        }
    }
}