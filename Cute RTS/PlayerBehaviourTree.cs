﻿using Cute_RTS.Scenes;
using Cute_RTS.Structures;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cute_RTS
{
    class PlayerBehaviourTree : Component, IUpdatable
    {
        private BehaviorTree<PlayerBehaviourTree> _tree;
        private AIPlayer _player;
        private Player _opponent;

        bool walkingToEnemy = false;
        bool walkingToFlag = false;

        private Stack<AIPlayer.UnitCommand> _commandStack;

        public PlayerBehaviourTree(AIPlayer player, Player opponent)
        {
            //_commandStack = new Stack<AIPlayer.UnitCommand>();
            //_player = player;
            _opponent = opponent;
        }

        private void buildTree()
        {
            var builder = BehaviorTreeBuilder<PlayerBehaviourTree>.begin(this);
            builder.sequence(AbortTypes.Self);


            /*builder.conditionalDecorator(b => b._opponent.Units.Count <= 0);
            builder.sequence()
                .logAction("No enemies Left! Time to get some flags!")
                .action(b => b.captureFlag())
                .endComposite(); */
            //builder.parallel();
            

            builder.conditionalDecorator(b => b._player.Units.Count >= b._opponent.Units.Count);
            builder.sequence()
                .logAction("Enemy is WEAKER! CHARRGGEEE")
                .action( b => b.attackEnemy())
                .endComposite();

            /*

            builder.conditionalDecorator(b => b._player.Gold >= 50).untilFail();
            builder.sequence()
                .logAction("Building muh units!")
                .action(b => b.trainUnit())
                .endComposite();

            builder.conditionalDecorator(b => b._player.Units.Count < b._opponent.Units.Count);
            builder.sequence()
                .logAction("I am Weaker, Better Find a flag!")
                .action(b => b.captureNearestFlag())
                .endComposite();
            builder.endComposite();
            */




            builder.endComposite();
            _tree = builder.build();
        }

        public override void onAddedToEntity()
        {
            buildTree();
            base.onAddedToEntity();
        }

        public override void onRemovedFromEntity()
        {
            base.onRemovedFromEntity();
        }

        public void update()
        {
            if (_tree != null)
                _tree.tick();
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
            return TaskStatus.Running;
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
                        Console.WriteLine("Capturing Flag #" + nearestIndex);
                        bool foundPath = u.captureFlag(captureFlags[nearestIndex]);
                        Console.WriteLine("Found Path? - " + foundPath);
                        if(captureFlags[nearestIndex].Capturer != null)
                          Console.WriteLine(captureFlags[nearestIndex].Capturer.Name);
                          Console.WriteLine(captureFlags[nearestIndex].Capturer == entity);
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
            return TaskStatus.Running;
        }

        private TaskStatus trainUnit()
        {
            Console.WriteLine("Training units " + _player.Gold);
            _player.mainBase.trainUnit();
            return TaskStatus.Success;
        }
    }
}
