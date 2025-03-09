[![GitHub Release](https://img.shields.io/github/v/release/Demexis/Unity-Buffs.svg)](https://github.com/Demexis/Unity-Buffs/releases/latest)
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
# Unity-Buffs

<table>
  <tr></tr>
  <tr>
    <td colspan="3">Языки Readme:</td>
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

Простая реализация системы бафов для использования в скриптах. Её преимуществом является то, что вы можете использовать любые C#-типы в качестве значения бафа и его обработчиков.

В дополнение к уже реализованным обработчикам для значений бафов, вы можете создать свои собственные.

## Table of Contents
- [Настройка](#setup)
- [Использование](#usage)
- [Примеры](#examples)
- [Подсказки](#hints)

<br>

## Настройка

### Требования

* Unity 2021.3 или позднее

### Установка

Используйте __ОДИН__ из двух вариантов:

#### а) Менеджер пакетов (Рекомендуется)
1. Откройте Package Manager из Window > Package Manager.
2. Нажмите на кнопку "+" > Add package from git URL.
3. Введите в поле этот URL:
```
https://github.com/Demexis/Unity-Buffs.git
```

Альтернативно, можете открыть *Packages/manifest.json* и добавить туда новый блок с зависимостями:

```json
{
    "dependencies": {
        "com.demegraunt.buffs": "https://github.com/Demexis/Unity-Buffs.git"
    }
}
```

#### б) Юнити-пакет
Скачайте юнити-пакет из [последнего релиза](../../releases).

## Использование
__1) Создайте баф с желаемым типом и привяжите оригинальное значение:__
```cs
// Эта переменная может менять своё значение!
public float originalValue = 1f;
...
var buff = new Buff<float>(() => originalValue);
```

__2) Добавьте обработчики которые будут последовательно (в порядке добавления) изменять оригинальное значение:__
```cs
private readonly Guid processorId = Guid.NewGuid();
...
buff.Add(processorId, new BuffProcessor<float>(originalValue => originalValue * 4f));
```
  
__3) Вычислите значение бафа:__
```cs
var resultValue = buff.Calculate();
Debug.Log(resultValue); // выводит 4
```
  
__4) Замените обработчик:__
```cs
buff.Replace(processorId, new BuffProcessor<float>(originalValue => originalValue + 20f));

resultValue = buff.Calculate();
Debug.Log(resultValue); // выводит 21
```

__5) Удалите обработчик:__
```cs
buff.Remove(processorId);

resultValue = buff.Calculate();
Debug.Log(resultValue); // выводит 1 - это изначальное значение
```

## Примеры
__1: Механика спринта с использованием бафа__

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

__2: Один баф влияет на другой__
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

## Подсказки
* Пакет содержит уже готовые обработчики, такие как: `FloatBuffAdder`, `FloatBuffMultiplier`, `IntBuffAdder`, `IntBuffMultiplier`. Вы можете определить свои собственные типы обработчиков, отнаследовавшись от `BuffProcessor<T>`.

* Если вы закешируете созданный экземпляр обработчика, сможете обратиться к свойству `ProcessCallback` для изменения логики обработки, не прибегая к созданию совершенно нового экземпляра:
```cs
var buff = new Buff<float>(1f);
var processor = new BuffProcessor(value => value + 5f);

buff.Add(Guid.NewGuid(), processor);
Debug.Log(buff.Calculate()); // выводит 6

processor.ProcessCallback = value => value / 2f;
Debug.Log(buff.Calculate()); // выводит 0.5
```
