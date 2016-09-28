using Cute_RTS.Scenes;
using Cute_RTS.Structures;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cute_RTS.AI
{
    class PlayerBehaviourTree : Component, IUpdatable
    {
        private BehaviorTree<PlayerBehaviourTree> _tree;
        private Player _player;
        private Player _opponent;
        private PlayerState _state;

        bool walkingToEnemy = false;
        bool walkingToFlag = false;
        float _elapsedTime = 0;
        const float UPDATE_PERIOD = 0.5f;

        //private Stack<AIPlayer.UnitCommand> _commandStack;

        public PlayerBehaviourTree(Player opponent)
        {
            _opponent = opponent;
            
            //_commandStack = new Stack<AIPlayer.UnitCommand>();
            //_player = player;

        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();

            _player = entity as Player;
            _state = new PlayerState(_player);

            buildTree();
        }

        public override void onRemovedFromEntity()
        {
            base.onRemovedFromEntity();
        }

        public void update()
        {
            _elapsedTime -= Time.deltaTime;
            if (_elapsedTime <= 0)
            {
                while (_elapsedTime <= 0)
                    _elapsedTime += UPDATE_PERIOD;
                _state.updateState();
                _tree.tick();
            }
            
        }

        private void buildTree()
        {
            var builder = BehaviorTreeBuilder<PlayerBehaviourTree>.begin(this);
            builder.selector(AbortTypes.Self);
            builder.parallelSelector();

            builder.selector()
                // defend base it priority #1!!
                .action(b => defendIfBaseThreatened())
                    .selector()
                        // attack enemy when I have more units...
                        .conditionalDecorator(b => _player.Units.Count > _opponent.Units.Count)
                            .action(b => attackEnemy())
                        // ...otherwise capture flags
                        .action(b => b.captureNearestFlag())
                    .endComposite()
                .endComposite();

            // always choose to build units when resource is available
            builder.conditionalDecorator(b => b._player.Gold >= 50).untilFail();
            builder.sequence()
                .logAction("Building muh units!")
                .action(b => b.trainUnit())
                .endComposite();

            
            builder.endComposite();
            builder.endComposite();

            _tree = builder.build();
            _tree.updatePeriod = 0; // we define the update peiod in this component instead
        }

        private TaskStatus defendIfBaseThreatened()
        {
            if (_state.getThreatLevel() > 0)
            {
                List<BaseUnit> infantries = new List<BaseUnit>();
                foreach (var u in _player.Units)
                {
                    if (u is BaseUnit)
                    {
                        infantries.Add(u as BaseUnit);
                    }
                }
                for (int i = 0; i < infantries.Count; i++)
                {
                    if (i > _state.Threats.Count) break;

                    infantries[i].attackLocation(_player.mainBase.transform.position.ToPoint());
                }
                return TaskStatus.Success;
            } else
            {
                _state.Defenders.ForEach(defenders => defenders.stopMoving());
                return TaskStatus.Failure;
            }
        }

        private TaskStatus attackEnemy()
        {
            Debug.log("Heading to War!");
            if (!walkingToEnemy)
            {
                walkingToEnemy = true;

                foreach (Attackable unit in _player.Units)
                {
                    if (unit != null && unit is BaseUnit && _opponent.Units.Count > 0)
                    {
                        BaseUnit u = unit as BaseUnit;
                        if (u.ActiveCommand == BaseUnit.UnitCommand.Idle)
                        {
                            u.attackLocation(_opponent.Units[0].transform.position.ToPoint());
                        }
                    }
                    
                }
                return TaskStatus.Running;
            }else
            {
                foreach (Attackable unit in _player.Units)
                {
                    if (unit is BaseUnit)
                    {
                        BaseUnit u = unit as BaseUnit;
                        if (u.ActiveCommand != BaseUnit.UnitCommand.AttackLocation)
                        {
                            walkingToEnemy = false;
                            return TaskStatus.Success;
                        }
                       
                    }
                }
                return TaskStatus.Running;
            }
        }

        private TaskStatus captureNearestFlag()
        {
            walkingToFlag = true;
            List<CaptureFlag> captureFlags = new List<CaptureFlag>();
            ((GameScene)entity.scene).captureFlags.ForEach(item => captureFlags.Add(item));
            foreach (Attackable unit in _player.Units)
            {
                if (unit is BaseUnit)
                {
                    BaseUnit u = unit as BaseUnit;

                    // unit shouldn't be tasked to capture flag when it is busy
                    if (u.ActiveCommand != BaseUnit.UnitCommand.Idle) continue;

                    float nearestDist = 999999;
                    var nearestIndex = -1;
                    var currentIndex = 0;
                    foreach (var flag in captureFlags)
                    {
                        if (flag != null && flag.Capturer != entity)
                        {
                            var dist = Vector2.Distance(u.transform.position, flag.transform.position);
                            if (nearestIndex != currentIndex && nearestDist > dist)
                            {
                                nearestDist = dist;
                                nearestIndex = currentIndex;
                            }
                        }
                        currentIndex++;
                    }
                    if (nearestIndex != -1 && u.ActiveCommand != BaseUnit.UnitCommand.EnemyCaptureFlag)
                    {
                        Console.WriteLine("Capturing Flag #" + nearestIndex);
                        bool foundPath = u.enemyCaptureFlag(captureFlags[nearestIndex]);
                        Console.WriteLine("Found Path? - " + foundPath);

                        if (captureFlags[nearestIndex].Capturer != null)
                        { 
                            Console.WriteLine(captureFlags[nearestIndex].Capturer.Name);
                            Console.WriteLine(captureFlags[nearestIndex].Capturer == entity);
                        }
                        
                        captureFlags.Remove(captureFlags[nearestIndex]);
                        nearestIndex = -1;
                    }
                    else
                    {
                        return TaskStatus.Failure;
                    }
                }
            }
            return TaskStatus.Success;
        }

        private TaskStatus trainUnit()
        {
            Console.WriteLine("Training units " + _player.Gold);
            _player.mainBase.trainUnit();
            return TaskStatus.Success;
        }


    }
}
