[![GitHub Release](https://img.shields.io/github/v/release/Demexis/Unity-Buffs.svg)](https://github.com/Demexis/Unity-Buffs/releases/latest)
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
# Unity-Buffs

<table>
  <tr></tr>
  <tr>
    <td colspan="3">Readme Languages:</td>
  </tr>
  <tr></tr>
  <tr>
    <td nowrap width="100">
      <a href="https://github.com/Demexis/Unity-Buffs">
        <span>English</span>
      </a>  
    </td>
    <td nowrap width="100">
      <a href="https://github.com/Demexis/Unity-Buffs/blob/main/README-RU.md">
        <span>Русский</span>
      </a>  
    </td>
  </tr>
</table>

A simple implementation of a buff-system for use in scripts. Its advantage is that you can use any C#-types as the buff value and value-processors.

In addition to the default processors for the buff values, you can create your own implementations.

## Table of Contents
- [Setup](#setup)
- [Usage](#usage)
- [Processing order](#processing-order)
- [Handles and sources](#handles-and-sources)
- [Caching and events](#caching-and-events)
- [Buff-fields and the inspector](#buff-fields-and-the-inspector)
- [Examples](#examples)
- [Hints](#hints)
- [Tests](#tests)
- [Migration from 1.x](#migration-from-1x)

<br>

## Setup

### Requirements

* Unity 2021.3 or later

### Installation

Use __ONE__ of two options:

#### a) Package Manager (Recommended)
1. Open Package Manager from Window > Package Manager.
2. Click the "+" button > Add package from git URL.
3. Enter the following URL:
```
https://github.com/Demexis/Unity-Buffs.git
```

Alternatively, open *Packages/manifest.json* and add the following to the dependencies block:

```json
{
    "dependencies": {
        "com.demegraunt.buffs": "https://github.com/Demexis/Unity-Buffs.git"
    }
}
```

#### b) Unity Package
Download a unity package from [the latest release](../../releases).

## Usage
All types are in the `Demegraunt.Framework` namespace.

__1) Create a buff with the desired type and bind an original value:__
```cs
// This variable can change its value!
public float originalValue = 1f;
...
var buff = new Buff<float>(() => originalValue);

// Or use a constant that can be changed later with buff.SetOriginalValue(...)
var constantBuff = new Buff<float>(1f);
```

__2) Add processor(-s) that will sequentially transform the original value:__
```cs
private readonly Guid processorId = Guid.NewGuid();
...
buff.Add(processorId, new BuffProcessor<float>(value => value * 4f));

// Shorter form with a callback:
// buff.Add(processorId, value => value * 4f);
```

__3) Calculate the buff value:__
```cs
var resultValue = buff.Calculate();
Debug.Log(resultValue); // prints 4
```

__4) Replace the processor (its position in the chain is kept):__
```cs
buff.Replace(processorId, new BuffProcessor<float>(value => value + 20f));

resultValue = buff.Calculate();
Debug.Log(resultValue); // prints 21
```

__5) Remove the processor:__
```cs
buff.Remove(processorId);

resultValue = buff.Calculate();
Debug.Log(resultValue); // prints 1 - the original value
```

`Add` throws `ArgumentException` if the ID is already registered (use `TryAdd` to get `false` instead) and `ArgumentNullException` for a null processor.

## Processing order
Every processor is registered with an integer __order__. Processors with a lower order are applied first; processors with the same order are applied in the order they were added (this is kept after removals and replacements).

If the order is not specified, the processor's `DefaultOrder` is used:

| Constant | Value | Used by default for |
|---|---|---|
| `BuffOrder.Default` | 0 | `BuffProcessor<T>`, callbacks |
| `BuffOrder.Additive` | 100 | `FloatBuffAdder`, `IntBuffAdder` |
| `BuffOrder.Multiplicative` | 200 | `FloatBuffMultiplier`, `IntBuffMultiplier` |
| `BuffOrder.Final` | 1000 | — (e.g. for clamping) |

So the common formula `(base + bonuses) * multipliers` works regardless of the order of `Add` calls:
```cs
var damage = new Buff<float>(10f);
damage.Add(new FloatBuffMultiplier(2f));
damage.Add(new FloatBuffAdder(5f));
Debug.Log(damage.Calculate()); // prints 30 = (10 + 5) * 2

// Explicit order. Any int can be used, e.g. BuffOrder.Additive + 1.
damage.Add(value => Mathf.Min(value, 25f), BuffOrder.Final);
Debug.Log(damage.Calculate()); // prints 25
```

## Handles and sources
`Add` and `Replace` return a `BuffHandle`. Disposing it removes the processor, so you don't have to store a `Guid`:
```cs
var handle = speedBuff.Add(new FloatBuffMultiplier(0.5f));
...
handle.Dispose();       // or handle.Remove()
Debug.Log(handle.IsActive); // false
```

A processor can be registered with a __source__ (any object, e.g. an item or an effect). All processors of the source can be removed at once:
```cs
attack.Add(new IntBuffAdder(5), source: sword);
attack.Add(new IntBuffMultiplier(1.5f), source: sword);
...
attack.RemoveBySource(sword); // returns 2
```

Other methods: `RemoveProcessor(processor)` removes all registrations of the processor instance, `Clear()`, `Contains(id)`, `TryGetProcessor(id, out processor)`, `Count`.

## Caching and events
`Calculate()` does not allocate memory, but by default it recalculates the whole chain on every call. If the original value and the processors only change through the buff API, enable caching:
```cs
buff.CacheEnabled = true;
```

The cache is reset when processors are added, removed or replaced, when the original value is set with `SetOriginalValue`, when a processor raises its `Changed` event (e.g. `ProcessCallback` is set), or when `buff.NotifyChanged()` is called.

> ⚠️ The buff can't detect changes of external variables used inside callbacks (`new Buff<float>(() => speed)`, `value => value * multiplier`). With caching enabled, call `buff.NotifyChanged()` (or `processor.NotifyChanged()`) when they change, or keep caching disabled.

The `Changed` event is raised on every change listed above, so you can update the UI only when needed:
```cs
healthBuff.Changed += () => healthLabel.text = healthBuff.Calculate().ToString();
```

The buff may be modified during the calculation (from a processor or a `Changed` handler); changes take effect on the next calculation. A recursive calculation of the same buff (e.g. its original value callback calls its own `Calculate()`) throws `InvalidOperationException`.

## Buff-fields and the inspector
`BuffField<T>` combines a serialized original value and a buff. It works with any serializable type; `FloatBuffField` and `IntBuffField` are short aliases.
```cs
public sealed class Player : MonoBehaviour {
    [SerializeField] private FloatBuffField speed = new(5f);
    [SerializeField] private BuffField<Vector3> jumpForce = new(Vector3.up);

    private void Start() {
        speed.Buff.Add(new FloatBuffMultiplier(2f));
        Debug.Log(speed.Calculate());       // prints 10
        Debug.Log(speed.OriginalValue);     // prints 5

        speed.SetOriginalValue(6f);         // notifies the buff
    }
}
```

In the inspector a buff-field is shown as a regular field of its original value (without a foldout).
In Unity versions before 2023.2 the drawer is applied only to `BuffField<T>`, `FloatBuffField` and `IntBuffField`. For your own non-generic subclasses add a drawer in an Editor folder:
```cs
[CustomPropertyDrawer(typeof(MyBuffField))]
public sealed class MyBuffFieldDrawer : BuffFieldDrawer { }
```

## Examples
__1: A sprint mechanic using a buff__

```cs
using Demegraunt.Framework;
using UnityEngine;

public sealed class MovementWithBuff : MonoBehaviour {
    [SerializeField] private FloatBuffField speed = new(1f);
    [SerializeField] private float sprintMultiplier = 4f;

    private BuffHandle sprint;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            ActivateSprint();
        } else if (Input.GetKeyUp(KeyCode.LeftShift)) {
            DeactivateSprint();
        }

        transform.position += Vector3.right * (Time.deltaTime * speed.Calculate());
    }

    public void ActivateSprint() {
        if (sprint.IsActive) {
            return;
        }

        // The multiplier is read on every calculation, so it can be tweaked in the inspector.
        sprint = speed.Buff.Add(new FloatBuffMultiplier(() => sprintMultiplier));
    }

    public void DeactivateSprint() {
        sprint.Dispose();
    }
}
```

__2: One buff affects the other__
```cs
public sealed class WalkRunBuffs : MonoBehaviour {
    [field: SerializeField] public float Speed { get; set; } = 1f;
    [field: SerializeField] public float SprintMultiplier { get; set; } = 4f;

    private Buff<float> WalkBuff { get; set; }
    private Buff<float> RunBuff { get; set; }

    private void Awake() {
        WalkBuff = new Buff<float>(() => Speed);
        RunBuff = new Buff<float>(() => WalkBuff.Calculate() * SprintMultiplier);
    }

    private void Start() {
        Slowdown();
    }

    private void Update() {
        // always running
        transform.position += Vector3.right * (Time.deltaTime * RunBuff.Calculate());
    }

    public void Slowdown() {
        WalkBuff.Add(new FloatBuffMultiplier(0.1f));
    }
}
```

__3: Equipment with temporary effects__
```cs
public sealed class Character : MonoBehaviour {
    [SerializeField] private IntBuffField attack = new(10);

    public void Equip(Item item) {
        attack.Buff.Add(new IntBuffAdder(item.AttackBonus), source: item);
        attack.Buff.Add(new IntBuffMultiplier(item.AttackMultiplier, BuffRounding.Floor), source: item);
    }

    public void Unequip(Item item) {
        attack.Buff.RemoveBySource(item);
    }

    public IEnumerator Rage(float duration) {
        using (attack.Buff.Add(new IntBuffMultiplier(2))) {
            yield return new WaitForSeconds(duration);
        } // the processor is removed here
    }
}
```

## Hints
* The package contains processors `FloatBuffAdder`, `FloatBuffMultiplier`, `IntBuffAdder`, `IntBuffMultiplier`. Each of them accepts a constant or a `Func<>` that is read on every calculation.
  `IntBuffMultiplier` also accepts a fractional multiplier with a rounding mode: `new IntBuffMultiplier(1.5f, BuffRounding.Nearest)` (`Nearest` rounds midpoints away from zero; `Floor`, `Ceiling`, `Truncate` are also available).

* You can define your own types of processors by inheriting from `BuffProcessor<T>`. Pass a callback to the base constructor or override `Process`; override `DefaultOrder` to change the default stage:
```cs
public sealed class ClampProcessor : BuffProcessor<float> {
    private readonly float min, max;
    public override int DefaultOrder => BuffOrder.Final;

    public ClampProcessor(float min, float max) {
        this.min = min;
        this.max = max;
    }

    public override float Process(float value) => Mathf.Clamp(value, min, max);
}
```

* If you cache the created processor instance, you can change the `ProcessCallback` property instead of creating a new instance. The buffs that contain the processor are notified automatically:
```cs
var buff = new Buff<float>(1f);
var processor = new BuffProcessor<float>(value => value + 5f);

buff.Add(processor);
Debug.Log(buff.Calculate()); // prints 6

processor.ProcessCallback = value => value / 2f;
Debug.Log(buff.Calculate()); // prints 0.5
```

* Use the `IBuff` interface to store buffs of different value types together (e.g. to call `RemoveBySource` for all stats of a character). `IBuff.CalculateObject()` returns a boxed value; prefer `Buff<T>.Calculate()` when the type is known.

* The buff is not thread-safe. Use it from the main thread.

## Tests
The package contains EditMode tests. To run them, add the package to `testables` in *Packages/manifest.json* and open Window > General > Test Runner:
```json
{
    "testables": [
        "com.demegraunt.buffs"
    ]
}
```

## Migration from 1.x
Serialized data of buff-fields is kept, the namespace and the assembly name are unchanged. Code changes:

| 1.x | 2.0 |
|---|---|
| `BaseBuff` | `IBuff` |
| `BaseBuff.buffType` | `IBuff.ValueType` |
| `AddBase`, `TryAddBase`, `ReplaceBase` | `Add`, `TryAdd`, `Replace` of `Buff<T>` |
| `BaseBuffProcessor` | `BuffProcessor<T>` |
| `ProcessorsChanged` | `Changed` |
| `NotifyProcessorsChanged()` | `NotifyChanged()` |
| Type mismatch logged an error | Types are checked by the compiler |
| `Add` with a duplicate ID threw from `Dictionary` | `Add` throws `ArgumentException`, `TryAdd` returns `false` |

⚠️ Behaviour change: built-in adders are now applied before built-in multipliers, and custom processors (order 0) before both of them. If your code relied on the order of `Add` calls with mixed processor types, pass the order explicitly: `buff.Add(processor, order)`.

See [CHANGELOG.md](CHANGELOG.md) for the full list of changes.
