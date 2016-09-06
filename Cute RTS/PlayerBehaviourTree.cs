using Cute_RTS.Scenes;
using Cute_RTS.Units;
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
            _commandStack = new Stack<AIPlayer.UnitCommand>();
            _player = player;
            _opponent = opponent;
        }

        private void buildTree()
        {
            var builder = BehaviorTreeBuilder<PlayerBehaviourTree>.begin(this);
            builder.selector(AbortTypes.Self);

            
            builder.conditionalDecorator(b => b._opponent.Units.Count <= 0);
            builder.sequence()
                .logAction("No enemies Left! Time to get some flags!")
                .action(b => b.captureFlag())
                .endComposite(); 
            
            builder.conditionalDecorator(b => b._player.Units.Count >= b._opponent.Units.Count);
            builder.sequence()
                .logAction("Enemy is WEAKER! CHARRGGEEE")
                .action( b => b.attackEnemy())
                .endComposite();
            

            builder.endComposite();
            _tree = builder.build();
        }

        private TaskStatus attackEnemy()
        {
            Debug.log("Heading to War!");
            if (!walkingToEnemy)
            {
                walkingToEnemy = true;

                foreach (Attackable unit in _player.Units)
                {
                    if (unit is BaseUnit)
                    {
                        BaseUnit u = unit as BaseUnit;
                        u.attackUnit(_opponent.Units[0]);
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
                        if (u.ActiveCommand != BaseUnit.UnitCommand.AttackUnit)
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
    }
}
