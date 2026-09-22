using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Extensions;

namespace Core.CombatSystem
{
    [Serializable]
    public class HitBoxSettings
    {
        public Vector2 Offset;
        public Vector2 Size;
        public float Rotation;

        public HitBoxSettings(
            Vector2 offset,
            Vector2 size,
            float rotation = 0)
        {
            Offset = offset;
            Size = size;
            Rotation = rotation;
        }
    }

    [Serializable]
    public class AttackMovementSettings
    {
        public AnimationCurve Curve = AnimationCurve.Constant(0, 0, 0);
        public float DistanceMultiplier;
        public float StartTime;
        public float Duration;
        public bool IsEmpty;
        
        public AttackMovementSettings(
            AnimationCurve curve,
            float distanceMultiplier,
            float startTime,
            float duration,
            bool isEmpty = false)
        {
            Curve = curve;
            DistanceMultiplier = distanceMultiplier;
            StartTime = startTime;
            Duration = duration;
            IsEmpty = isEmpty;
        }

        public static AttackMovementSettings Empty()
        {
            return new AttackMovementSettings(AnimationCurve.Constant(0, 0, 0), 1, 0, 0, true);
        }
    }
    
    [Serializable]
    public class Attack
    {
        public string Name;
        public string AnimationName;
        public float Duration;
        public float BaseDamage;
        public float NextInputWindowStart = 0.5f;
        public float NextAttackTransitionStart = .8f;
        public float IdleTransitionStart = 0.9f;
        public AttackMovementSettings XMovementSettings = AttackMovementSettings.Empty();
        public AttackMovementSettings YMovementSettings = AttackMovementSettings.Empty();
        public List<HitBoxSettings> HitBoxSettings = new List<HitBoxSettings>();

        public Attack(
            string name,
            string animationName,
            float duration,
            float baseDamage,
            float nextInputWindowStart,
            float nextAttackTransitionStart,
            float idleTransitionStart,
            List<HitBoxSettings> hitBoxSettings,
            AttackMovementSettings xMovementSettings = null,
            AttackMovementSettings yMovementSettings = null)
        {
            Name = name;
            AnimationName = animationName;
            Duration = duration;
            BaseDamage = baseDamage;
            NextInputWindowStart = nextInputWindowStart;
            NextAttackTransitionStart = nextAttackTransitionStart;
            IdleTransitionStart = idleTransitionStart;

            if (!hitBoxSettings.IsNull() && hitBoxSettings.Count > 0)
            {
                HitBoxSettings = hitBoxSettings;
            }

            if (!xMovementSettings.IsNull())
            {
                XMovementSettings = xMovementSettings;
            }

            if (!yMovementSettings.IsNull())
            {
                YMovementSettings = yMovementSettings;
            }
        }
    }

    [CreateAssetMenu(fileName = "AttacksConfig", menuName = "CombatSystem/AttacksConfig")]
    public class AttacksConfig : ScriptableObject
    {
        public List<Attack> Attacks;
    }
    
    [Serializable]
    public enum AttackInputType
    {
        None = 0,
        LightTap = 1,
        HeavyTap = 2,
        LightHold = 3,
        HeavyHold = 4
    }
    
    [Serializable]
    public class AttackTransitionEdge
    {
        public string FromAttackName;
        public AttackInputType InputType;
        public string ToAttackName;
    }

    [CreateAssetMenu(fileName = "ChainTree", menuName = "CombatSystem/ChainTree")]
    public class ChainTree : ScriptableObject
    {
        public List<AttackTransitionEdge> Edges;
    }

    public sealed class AttackTransition
    {
        private readonly Dictionary<(string, AttackInputType), Attack> _map = new Dictionary<(string, AttackInputType), Attack>();

        public AttackTransition(
            Dictionary<string, Attack> attacks,
            List<AttackTransitionEdge> edges)
        {
            foreach (var edge in edges)
            {
                if (!attacks.TryGetValue(edge.ToAttackName, out var value))
                {
                    throw new KeyNotFoundException($"Unknown next attack: {edge.ToAttackName}");
                }

                var key = (edge.FromAttackName, edge.InputType);

                if (!_map.TryAdd(key, value))
                {
                    throw new InvalidOperationException($"Duplicate edge: {key}");
                }
            }
        }
        
        public bool TryGetNext(string currentAttackName, AttackInputType input, out Attack next)
        {
            return _map.TryGetValue((currentAttackName, input), out next);
        } 
    }

    public class CombatSystem
    {
        private readonly AttacksConfig _config;
        private readonly Animator _animator;
        private Attack _currentAttack;
        private Attack _nextAttack;
        private bool _inputAllowed = true;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        public CombatSystem(AttacksConfig config, Animator animator)// todo вместо прямого аниматора передавать юнита, который атакует ??
        {
            _config = config;
            _animator = animator;
        }

        public void ProcessAttack(AttackInputType inputType)
        {
            if (!_inputAllowed)
            {
                return;
            }

            if (_currentAttack == null)// todo брать следующую атаку из дерева
            {
                if (inputType == AttackInputType.LightTap)
                {
                    ScheduleNext(_config.Attacks[0]);
                }
                
                if (inputType == AttackInputType.HeavyTap)
                {
                    ScheduleNext(_config.Attacks[2]);
                }

                return;
            }
            
            if (inputType == AttackInputType.LightTap)
            {
                ScheduleNext(_config.Attacks[1]);
            }
        }

