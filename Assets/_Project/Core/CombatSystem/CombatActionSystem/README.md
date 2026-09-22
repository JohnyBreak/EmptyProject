# Combat Action System — дизайн

Документ фиксирует договорённости из обсуждения боевой системы для Unity (C#): таблица переходов, абстракция действий, runner, триггеры, теги, cancel.

Код ниже — **примеры для ориентира**, не готовый пакет ассетов.

---

## Цель

Нужна структура, которая по:

- текущему действию (атака, уклон, парирование, idle…),
- текущему игровому триггеру (lightTap, dodge…),

даёт **следующее действие** и умеет отвечать, **какие триггеры доступны сейчас**.

Отдельно от комбо-переходов: политика **cancel** — какие действия могут оборвать текущее (и в какой фазе).

---

## Ключевые идеи

1. **Нет готового BCL/Unity-типа «combo tree»** — это таблица переходов поверх `Dictionary` + тонкая обёртка.
2. В SO удобно хранить **строковые имена** и список рёбер; в runtime после **bake** — ссылки на `ICombatAction`.
3. Lookup: **плоский составной ключ** `(fromId, trigger) → ICombatAction` — один O(1) запрос.
4. `ICombatAction` — **тонкий паспорт** (id, теги, timeline, данные). Логика — в **CombatActionRunner** и **handlers**.
5. **Transition** и **Cancel** — разные оси; не смешивать в одну кучу `if` внутри атаки.

| | Transition (комбо / commit) | Cancel / interrupt |
|---|---|---|
| Смысл | «из A по триггеру иду в B» | «B может оборвать A» |
| Пример | Attack1 + LightTap → Attack2 | Dodge отменяет Attack1 в Recovery |

---

## Типы

### `CombatTrigger` — намерение / событие на вход

Вход в систему решений runner’а. Не путать с тегами действия.

На старте достаточно enum:

```csharp
public enum CombatTrigger : byte
{
    None = 0,

    LightTap,
    LightHold,
    HeavyTap,
    HeavyHold,

    Dodge,
    Parry,
    Jump,

    // системные (из логики, не с геймпада)
    OnComplete,
    OnHit,
    OnWhiff,
}
```

Когда появятся направленный dodge / много скиллов — можно заменить на struct с параметром:

```csharp
public readonly struct CombatTrigger
{
    public CombatTriggerKind Kind { get; }
    public int Param { get; } // SkillId, направление dodge и т.д.

    public static CombatTrigger LightTap() => new(CombatTriggerKind.LightTap);
    public static CombatTrigger Dodge(int dir) => new(CombatTriggerKind.Dodge, dir);
    public static CombatTrigger Skill(int id) => new(CombatTriggerKind.Skill, id);
}
```

**Правило:** Trigger = *намерение*; Tag = *классификация результата*.

---

### `CombatActionTags` — классификация действия

Живут на действии. Нужны handlers, cancel-политике и фильтрам.

```csharp
[Flags]
public enum CombatActionTags : ushort
{
    None = 0,

    // роль
    Idle         = 1 << 0,
    Attack       = 1 << 1,
    Dodge        = 1 << 2,
    Parry        = 1 << 3,
    Block        = 1 << 4,
    Hitstun      = 1 << 5,
    Skill        = 1 << 6,

    // свойства (ортогонально роли)
    Melee           = 1 << 8,
    Ranged          = 1 << 9,
    Movement        = 1 << 10,
    Uninterruptible = 1 << 12,
}
```

Примеры:

- Attack1 → `Attack | Melee`
- Dodge roll → `Dodge | Movement`
- Uppercut → `Attack | Melee`

Не делать тег на каждый удар (`Attack1Tag`) — для уникальности есть `Id` / `AttackName`.

То, что меняется во времени (i-frames, окно cancel) — в **Timeline / фазах**, а не статичным флагом на всё действие.

---

### `CombatInputMask` — доступные триггеры из текущего действия

Для API «какие инпуты/триггеры доступны» при 4–8 вариантах удобна битовая маска (собирается на bake):

```csharp
[Flags]
public enum CombatInputMask : byte
{
    None      = 0,
    LightTap  = 1 << 0,
    LightHold = 1 << 1,
    HeavyTap  = 1 << 2,
    HeavyHold = 1 << 3,
    Dodge     = 1 << 4,
    Parry     = 1 << 5,
}
```

Проверка без аллокаций; для UI — хелпер `CopyAvailableInputs` в `Span<CombatTrigger>`.

---

### `ICombatAction` — тонкий паспорт действия

```csharp
public interface ICombatAction
{
    string Id { get; }
    CombatActionTags Tags { get; }
    ActionTimeline Timeline { get; }

    // опционально: типизированные данные с SO
    bool TryGet<T>(out T payload) where T : class;
}
```

Конфигурация по-прежнему может идти из ScriptableObject; внутри есть ключ вроде `AttackName` → это и есть `Id`.

**Не класть** в интерфейс толстую логику (`CanCancel`, полный FSM, знание про все другие действия). Иначе god-object и плохая расширяемость.

---

### `AttackTransitionEdge` / рёбра в SO

В инспекторе — список рёбер со **строками**; не `Dictionary` в сериализации.

```csharp
[Serializable]
public struct AttackTransitionEdge
{
    public string FromAttack;    // Id текущего
    public CombatTrigger Input;  // или CombatTriggerKind
    public string NextAttack;    // Id следующего
}
```

---

### `ActionTransitionTable` (бывш. AttackTransitionTable)

Runtime после bake:

- `_map`: `(fromId, trigger) → ICombatAction`
- `_available`: `fromId → CombatInputMask` (или аналог для всех CombatTrigger)

```csharp
public sealed class ActionTransitionTable
{
    readonly Dictionary<(string from, CombatTrigger trigger), ICombatAction> _map;
    readonly Dictionary<string, CombatInputMask> _available;

    public ActionTransitionTable(
        IReadOnlyDictionary<string, ICombatAction> actionsById,
        IEnumerable<AttackTransitionEdge> edges)
    {
        _map = new Dictionary<(string, CombatTrigger), ICombatAction>();
        _available = new Dictionary<string, CombatInputMask>();

        foreach (var e in edges)
        {
            if (!actionsById.TryGetValue(e.NextAttack, out var next))
                throw new KeyNotFoundException($"Unknown next action: {e.NextAttack}");

            var key = (e.FromAttack, e.Input);
            if (!_map.TryAdd(key, next))
                throw new InvalidOperationException($"Duplicate edge: {key}");

            _available.TryGetValue(e.FromAttack, out var mask);
            _available[e.FromAttack] = mask | ToMask(e.Input);
        }
    }

    public bool TryGetNext(string currentId, CombatTrigger trigger, out ICombatAction next)
        => _map.TryGetValue((currentId, trigger), out next);

    public CombatInputMask GetAvailableInputs(string actionId)
        => _available.TryGetValue(actionId, out var mask) ? mask : CombatInputMask.None;

    public bool HasInput(string actionId, CombatTrigger trigger)
        => (GetAvailableInputs(actionId) & ToMask(trigger)) != 0;
}
```

Почему value = `ICombatAction`, а не string: в бою нет повторного поиска по имени после bake.

Альтернативы, которые рассматривались:

| Подход | Плюсы | Минусы |
|--------|--------|--------|
| Вложенный `Dictionary<string, Dictionary<trigger, …>>` | Интуитивно «дети» узла | Два lookup, шумнее |
| Плоский `(from, trigger) → …` | Один O(1), просто | Список доступных триггеров — отдельный кэш на bake |
| Value = string | Проще на старте | Лишний resolve каждый раз |

**Выбранный:** плоский ключ + value = ссылка на действие + маска доступных триггеров.

---

### `ICancelPolicy`

```csharp
public interface ICancelPolicy
{
    bool CanCancel(ICombatAction current, ICombatAction incoming, ActionPhase phase);
}
```

Пример правила: Dodge может cancel любой `Attack`, кроме `Uninterruptible`, в фазах Active/Recovery (точные окна — в Timeline).

Уклон из recovery **не** надо прописывать ребром из каждой атаки — это cancel, не combo-edge.

---

### `IActionHandler` — исполнители по тегам / payload

```csharp
public interface IActionHandler
{
    bool Supports(ICombatAction action);
    void Enter(ICombatAction action, CombatContext ctx);
    void Tick(ICombatAction action, ActionPhase phase, float t, CombatContext ctx);
    void Exit(ICombatAction action, CombatContext ctx);
}
```

Примеры:

- `AnimationHandler` — почти всё → Play клипа
- `AttackHitHandler` — `Tags.Has(Attack)` → хитбоксы в Active, `TryGet<AttackHitData>`
- `DodgeHandler` — `Tags.Has(Dodge)` → i-frames, смещение
- `ParryHandler` — `Tags.Has(Parry)` → окно парирования

Одно действие может обслуживаться несколькими handlers.

---

### `CombatActionRunner` — диспетчер lifecycle

Держит текущее действие, время, фазу. На trigger: сначала cancel, потом transition. На смене: Exit → Enter через handlers.

```csharp
public sealed class CombatActionRunner
{
    readonly ActionTransitionTable _transitions;
    readonly ICancelPolicy _cancels;
    readonly IReadOnlyList<IActionHandler> _handlers;
    readonly CombatContext _ctx;

    // бинд «сырой trigger → действие» для cancel-целей вроде Dodge/Parry
    readonly IReadOnlyDictionary<CombatTrigger, ICombatAction> _triggerDefaults;

    ICombatAction _current;
    float _elapsed;
    ActionPhase _phase;

    public void SetInitial(ICombatAction idle) => Enter(idle);

    public void Tick(float dt, CombatTrigger? bufferedTrigger)
    {
        _elapsed += dt;
        _phase = _current.Timeline.Evaluate(_elapsed);

        if (bufferedTrigger is { } trigger)
            TryResolveTrigger(trigger);

        for (int i = 0; i < _handlers.Count; i++)
        {
            if (_handlers[i].Supports(_current))
                _handlers[i].Tick(_current, _phase, _elapsed, _ctx);
        }
    }

    void TryResolveTrigger(CombatTrigger trigger)
    {
        if (_triggerDefaults.TryGetValue(trigger, out var incoming)
            && _cancels.CanCancel(_current, incoming, _phase))
        {
            SwitchTo(incoming);
            return;
        }

        if (_transitions.TryGetNext(_current.Id, trigger, out var next)
            && _current.Timeline.AllowsTransition(_phase, trigger))
        {
            SwitchTo(next);
            // иначе оставить в буфере до окна
        }
    }

    void SwitchTo(ICombatAction next)
    {
        Exit(_current);
        Enter(next);
    }

    void Enter(ICombatAction next)
    {
        foreach (var h in _handlers)
            if (h.Supports(next)) h.Enter(next, _ctx);

        _current = next;
        _elapsed = 0f;
        _phase = ActionPhase.Startup;
    }

    void Exit(ICombatAction old)
    {
        foreach (var h in _handlers)
            if (h.Supports(old)) h.Exit(old, _ctx);
    }
}
```

Тонкий `ICombatAction` сам не «думает». Runner оркестрирует по **Timeline + Tags + Tables**; handlers делают конкретику.

---

## Примеры использования

### Комбо из SO

Рёбра в ScriptableObject:

| From     | Input     | Next     |
|----------|-----------|----------|
| attack1  | LightTap  | attack2  |
| attack1  | LightHold | uppercut |
| attack2  | HeavyTap  | finisher |

Bake:

```csharp
var registry = BuildActionsFromScriptableObjects(); // Id → ICombatAction
var table = new ActionTransitionTable(registry, edgesFromSo);

table.TryGetNext("attack1", CombatTrigger.LightTap, out var next);
// next.Id == "attack2", next уже готовый объект
```

### Доступные инпуты (UI / буфер)

```csharp
var mask = table.GetAvailableInputs(runner.Current.Id);
if ((mask & CombatInputMask.LightHold) != 0)
{
    // показать подсказку «hold light → uppercut»
}
```

### Cancel уклонением

```csharp
// CancelPolicy (упрощённо):
bool CanCancel(ICombatAction current, ICombatAction incoming, ActionPhase phase)
{
    if (!incoming.Tags.HasFlag(CombatActionTags.Dodge)) return false;
    if (current.Tags.HasFlag(CombatActionTags.Uninterruptible)) return false;
    if (!current.Tags.HasFlag(CombatActionTags.Attack)) return false;
    return phase is ActionPhase.Active or ActionPhase.Recovery
           && current.Timeline.IsCancelOpen(phase, CombatTrigger.Dodge);
}
```

Игрок жмёт Dodge во время recovery Attack1 → runner не ищет ребро `attack1+Dodge` в комбо-таблице, а проходит cancel → Enter(dodge_roll).

### Handler атаки

```csharp
public sealed class AttackHitHandler : IActionHandler
{
    public bool Supports(ICombatAction a)
        => a.Tags.HasFlag(CombatActionTags.Attack);

    public void Enter(ICombatAction action, CombatContext ctx) { /* armed = false */ }

    public void Tick(ICombatAction action, ActionPhase phase, float t, CombatContext ctx)
    {
        if (!action.TryGet<AttackHitData>(out var data)) return;
        // включить/выключить хитбоксы по phase и data
    }

    public void Exit(ICombatAction action, CombatContext ctx) { /* выключить хиты */ }
}
```

---

## Общий флоу

```text
[ScriptableObjects]
  CombatAction definitions (Id, Tags, Timeline, payload)
  Transition edges (From, Trigger, Next) — строки
  Cancel rules / tag policy
        │
        ▼ bake при загрузке
[Runtime]
  registry: Id → ICombatAction
  ActionTransitionTable: (from, trigger) → ICombatAction
                         from → available mask
  ICancelPolicy
  Handlers[]
  CombatActionRunner (current, phase, elapsed)
        │
        ▼ каждый кадр
[Input layer] → буфер CombatTrigger
        │
        ▼
Runner.Tick(dt, trigger?)
  1. Обновить elapsed / phase по Timeline
  2. Если есть trigger:
       a. Cancel? (defaults + CancelPolicy + фаза) → SwitchTo
       b. Иначе Transition? (таблица + AllowsTransition) → SwitchTo
       c. Иначе держать в буфере до окна
  3. Handlers.Tick(current, phase, …)
        │
        ▼ при SwitchTo
  Exit(old handlers) → Enter(new handlers)
  Animation / hits / i-frames / parry window …
```

Жизненный цикл одного удара:

```text
Enter(Attack1)
  AnimationHandler: Play("attack1")
  AttackHitHandler: armed = false

Startup → Active
  AttackHitHandler: хитбокс ON

Active → Recovery
  хитбокс OFF
  окно: LightTap → Attack2 (TransitionTable)
  окно: Dodge → cancel (CancelPolicy)

Exit(Attack1) → Enter(Attack2 | Dodge | Idle)
```

---

## Слои ответственности (кратко)

| Слой | Ответственность |
|------|-----------------|
| SO / edges | Данные для дизайнера |
| Bake | Строки → ссылки, валидация дублей и битых Id |
| `ICombatAction` | Паспорт действия |
| `ActionTransitionTable` | Комбо / осознанные переходы |
| `ICancelPolicy` | Кто кого может прервать |
| `CombatActionRunner` | Время, фазы, приоритет cancel vs transition |
| `IActionHandler` | Конкретное поведение по тегам/payload |
| Input layer | Сырой ввод → `CombatTrigger` + буфер |

---

## Что сознательно не смешивать

1. **Trigger ≠ Tag** — нажал Dodge vs действие с тегом Dodge.
2. **Combo edge ≠ Cancel** — follow-up удар vs прерывание уклоном.
3. **Данные действия ≠ Runner** — SO не должен знать весь граф отмен.
4. **Уникальность удара** — через `Id`, не через сотни тегов.

---

## Открытые следующие шаги (не зафиксированы)

- Где живёт таблица: один SO на персонажа / stance / weapon.
- Приоритет буфера, если в одном кадре и LightTap, и Dodge.
- Детальная модель `ActionTimeline` (окна cancel / transition по нормализованному времени или аним-событиям).

---

*Собрано из дизайн-обсуждения. Реализацию в проект можно переносить по частям, начиная с таблицы переходов и bake.*
