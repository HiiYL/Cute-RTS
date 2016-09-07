using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Cute_RTS.Structures
{
    class CaptureFlag : Entity
    {
        public Player Capturer { get; set; }
        public BaseUnit CapturingBaseUnit
        {
            get { return _capturingBaseUnit; }
            set
            {
                // you can only capture if no one is capturing
                if (_capturingBaseUnit == null
                    && value.UnitPlayer != Capturer
                    && value.ActiveCommand == BaseUnit.UnitCommand.CaptureFlag)
                {
                    // can only capture if within capture range
                    float distance = Vector2.Distance(value.transform.position, getPosition());
                    if (distance <= CaptureRange)
                    {
                        _capturingBaseUnit = value;
                        _captureTimer.Start();
                    }
                }
            }
        }

        public float DeflagProgress { get { return _deflagProgress; } }
        public float CaptureProgress { get { return _captureProgress; } }
        public Selectable Select { get { return _selectable; } }
        public float CaptureRange { get; set; } = 70;
        public float CaptureDuration { get; set; } = 5000; // miliseconds
        public int GoldIncrease { get; set; } = 10;

        private BoxCollider _collider;
        private Selectable _selectable;
        private Sprite _flagTex;
        private Text _displayText;
        private BaseUnit _capturingBaseUnit;
        private Timer _captureTimer;
        private Timer _goldTimer;
        private const float _UPDATE_INTERVAL = 250;
        private const float _GOLD_INTERVAL = 2000;
        private float _captureProgress = 0;
        private float _deflagProgress = 0;

        public CaptureFlag(Texture2D tex, Texture2D selectTex)
        {
            _captureTimer = new Timer(_UPDATE_INTERVAL); // update every quarter second.
            _captureTimer.Elapsed += _captureTimer_Elapsed;
            _goldTimer = new Timer(_GOLD_INTERVAL);
            _goldTimer.Elapsed += _goldTimer_Elapsed;

            var textureOffset = new Vector2(33, -5);

            _flagTex = new Sprite(tex);
            // offset to set pitch point of flag as origin
            _flagTex.setLocalOffset(textureOffset);
            var s = new Sprite(selectTex);
            s.setLocalOffset(textureOffset);
            _selectable = new Selectable(s);
            _selectable.setSelectionColor(Color.Yellow);

            _collider = new BoxCollider(32, 32);
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)RTSCollisionLayer.Units);
            colliders.add(_collider);

            _displayText = new Text(Graphics.instance.bitmapFont, "", new Vector2(5, -40), Color.LightGoldenrodYellow);
            //_displayText.setText("DIE DIE DIE!");
            _displayText.setRenderLayer(-10);

            addComponent(_flagTex);
            addComponent(_selectable);
            addComponent(_displayText);
        }

        private void _goldTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Capturer == null)
            {
                _goldTimer.Stop();
                return;
            }

            if (_capturingBaseUnit != null) return; // no gold when an enemy is capturing your flag

            Capturer.Gold += 10;
            _displayText.setText(String.Format("+{0} Gold", GoldIncrease));
            Core.schedule(0.5f, t => { _displayText.setText(""); });
        }

        private void _captureTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // capture becomes invalid when unit dies or its active command is not capture flag
            if (_capturingBaseUnit == null
                || _capturingBaseUnit.ActiveCommand != BaseUnit.UnitCommand.CaptureFlag)
            {
                endCapture();
                return;
            }

            // update capture progress
            float percentageIncrease = _UPDATE_INTERVAL / CaptureDuration;
            _captureProgress += percentageIncrease;
            _displayText.setText(String.Format("{0} : {1:P0}", _capturingBaseUnit.UnitPlayer.Name, _captureProgress));

            // capture complete!
            if (_captureProgress >= 1)
            {
                Capturer = _capturingBaseUnit.UnitPlayer;
                _flagTex.setColor(Capturer.PlayerColor);
                _goldTimer.Start(); // let the gold come in!

                endCapture();
            }
        }

        private void endCapture()
        {
            _capturingBaseUnit = null;
            _captureProgress = 0;
            _displayText.setText("");
            _captureTimer.Stop();
        }

        public Vector2 getPosition()
        {
            return _collider.absolutePosition;
        }

    }
}
