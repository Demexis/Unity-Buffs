# Changelog

## [2.0.0]

Serialized data of `BuffField<T>`, `FloatBuffField` and `IntBuffField` is fully compatible with previous versions.
The assembly name (`Buffs`) and the namespace (`Demegraunt.Framework`) are unchanged.

### Fixed
- Processors were not guaranteed to be applied in the order they were added: after removing a processor,
  the next added one could take its place in the middle of the chain.
- Broken references in XML documentation.

### Added
- Processing order: `Add(processor, order)` and `BuffOrder` constants. Processors with the same order are applied in the order they were added.
  Built-in adders use `BuffOrder.Additive`, multipliers use `BuffOrder.Multiplicative`, so `(base + adders) * multipliers` is calculated regardless of the order of `Add` calls.
- `BuffHandle` returned by `Add`/`Replace`: removes the processor on `Dispose()`; `Add` without a GUID.
- Processor sources: `Add(..., source: owner)` and `RemoveBySource(owner)`.
- `RemoveProcessor`, `Clear`, `Count`, `TryGetProcessor`.
- Optional result caching: `CacheEnabled`.
- `Buff<T>.OriginalValue`, `Buff<T>.SetOriginalValue`.
- `BuffProcessor<T>.Changed`, `NotifyChanged`, overridable `Process` and `DefaultOrder`.
- `IntBuffMultiplier` with a fractional multiplier and `BuffRounding`.
- Public `BuffField<T>.OriginalValue` getter, `BuffField<T>.SetOriginalValue`, parameterless constructors.
- Property drawer: buff-fields are shown in the inspector as a single field.
- EditMode tests.

### Changed
- `Calculate()` no longer allocates memory (no boxing of value types).
- The buff may be modified during calculation (from a processor or an event handler); changes take effect on the next calculation.
- Recursive calculation of the same buff throws `InvalidOperationException` instead of `StackOverflowException`.

### Breaking changes
- `BaseBuff` is replaced by the `IBuff` interface. `CalculateObject()` is kept there.
  `TryAddBase`, `AddBase`, `ReplaceBase` are removed: use the generic `TryAdd`, `Add`, `Replace`.
- `BaseBuffProcessor` and `BaseBuffProcessor.processorType` are removed; type checks are done by the compiler. Use `BuffProcessor<T>`.
- `BaseBuff.buffType` → `IBuff.ValueType`.
- `ProcessorsChanged` → `Changed`; `NotifyProcessorsChanged()` → `NotifyChanged()`.
- `Add` with a duplicate ID throws `ArgumentException`; null processors throw `ArgumentNullException`
  (previously a type mismatch only logged an error).
- `Add` and `Replace` return `BuffHandle` instead of `void`.
- `FloatBuffAdder`, `IntBuffAdder` (order 100) are now applied before `FloatBuffMultiplier`, `IntBuffMultiplier` (order 200) and after custom processors (order 0).
  Pass an explicit order to `Add` to get a different sequence.
- The `Runtime/BuffFIelds` folder is renamed to `Runtime/BuffFields`.

## [1.2.0]
- Previous release.
