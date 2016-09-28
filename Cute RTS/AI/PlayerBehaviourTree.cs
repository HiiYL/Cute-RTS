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

            //builder.conditionalDecorator(b => b._player.Units.Count >= b._opponent.Units.Count);
            //builder.sequence()
            //    .logAction("Enemy is WEAKER! CHARRGGEEE")
            //    .action( b => attackEnemy())
            //    .endComposite();


            builder.selector()
                .action(b => defendIfBaseThreatened())
                    .selector()
                        .conditionalDecorator(b => _player.Units.Count >= _opponent.Units.Count)
                            .action(b => attackEnemy())
                        .action(b => b.captureNearestFlag())
                    .endComposite()
                .endComposite();


            

            builder.conditionalDecorator(b => b._player.Gold >= 50).untilFail();
            builder.sequence()
                .logAction("Building muh units!")
                .action(b => b.trainUnit())
                .endComposite();

            
            //builder.conditionalDecorator(b => b._player.Units.Count < b._opponent.Units.Count);
            //builder.sequence()
            //    .logAction("I am Weaker, Better Find a flag!")
            //    .action(b => b.captureNearestFlag())
            //    .endComposite();
            //builder.endComposite();

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

        private TaskStatus captureFlag()
        {

            if(!walkingToFlag)
            {
                walkingToFlag = true;
                var captureFlags = ((GameScene)entity.scene).captureFlags;
                foreach (Attackable unit in _player.Units)
                {
                    if (unit is BaseUnit)
                    {
                        BaseUnit u = unit as BaseUnit;
                        foreach (var flag in captureFlags)
                        {
                            if (flag != null && flag.Capturer != entity)
                            {
                                u.captureFlag(flag);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Attackable unit in _player.Units)
                {
                    if (unit is BaseUnit)
                    {
                        BaseUnit u = unit as BaseUnit;
                        if (u.ActiveCommand != BaseUnit.UnitCommand.CaptureFlag)
                        {
                            walkingToFlag = false;
                            //return TaskStatus.Success;
                        }
                    }
                }
            }
            return TaskStatus.Success;
        }

        private TaskStatus captureNearestFlag()
        {
            if (!walkingToFlag)
            {
                walkingToFlag = true;
                var captureFlags = ((GameScene)entity.scene).captureFlags;
                foreach (Attackable unit in _player.Units)
                {
                    if (unit is BaseUnit)
                    {
                        BaseUnit u = unit as BaseUnit;
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
                        if (nearestIndex != -1)
                        {
                            Console.WriteLine("Capturing Flag #" + nearestIndex);
                            bool foundPath = u.captureFlag(captureFlags[nearestIndex]);
                            Console.WriteLine("Found Path? - " + foundPath);
                            if (captureFlags[nearestIndex].Capturer != null)
                                Console.WriteLine(captureFlags[nearestIndex].Capturer.Name);
                            Console.WriteLine(captureFlags[nearestIndex].Capturer == entity);
                            captureFlags.Remove(captureFlags[nearestIndex]);
                            nearestIndex = -1;
                        }
                        else
                        {
                            return TaskStatus.Failure;
                        }
                    }
                }
            }
            else
            {
                foreach (Attackable unit in _player.Units)
                {
                    if (unit is BaseUnit)
                    {
                        BaseUnit u = unit as BaseUnit;
                        if (u.ActiveCommand != BaseUnit.UnitCommand.CaptureFlag)
                        {
                            walkingToFlag = false;
                            //return TaskStatus.Success;
                        }
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
