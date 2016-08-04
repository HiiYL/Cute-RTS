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
    class UnitBehaviorTree : Component
    {
        public float UpdateInterval { get; set; } = 0.03f;
        private BehaviorTree<UnitBehaviorTree> _tree;
        private BaseUnit _baseunit;
        private PathMover _pathmover;
        private bool _isAttacking = false;
        private Timer _attackTimer;

        public UnitBehaviorTree(BaseUnit bu, PathMover pm)
        {
            _attackTimer = new Timer(1000);
            _attackTimer.Elapsed += _attackTimer_Elapsed;
            _baseunit = bu;
            _pathmover = pm;
        }

        private void buildTree()
        {
            var builder = BehaviorTreeBuilder<UnitBehaviorTree>.begin(this);
            builder.selector(AbortTypes.Self);

            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.Idle);
            builder.action(b => becomeIdle());


            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.GoTo);
            builder.selector()
                .action(b => checkArrival())
                .endComposite();

            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.Follow);
            builder.sequence()
                .action(b => followUnit(3))
                .action(b => becomeIdle())
                .endComposite();

            builder.conditionalDecorator(b => b._baseunit.ActiveCommand == BaseUnit.UnitCommand.Attack);
            builder.sequence()
                .action(b => followUnit(_baseunit.Range))
                .action(b => attackUnit())
                .action(b => becomeIdle())
                .endComposite();


            builder.endComposite();
            _tree = builder.build();
        }

        private TaskStatus attackUnit()
        {
            // if killed target:
            if (_baseunit.TargetUnit == null)
            {
                _baseunit.ActiveCommand = BaseUnit.UnitCommand.Idle;
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
            float distance = (float) Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
            if (distance <= followDistance)
            {
                _pathmover.stopMoving();
                return TaskStatus.Success;
            } else if (!_pathmover.HasArrived)
            {
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
            if (!_pathmover.HasArrived)
            {
                _pathmover.stopMoving();
            }

            _baseunit.playAnimation(BaseUnit.Animation.Idle);
            return TaskStatus.Success;
        }

        private TaskStatus checkArrival()
        {
            // if arrived
            //      send arrival event
            //      return taskstatus.success
            // if not arrived return taskstatus.fail

            if (_pathmover.HasArrived)
            {
                _baseunit.ActiveCommand = BaseUnit.UnitCommand.Idle;
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }

        public override void onAddedToEntity()
        {
            buildTree();
            enabled = true;
            RunTick();

            base.onAddedToEntity();
        }

        public override void onRemovedFromEntity()
        {
            enabled = false;
            base.onRemovedFromEntity();
        }

        public override void onEnabled()
        {
            RunTick();
            base.onEnabled();
        }

        private void RunTick()
        {
            if (!enabled) return;

            _tree.tick();

            Core.schedule(UpdateInterval, timer => RunTick());
        }

        private void _attackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _isAttacking = false;
            _attackTimer.Stop();
        }
    }
}
