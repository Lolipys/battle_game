# BattleGame

Пошаговая стратегия на C# с консольным и графическим интерфейсом (WinForms). Две армии сражаются ход за ходом: ближний бой, дальние атаки, лечение, клонирование, баффы, смена тактики и полный Undo/Redo.

## Запуск

```bash
dotnet run --project BattleGame.csproj
```

При запуске выбирается режим: **терминал** или **графический интерфейс**.

## Геймплей

1. Создать новую игру или загрузить сохранение.
2. Задать бюджет — каждая армия набирается в его пределах (случайно или вручную).
3. Выбрать тактику построения (3 варианта).
4. Делать ходы вручную, авто-боем или откатывать их назад (Undo/Redo/Reset).
5. Сохранить в любой момент; файлы хранятся в `saves/`.

## Типы юнитов

| Тип | Роль | Особенность |
|-----|------|-------------|
| Тяжёлый пехотинец | Танк | Высокая защита и HP; принимает баффы от оруженосцев |
| Лёгкий пехотинец | Дамагер + оруженосец | 60% шанс надеть бафф на соседнего тяжёлого; клонируется |
| Лучник | Дальний урон | Игнорирует 50% защиты; стреляет с позиции ≤ Range |
| Хилер | Поддержка | Лечит союзников в радиусе Range |
| Маг | Поддержка | С вероятностью CloneChance% клонирует союзника |
| Гуляй-город | Защитник | Максимальная защита; не атакует, не лечится, не клонируется |

## Баффы (Decorator)

Оруженосец (LightInfantry) надевает один случайный бафф в ход на соседнего HeavyInfantry:

| Бафф | ATK | DEF |
|------|-----|-----|
| Конь | +6 | +4 |
| Копьё | +5 | — |
| Щит | — | +5 |
| Шлем | — | +3 |

Баффы цепочкой оборачивают юнита (Decorator). При сильном ударе (HP < 50%) — 30% шанс сбить верхний бафф.

## Тактики построения (Strategy)

| Тактика | Описание |
|---------|----------|
| Узкий мост (1 vs 1) | Только первые юниты бьются |
| Широкий мост (по трое) | Первые 3 позиции бьются попарно |
| Стенка на стенку | Каждый i-й бьёт i-го противника |

Тактику можно сменить в любой момент — действие записывается в историю и поддерживает Undo.

## Механика боя (один ход)

1. **Ближний бой** — по выбранной тактике (Strategy).
2. **Спецспособности** — лучники стреляют, хилеры лечат, маги клонируют, оруженосцы надевают баффы.
3. **Очистка** — мёртвые удаляются, строй смыкается. Случайный сбой баффа при сильном уроне.

Урон = `ATK − DEF`, минимум 1. Лучник: `Power − DEF/2`.

**Ничья:** 10 ходов без нанесения урона — DamageObserver фиксирует пат.

## Undo / Redo / Reset

| Действие | Что делает |
|----------|-----------|
| Undo | Откатывает один ход / одно действие |
| Redo | Повторяет отменённое |
| Reset | Откатывает всё до начала игры |

Снимок состояния делается через JSON-сериализацию (`Battlefield.Snapshot()`).

## Формула цены

```
Price = Damage × 2 + ⌈Defense × 1.5⌉ + MaxHealth
```

## Структура проекта