        private void ScheduleNext(Attack nextAttack)
        {
            if (_currentAttack != null)
            {
                _nextAttack = nextAttack;
                return;
            }

            StartAttack(nextAttack);
        }

        private void StartAttack(Attack attack)
        {
            _nextAttack = null;
            _currentAttack = attack;
            _inputAllowed = false;
            _animator.Play(attack.AnimationName);

            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            
            DelayAndExecute(
                GetNormalizedTime(attack.Duration, attack.NextInputWindowStart),
                OnInputAllowed,
                _cancellationTokenSource.Token)
                .Forget();
            
            DelayAndExecute(
                GetNormalizedTime(attack.Duration, attack.NextAttackTransitionStart),
                    OnNextTransitionStart,
                    _cancellationTokenSource.Token)
                .Forget();
            
            DelayAndExecute(
                    GetNormalizedTime(attack.Duration, attack.IdleTransitionStart),
                    OnIdleTransitionStart,
                    _cancellationTokenSource.Token)
                .Forget();
            
        }

        private float GetNormalizedTime(float duration, float normalizedValue)
        {
            return duration * normalizedValue;
        }

        private void OnInputAllowed()
        {
            _inputAllowed = true;
        }
        
        private void OnNextTransitionStart()
        {
            if (_nextAttack == null)
            {
                return;
            }
            StartAttack(_nextAttack);
        }
        
        private void OnIdleTransitionStart()
        {
            _animator.Play("Idle");
            _currentAttack = null;
            OnInputAllowed();
        }
        
        private async UniTaskVoid DelayAndExecute(float seconds, Action action, CancellationToken cancellationToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellationToken);
        
            action?.Invoke();
        }
    }

    public class CombatTest : MonoBehaviour
    {
        [SerializeField] private AttacksConfig _config;

        [SerializeField] private Animator _animator;
        private CombatSystem _combatSystem;

        private void Start()
        {
            _combatSystem = new CombatSystem(_config, _animator);
        }

        private void Update()
        {
            AttackInputType input = AttackInputType.None;
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                input = AttackInputType.LightTap;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                input = AttackInputType.HeavyTap;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                input = AttackInputType.LightHold;
            }

            if (input == AttackInputType.None)
            {
                return;
            }
            
            _combatSystem.ProcessAttack(input);
        }

        /*
         атака это не просто класс, это интерфейс ICombatAction
         так мы можем добавить другие действия типа парирования уклонения спец приема через зажатие нескольких кнопок 
         или оперделенных боевых ситуаций типа супер добивания оглушенного враза при нажатии 2 кнопок
         */
        /*
         инпут тайп заменить на триггер, напрмиер
         public readonly struct CombatTrigger
        {
            public CombatTriggerKind Kind { get; }
            public int Param { get; } // SkillId, Direction (8-way dodge), etc.
            public static CombatTrigger LightTap() => new(CombatTriggerKind.LightTap);
            public static CombatTrigger Dodge(int dir) => new(CombatTriggerKind.Dodge, dir);
            public static CombatTrigger Skill(int id) => new(CombatTriggerKind.Skill, id);
        }
         */
        
        /*
         добавить тэги чтобы понимать кто что и почему
         [Flags]
        public enum CombatActionTags : ushort
         
         */
        
        /*
атака как объект
атака создается, конфигурируется и записывается в мапу по имени
точка входа для всей системы - список конфигураций: название, название анимации, время, урон
    настройки для хитбоксов
    {
        время когда появиться
        позиция относительно персонажа
        размеры
    }
    настройки движения персонажа во время атаки
    две кривые по X и Y
    настройки расстояния, например 5 по X
    нормализованное время начала движения, если движение начинается по среди атаки, то ставим 0.5f
    время движения, скорость, с которой происходит evaluate кривой от 0 до 1
*/

/*
за создание хитбоксов ответственна отдельная система, берет позицию персонажа,
его направление, настройки хитбокса и создает хитбокс, возвращает его наружу
снаружи хитокс уже использует другая система для определения попадания
 */

/*
граф цепочек атак
тут прописывается из какой атаки можно пойти дальше в другие атаки и при каких инпутах
например атака Light1
может перейти в Light2 при нажатии кнопки легкой атаки,
может перейти в Uppercut при зажатии кнопки легкой атаки,
может перейти в HeavyKick при нажатии кнопки тяжелой атаки
*/

/*
урон это объект
внутри кто нанес, название атаки, кому нанес, оружие, если в игре есть разное оружие

система урона по этим данным может посчитать и выдать правильный урон кому надо и при этом
назначить временные статус эффекты в зависимости от атаки или типа оружия(если такое предполагается), по типу ошеломления, отравления/кровотечения и тд

урон по персонажу учитывается через хэшсет, если 1 хитбокс попал по врагу,
другие хитбоксы этой же атаки не реагируют на этого врага или херт бокс, относящийся к сущности (бочка)

для урона потом можно отдельно накрутить систему, которая учитывает временные эффекты или прокачку урона
сам урон в атаку идет базовый из конфига, в момент подсчета нанесенного урона берется базовый показатель урона,
высчитывается урон исходя из показателей прокачки, например базовое 5 умножается на 1,5 от прокачки 1 перка,
+ 3 от прокачки другого перка и получается 10,5
и добавляем временный эффект от берсерка "урон Х2", в итоге получаем 21 урона
*/


/*
можно добавить возможность канцела атаки и перехода в другое состояние, например в додж или парирование
а еще приоритет атаки, например нажатия и зажатия кнопки, но лучше детектить это еще на моменте чтения инпута
 */
    }
}