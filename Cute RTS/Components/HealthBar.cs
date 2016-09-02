﻿using Cute_RTS.Scenes;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Components
{
    class HealthBar : RenderableComponent
    {
        public override float width { get { return 1000; } }
        public override float height { get { return 1000; } }
        public Vector2 PositionOffset { get; set; } = Vector2.Zero;

        private ProgressBar _healthbar;

        public HealthBar(Attackable attackable)
        {   
            attackable.OnHealthChange += _attackable_OnHealthChange;
            _healthbar = new ProgressBar(0, 1, 0.05f, false, ProgressBarStyle.create(Color.Green, Color.Red));
            _healthbar.setValue(attackable.HealthPercentage);
            _healthbar.setWidth(64);
        }

        public void setHealthbarWidth(int width)
        {
            _healthbar.setWidth(width);
        }

        private void _attackable_OnHealthChange(float healthpercentage)
        {
            _healthbar.setValue(healthpercentage);
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();

            ((BaseScene)entity.scene).canvas.stage.addElement(_healthbar);
        }

        public override void onRemovedFromEntity()
        {
            base.onRemovedFromEntity();

            _healthbar.remove();
        }

        public override void render(Graphics graphics, Camera camera)
        {
            _healthbar.setPosition(
                entity.transform.position.X - PositionOffset.X,
                entity.transform.position.Y - PositionOffset.Y); 
        }
    }
}
