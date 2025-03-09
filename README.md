
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
- [Examples](#examples)
- [Hints](#hints)

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
__1) Create a buff with the desired type and bind an original value:__
```cs
// This variable can change its value!
public float originalValue = 1f;
...
var buff = new Buff<float>(() => originalValue);
```

__2) Add processor(-s) that will sequentially transform the original value:__
```cs
private readonly Guid processorId = Guid.NewGuid();
...
buff.Add(processorId, new BuffProcessor<float>(value => value * 4f));
```
  
__3) Calculate the buff value:__
```cs
var resultValue = buff.Calculate();
Debug.Log(resultValue); // prints 4
```
  
__4) Replace the processor:__
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

## Examples
__1: A sprint mechanic using a buff__

```cs
using System;
using Demegraunt.Framework;
using UnityEngine;

public sealed class MovementWithBuff : MonoBehaviour {
    [field: SerializeField] public float Speed { get; set; } = 1f;
    [field: SerializeField] public float SprintMultiplier { get; set; } = 4f;
    
    private Buff<float> SpeedBuff { get; set; }
    private readonly Guid sprintProcessorId = Guid.NewGuid();

    private void Awake() {
        SpeedBuff = new Buff<float>(() => Speed);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            ActivateSprint();
        } else if (Input.GetKeyUp(KeyCode.LeftShift)) {
            DeactivateSprint();
        }

        transform.position += Vector3.right * Time.deltaTime * SpeedBuff.Calculate();
    }

    public void ActivateSprint() {
        if (SpeedBuff.Contains(sprintProcessorId)) {
            return;
        }
        
        SpeedBuff.Add(sprintProcessorId, new BuffProcessor<float>(originalValue => originalValue * SprintMultiplier));
    }

    public void DeactivateSprint() {
        SpeedBuff.Remove(sprintProcessorId);
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
    private readonly Guid slowdownProcessorId = Guid.NewGuid();

    private void Awake() {
        WalkBuff = new Buff<float>(() => Speed);
        RunBuff = new Buff<float>(() => WalkBuff.Calculate() * SprintMultiplier);
    }

    private void Start() {
        Slowdown();
    }

    private void Update() {
        // always running
        transform.position += Vector3.right * Time.deltaTime * RunBuff.Calculate();
    }

    public void Slowdown() {
        WalkBuff.Add(slowdownProcessorId, new FloatBuffMultiplier(0.1f));
    }
}
```

## Hints
* The package contains custom processors such as: `FloatBuffAdder`, `FloatBuffMultiplier`, `IntBuffAdder`, `IntBuffMultiplier`. You can define your own types of processors by inheriting from `BuffProcessor<T>`.

* If you cache the created processor instance, you can access the `ProcessCallback` property to change the processing logic, instead of creating a completely new instance:
```cs
var buff = new Buff<float>(1f);
var processor = new BuffProcessor<float>(value => value + 5f);

buff.Add(Guid.NewGuid(), processor);
Debug.Log(buff.Calculate()); // prints 6

processor.ProcessCallback = value => value / 2f;
Debug.Log(buff.Calculate()); // prints 0.5
```