```
BattleGame/
├── Program.cs                    # Точка входа, меню, игровой цикл
├── Army.cs                       # Коллекция юнитов
├── Battlefield.cs                # Ядро боя: 3 фазы, Observer, Strategy
│
├── Interfaces/
│   ├── IBattleLogger.cs          # Контракт логирования (DI)
│   ├── IBattleObserver.cs        # Observer: OnDamage / OnDeath / OnTurnStarted
│   ├── IBattleStrategy.cs        # Strategy: ExecuteMelee
│   ├── ICommand.cs               # Command: Execute / Undo
│   ├── ISpecialAbility.cs        # Дальняя атака / лечение / оруженосец
│   ├── ICanBeHealed.cs           # Маркер: юнит лечится хилером
│   └── ICanBeCloned.cs           # Маркер: юнит клонируется магом
│
├── Units/
│   ├── Unit.cs                   # Базовый класс (виртуальные свойства для Decorator)
│   ├── HeavyInfantry.cs          # Танк
│   ├── LightInfantry.cs          # Дамагер + оруженосец
│   ├── Archer.cs                 # Дальний урон
│   ├── Healer.cs                 # Лечение
│   ├── Wizard.cs                 # Клонирование
│   ├── GulyayGorodAdapter.cs     # Adapter (MedievalRussia.dll)
│   └── Buffs/
│       ├── UnitBuff.cs           # Базовый Decorator-бафф
│       ├── HorseBuff.cs          # +ATK +DEF
│       ├── SpearBuff.cs          # +ATK
│       ├── ShieldBuff.cs         # +DEF (сильный)
│       └── HelmetBuff.cs         # +DEF (слабый)
│
├── Services/
│   ├── ArmyFactory.cs            # Singleton + Abstract Factory
│   ├── RandomUnitFactory.cs      # Конкретная фабрика с рандомизацией ±variance
│   ├── IUnitFactory.cs           # Контракт фабрики юнитов
│   ├── BattleLog.cs              # Цветной вывод в консоль
│   ├── GameSerializer.cs         # JSON-сохранение (System.Text.Json)
│   ├── Commands/
│   │   ├── ICommand.cs           # (интерфейс в Interfaces/)
│   │   ├── CommandManager.cs     # Стеки истории и redo
│   │   ├── MakeTurnCommand.cs    # Один ход + снимок для Undo
│   │   ├── PlayToEndCommand.cs   # Авто-бой + один снимок для Undo
│   │   └── ChangeStrategyCommand.cs # Смена тактики с Undo
│   ├── Observers/
│   │   ├── DeathObserver.cs      # Логирует гибель, считает потери
│   │   └── DamageObserver.cs     # Отслеживает урон, детектирует ничью
│   └── Strategies/
│       ├── BridgeStrategy.cs     # 1 vs 1
│       ├── WideBridgeStrategy.cs # 3 vs 3
│       ├── WallStrategy.cs       # N vs N
│       └── StrategyResolver.cs   # Резолвер по имени (для загрузки/Undo)
│
├── GUI/
│   ├── MainForm.cs               # Главное окно (кнопки, лог, клавиши)
│   ├── BattleFieldPanel.cs       # Визуализация армий с анимациями
│   ├── GuiLogger.cs              # IBattleLogger → RichTextBox
│   ├── ManualArmyForm.cs         # Ручной набор армии с бюджетом
│   ├── NewGameForm.cs            # Настройки новой игры
│   └── UnitStatEditor.cs         # Редактор характеристик юнита
│
└── lib/
    └── MedievalRussia.dll        # Внешняя библиотека с GulyayGorod
```

## Паттерны проектирования

| Паттерн | Где применён |
|---------|-------------|
| Singleton | `ArmyFactory.Instance` |
| Abstract Factory | `IUnitFactory` / `RandomUnitFactory` |
| Adapter | `GulyayGorodAdapter` → `MedievalRussia.GulyayGorod` |
| Decorator | `UnitBuff` и наследники (баффы на юнита) |
| Strategy | `IBattleStrategy`: Bridge / WideBridge / Wall |
| Observer | `IBattleObserver`: DeathObserver, DamageObserver |
| Command | `ICommand`: MakeTurnCommand, PlayToEndCommand, ChangeStrategyCommand |
| Dependency Injection | `Battlefield(IBattleLogger)`, `CommandManager` |

## Сохранения

Файлы — в `saves/` рядом с папкой проекта (создаётся автоматически). Формат: JSON с полиморфной сериализацией (`JsonDerivedType` на `Unit`). Логгер и наблюдатели не сохраняются — назначаются заново при загрузке через `AttachServices`.
