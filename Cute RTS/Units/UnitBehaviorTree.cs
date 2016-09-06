using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Cute_RTS.Units
{
    class UnitBehaviorTree : Component, IUpdatable
    {
        private BehaviorTree<UnitBehaviorTree> _tree;
        private BaseUnit _baseunit;
        private PathMover _pathmover;
        private bool _isAttacking = false;
        private Timer _attackTimer;
        private Stack<BaseUnit.UnitCommand> _commandStack;

        public UnitBehaviorTree(BaseUnit bu, PathMover pm)
        {
            _commandStack = new Stack<BaseUnit.UnitCommand>();
            _attackTimer = new Timer(bu.AttackSpeed * 1000);
            _attackTimer.Elapsed += _attackTimer_Elapsed;
            _baseunit = bu;
            _pathmover = pm;
        }

        private void buildTree()
        {
            var builder = BehaviorTreeBuilder<UnitBehaviorTree>.begin(this);
            builder.selector(AbortTypes.Self);

            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.Idle);
            builder.sequence()
                .action(b => radarCheck())
                .action(b => becomeIdle())
                .endComposite();


            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.GoTo);
            builder.sequence()
                .action(b => checkArrival())
                .action(b => becomeIdle())
                .endComposite();

            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.Follow);
            builder.sequence()
                .action(b => followUnit(3))
                .action(b => becomeIdle())
                .endComposite();

            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.AttackUnit);
            builder.sequence()
                .action(b => followUnit(_baseunit.Range))
                .action(b => attackUnit())
                .action(b => becomeIdle())
                .endComposite();

            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.AttackLocation);
            builder.sequence()
                .action(b => radarCheck(BaseUnit.UnitCommand.AttackLocation))
                .action(b => _pathmover.setTargetLocation(_baseunit.AttackLocation))
                .action(b => checkArrival())
                .action(b => becomeIdle())
                .endComposite();

            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.CaptureFlag);
            builder.sequence()
                .action(b => checkArriveOnRange(b._baseunit.TargetFlag.getPosition(), b._baseunit.TargetFlag.CaptureRange))
                .action(b => captureFlag())
                .action(b => becomeIdle())
                .endComposite();


            builder.endComposite();
            _tree = builder.build();
            _tree.updatePeriod = 0.03f;
        }

        private TaskStatus checkArriveOnRange(Vector2 target, float range)
        {
            float distance = Vector2.Distance(_baseunit.transform.position, target);
            if (distance <= range)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }

        private TaskStatus captureFlag()
        {
            if (_baseunit.TargetFlag.Capturer == _baseunit.UnitPlayer)
            {
                return TaskStatus.Success;
            } else
            {
                _baseunit.TargetFlag.CapturingBaseUnit = _baseunit;
                return TaskStatus.Failure;
            }
        }

        private TaskStatus radarCheck(BaseUnit.UnitCommand returnCommand = BaseUnit.UnitCommand.None)
        {
            BaseUnit enemy = _baseunit.Radar.detectEnemyInArea();
            if (enemy != null)
            {
                Player p = enemy.UnitPlayer;
                Console.WriteLine("Enemy Detected!");
                _baseunit.attackUnit(enemy);
                if (returnCommand != BaseUnit.UnitCommand.None)
                {
                    _commandStack.Push(returnCommand);
                }

                return TaskStatus.Failure;
            }

            return TaskStatus.Success;
        }

        private TaskStatus attackUnit()
        {
            // if killed target:
            if (_baseunit.TargetUnit == null)
            {
                return TaskStatus.Success;
            }

            if (_isAttacking) { return TaskStatus.Failure; }
            else
            {
                if (!_isAttacking) _baseunit.executeAttack();
                _isAttacking = true;
                _attackTimer.Start();
            }

            return TaskStatus.Failure;
        }

        private TaskStatus followUnit(float followDistance)
        {
            if (_baseunit.TargetUnit == null)
            {
                _baseunit.ActiveCommand = BaseUnit.UnitCommand.Idle;
                return TaskStatus.Success;
            }

            Point diff = _baseunit.getTilePosition() - _baseunit.TargetUnit.getTilePosition();
            // need to account for the size of the collider, otherwise if collider is big unit never 
            // thinks it reaches it.
            float size = _baseunit.TargetUnit.colliders.getCollider<Collider>().bounds.width / 10;
            float distance = (float)Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) - size;

            if (distance <= followDistance)
            {
                _pathmover.stopMoving();
                return TaskStatus.Success;
            } else if (!_pathmover.HasArrived)
            {
                // AI will go to where it is suppose to go first before switching to follow the unit
                return TaskStatus.Running;
            }

            bool canGo = _pathmover.setTargetLocation(_baseunit.TargetUnit.transform.position.ToPoint());

            if (!canGo)
            {
                _baseunit.ActiveCommand = BaseUnit.UnitCommand.Idle;
                return TaskStatus.Failure;
            }

            return TaskStatus.Running;
        }

        private TaskStatus becomeIdle()
        {
            if (_commandStack.Count > 0)
            {
                _baseunit.ActiveCommand = _commandStack.Pop();
                return TaskStatus.Success;
            } else
            {
                _baseunit.ActiveCommand = BaseUnit.UnitCommand.Idle;
            }

            if (!_pathmover.HasArrived)
            {
                _pathmover.stopMoving();
            }

            _baseunit.playAnimation(BaseUnit.Animation.Idle);
            return TaskStatus.Success;
        }

        private TaskStatus checkArrival()
        {
            if (_pathmover.HasArrived)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            buildTree();
        }


        private void _attackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _isAttacking = false;
            _attackTimer.Stop();
        }

        void IUpdatable.update()
        {
            if (_tree != null)
                _tree.tick();
        }
    }
}
