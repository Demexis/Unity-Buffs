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

## Содержание
- [Настройка](#настройка)
- [Использование](#использование)
- [Порядок обработки](#порядок-обработки)
- [Дескрипторы и источники](#дескрипторы-и-источники)
- [Кэширование и события](#кэширование-и-события)
- [Баф-поля и инспектор](#баф-поля-и-инспектор)
- [Примеры](#примеры)
- [Подсказки](#подсказки)
- [Тесты](#тесты)
- [Переход с 1.x](#переход-с-1x)

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

Альтернативно, можете открыть *Packages/manifest.json* и добавить туда новую строку в блок "dependencies":

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
Все типы находятся в пространстве имён `Demegraunt.Framework`.

__1) Создайте баф с желаемым типом и привяжите оригинальное значение:__
```cs
// Эта переменная может менять своё значение!
public float originalValue = 1f;
...
var buff = new Buff<float>(() => originalValue);

// Или используйте константу, которую позже можно изменить через buff.SetOriginalValue(...)
var constantBuff = new Buff<float>(1f);
```

__2) Добавьте обработчики, которые будут последовательно изменять оригинальное значение:__
```cs
private readonly Guid processorId = Guid.NewGuid();
...
buff.Add(processorId, new BuffProcessor<float>(value => value * 4f));

// Короткая форма с колбэком:
// buff.Add(processorId, value => value * 4f);
```

__3) Вычислите значение бафа:__
```cs
var resultValue = buff.Calculate();
Debug.Log(resultValue); // выводит 4
```

__4) Замените обработчик (его место в цепочке сохраняется):__
```cs
buff.Replace(processorId, new BuffProcessor<float>(value => value + 20f));

resultValue = buff.Calculate();
Debug.Log(resultValue); // выводит 21
```

__5) Удалите обработчик:__
```cs
buff.Remove(processorId);

resultValue = buff.Calculate();
Debug.Log(resultValue); // выводит 1 - это изначальное значение
```

`Add` бросает `ArgumentException`, если такой ID уже зарегистрирован (используйте `TryAdd`, чтобы получить `false`), и `ArgumentNullException` для null-обработчика.

## Порядок обработки
Каждый обработчик регистрируется с целочисленным __порядком__ (order). Обработчики с меньшим порядком применяются раньше; обработчики с одинаковым порядком применяются в порядке добавления (это сохраняется и после удалений и замен).

Если порядок не указан, используется `DefaultOrder` обработчика:

| Константа | Значение | По умолчанию у |
|---|---|---|
| `BuffOrder.Default` | 0 | `BuffProcessor<T>`, колбэков |
| `BuffOrder.Additive` | 100 | `FloatBuffAdder`, `IntBuffAdder` |
| `BuffOrder.Multiplicative` | 200 | `FloatBuffMultiplier`, `IntBuffMultiplier` |
| `BuffOrder.Final` | 1000 | — (например, для ограничения значения) |

Поэтому привычная формула `(база + бонусы) * множители` работает независимо от порядка вызовов `Add`:
```cs
var damage = new Buff<float>(10f);
damage.Add(new FloatBuffMultiplier(2f));
damage.Add(new FloatBuffAdder(5f));
Debug.Log(damage.Calculate()); // выводит 30 = (10 + 5) * 2

// Явный порядок. Можно использовать любое int-значение, например BuffOrder.Additive + 1.
damage.Add(value => Mathf.Min(value, 25f), BuffOrder.Final);
Debug.Log(damage.Calculate()); // выводит 25
```

## Дескрипторы и источники
`Add` и `Replace` возвращают `BuffHandle`. Вызов `Dispose()` у него удаляет обработчик, поэтому хранить `Guid` не обязательно:
```cs
var handle = speedBuff.Add(new FloatBuffMultiplier(0.5f));
...
handle.Dispose();           // или handle.Remove()
Debug.Log(handle.IsActive); // false
```

Обработчик можно зарегистрировать с __источником__ (любой объект, например предмет или эффект). Все обработчики источника удаляются одним вызовом:
```cs
attack.Add(new IntBuffAdder(5), source: sword);
attack.Add(new IntBuffMultiplier(1.5f), source: sword);
...
attack.RemoveBySource(sword); // возвращает 2
```

Другие методы: `RemoveProcessor(processor)` удаляет все регистрации экземпляра обработчика, `Clear()`, `Contains(id)`, `TryGetProcessor(id, out processor)`, `Count`.

## Кэширование и события
`Calculate()` не выделяет память, но по умолчанию пересчитывает всю цепочку при каждом вызове. Если оригинальное значение и обработчики меняются только через API бафа, включите кэширование:
```cs
buff.CacheEnabled = true;
```

Кэш сбрасывается, когда обработчики добавляются, удаляются или заменяются, когда оригинальное значение задаётся через `SetOriginalValue`, когда обработчик вызывает своё событие `Changed` (например, при установке `ProcessCallback`), или при вызове `buff.NotifyChanged()`.

> ⚠️ Баф не может отследить изменения внешних переменных, используемых внутри колбэков (`new Buff<float>(() => speed)`, `value => value * multiplier`). При включённом кэше вызывайте `buff.NotifyChanged()` (или `processor.NotifyChanged()`), когда они меняются, либо не включайте кэширование.

Событие `Changed` вызывается при каждом из перечисленных выше изменений, поэтому, например, интерфейс можно обновлять только при необходимости:
```cs
healthBuff.Changed += () => healthLabel.text = healthBuff.Calculate().ToString();
```

Баф можно изменять во время вычисления (из обработчика или подписчика `Changed`); изменения вступают в силу со следующего вычисления. Рекурсивное вычисление того же бафа (например, колбэк оригинального значения вызывает его же `Calculate()`) бросает `InvalidOperationException`.

## Баф-поля и инспектор
`BuffField<T>` объединяет сериализуемое оригинальное значение и баф. Работает с любым сериализуемым типом; `FloatBuffField` и `IntBuffField` — короткие псевдонимы.
```cs
public sealed class Player : MonoBehaviour {
    [SerializeField] private FloatBuffField speed = new(5f);
    [SerializeField] private BuffField<Vector3> jumpForce = new(Vector3.up);

    private void Start() {
        speed.Buff.Add(new FloatBuffMultiplier(2f));
        Debug.Log(speed.Calculate());       // выводит 10
        Debug.Log(speed.OriginalValue);     // выводит 5

        speed.SetOriginalValue(6f);         // уведомляет баф
    }
}
```

В инспекторе баф-поле отображается как обычное поле своего оригинального значения (без раскрывающегося списка).
В версиях Unity до 2023.2 отрисовщик применяется только к `BuffField<T>`, `FloatBuffField` и `IntBuffField`. Для своих не-generic наследников добавьте отрисовщик в папку Editor:
```cs
[CustomPropertyDrawer(typeof(MyBuffField))]
public sealed class MyBuffFieldDrawer : BuffFieldDrawer { }
```

## Примеры
__1: Механика спринта с использованием бафа__

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

        // Множитель читается при каждом вычислении, поэтому его можно менять в инспекторе.
        sprint = speed.Buff.Add(new FloatBuffMultiplier(() => sprintMultiplier));
    }

    public void DeactivateSprint() {
        sprint.Dispose();
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

    private void Awake() {
        WalkBuff = new Buff<float>(() => Speed);
        RunBuff = new Buff<float>(() => WalkBuff.Calculate() * SprintMultiplier);
    }

    private void Start() {
        Slowdown();
    }

    private void Update() {
        // всегда бежим
        transform.position += Vector3.right * (Time.deltaTime * RunBuff.Calculate());
    }

    public void Slowdown() {
        WalkBuff.Add(new FloatBuffMultiplier(0.1f));
    }
}
```

__3: Экипировка и временные эффекты__
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
        } // здесь обработчик удаляется
    }
}
```

## Подсказки
* В пакете есть обработчики `FloatBuffAdder`, `FloatBuffMultiplier`, `IntBuffAdder`, `IntBuffMultiplier`. Каждый из них принимает константу или `Func<>`, который читается при каждом вычислении.
  `IntBuffMultiplier` также принимает дробный множитель с режимом округления: `new IntBuffMultiplier(1.5f, BuffRounding.Nearest)` (`Nearest` округляет середину от нуля; также есть `Floor`, `Ceiling`, `Truncate`).

* Вы можете создавать свои типы обработчиков, наследуясь от `BuffProcessor<T>`. Передайте колбэк в базовый конструктор или переопределите `Process`; переопределите `DefaultOrder`, чтобы изменить этап по умолчанию:
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

* Если вы сохраните созданный экземпляр обработчика, то сможете менять свойство `ProcessCallback` вместо создания нового экземпляра. Бафы, содержащие этот обработчик, уведомляются автоматически:
```cs
var buff = new Buff<float>(1f);
var processor = new BuffProcessor<float>(value => value + 5f);

buff.Add(processor);
Debug.Log(buff.Calculate()); // выводит 6

processor.ProcessCallback = value => value / 2f;
Debug.Log(buff.Calculate()); // выводит 0.5
```

* Используйте интерфейс `IBuff`, чтобы хранить вместе бафы с разными типами значений (например, чтобы вызвать `RemoveBySource` для всех характеристик персонажа). `IBuff.CalculateObject()` возвращает упакованное значение; если тип известен, используйте `Buff<T>.Calculate()`.

* Баф не потокобезопасен. Используйте его из основного потока.

## Тесты
Пакет содержит EditMode-тесты. Чтобы запустить их, добавьте пакет в `testables` в *Packages/manifest.json* и откройте Window > General > Test Runner:
```json
{
    "testables": [
        "com.demegraunt.buffs"
    ]
}
```

## Переход с 1.x
Сериализованные данные баф-полей сохраняются, пространство имён и имя сборки не изменились. Изменения в коде:

| 1.x | 2.0 |
|---|---|
| `BaseBuff` | `IBuff` |
| `BaseBuff.buffType` | `IBuff.ValueType` |
| `AddBase`, `TryAddBase`, `ReplaceBase` | `Add`, `TryAdd`, `Replace` у `Buff<T>` |
| `BaseBuffProcessor` | `BuffProcessor<T>` |
| `ProcessorsChanged` | `Changed` |
| `NotifyProcessorsChanged()` | `NotifyChanged()` |
| Несовпадение типов логировало ошибку | Типы проверяет компилятор |
| `Add` с повторным ID бросал исключение из `Dictionary` | `Add` бросает `ArgumentException`, `TryAdd` возвращает `false` |

⚠️ Изменение поведения: встроенные сумматоры теперь применяются раньше встроенных множителей, а пользовательские обработчики (порядок 0) — раньше их обоих. Если ваш код полагался на порядок вызовов `Add` с обработчиками разных типов, укажите порядок явно: `buff.Add(processor, order)`.

Полный список изменений — в [CHANGELOG.md](CHANGELOG.md).
