using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Diggy_Hole
{
    internal class PlayerCharacters : MovingAnimated2D
    {
        public Vector2 SourcePoint
        { get { return _sourcePoint; } }

        public Vector2 Velocity
        { get { return _velocity; } }

        public Rectangle Hitbox
        { get { return _hitbox; } }

        public ActionState ActionState
        { get { return _actionState; } }

        private ActionState _actionState, _oldActionState;
        private Items _oldItems;
        private GamePadState _oldPad;

        private float _maxAirSpeed, _maxGroundSpeed,_groundDrag, _airDrag, _gravity, _airTimeCounter;
        private Vector2 _sourcePoint, _oldSourcePoint;
        private int _numberOfFrames, _frameCounter;
        private Rectangle _handSourceRectangle, _hitbox, _oldHitbox, _activeLadderHitBox, _baseLightBox, _headLightBox;
        private bool _facingRight, _dropThrough, _pressingInteract;
        private Texture2D _baseLight, _headLight;
        private SoundEffectInstance _pickaxeBlow, _shovel, _walking;

        public PlayerCharacters(
            Texture2D baseSprite, 
            Texture2D baseLight, 
            Texture2D headLight,
            SoundEffect pickaxeBlow,
            SoundEffect shovel,
            SoundEffect walking,
            float fps, 
            Rectangle SrcRect, 
            Rectangle rect, 
            Items Items) 
            : 
            base(
                baseSprite, 
                fps, 
                SrcRect, 
                rect)
        {
            _texture = baseSprite;
            _baseLight = baseLight;
            _headLight = headLight;
            _pickaxeBlow = pickaxeBlow.CreateInstance(); _pickaxeBlow.Volume = 0.50f;
            _shovel = shovel.CreateInstance(); _shovel.Volume = 0.65f;
            _walking = walking.CreateInstance(); _walking.Volume = 0.10f;
            _rectangle = rect;
            _sourceRectangle = SrcRect;
            _framesPerSecond = fps;

            _actionState = ActionState.Idle;
            _handSourceRectangle = new Rectangle(SrcRect.X, SrcRect.Y + SrcRect.Height, SrcRect.Width, SrcRect.Height);
            _sourcePoint = new Vector2(_rectangle.X + (_rectangle.Width / 2), _rectangle.Y + (_rectangle.Height / 2));
            _hitbox = new Rectangle(_rectangle.X + 64, _rectangle.Y + 32, 120, 184);
            _baseLightBox = new Rectangle((int)_sourcePoint.X - 256, (int)_sourcePoint.Y - 256, 512, 512);
            _headLightBox = new Rectangle((int)_sourcePoint.X, (int)_sourcePoint.Y - 225, 783, 332);

            _maxAirSpeed = 20f;
            _maxGroundSpeed = 10f;
            _groundDrag = 0.65f;
            _airDrag = 0.95f;
            _gravity = 1f;

            _facingRight = true;

            switchActionState();
            switchItems(Items);
        }

        public void updateme(KeyboardState kb, KeyboardState oldKb,MouseState cm, GamePadState pad, GameTime gt, Rectangle[,] eHB, Items Items)
        {
            //< Counters >
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            if (_actionState == ActionState.Airborne)
                _airTimeCounter += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;



            #region Character Movement
            // < Movement >
            #region Vertical Movement
            // ----- < Vertical Movement >
            // ----- // ----- < Character Jump >
            if ((pad.IsButtonDown(Buttons.A) || kb.IsKeyDown(Keys.Space)) &&
                (_oldPad.IsButtonUp(Buttons.A) && oldKb.IsKeyUp(Keys.Space)) &&
                _actionState != ActionState.Airborne && 
                _actionState != ActionState.Jumping && 
                _actionState != ActionState.StandUp && 
                _actionState != ActionState.LightLanding && 
                _actionState != ActionState.HeavyLanding)
            {
                if (_actionState != ActionState.Climbing)
                {
                    _velocity.Y = -16;
                    _actionState = ActionState.Jumping;
                }
                else
                {
                    _velocity.Y = 1;
                    _actionState = ActionState.Airborne;
                }
            }
            // ----- // ----- < Platform Drop Through >
            if (((pad.ThumbSticks.Left.Y < -0.9 &&
                pad.ThumbSticks.Left.X > -0.01 && 
                pad.ThumbSticks.Left.X < 0.01) ||
                kb.IsKeyDown(Keys.S) == true) && 
                _actionState != ActionState.Airborne &&
                _actionState != ActionState.Jumping &&
                _actionState != ActionState.StandUp &&
                _actionState != ActionState.LightLanding &&
                _actionState != ActionState.HeavyLanding &&
                _actionState != ActionState.Climbing
                )
            {
                _velocity.Y = 1;
                _actionState = ActionState.Airborne;
                _dropThrough = true;
            }
            if (((pad.ThumbSticks.Left.Y > -0.9 || pad.ThumbSticks.Left.X < -0.01 || pad.ThumbSticks.Left.X > 0.01) && kb.IsKeyUp(Keys.S)) && _actionState != ActionState.Climbing)
            {
                _dropThrough = false;
            }

            // ----- // ----- < Climbing >
            if (_actionState == ActionState.Climbing)
            {
                if (pad.ThumbSticks.Left.Y > 0.2 || pad.ThumbSticks.Left.Y < -0.2)
                    _velocity.Y = -pad.ThumbSticks.Left.Y * 6;
                else if (kb.IsKeyDown(Keys.W))
                    _velocity.Y = -6;
                else if (kb.IsKeyDown(Keys.S))
                    _velocity.Y = 6;
            }
            if (_actionState == ActionState.Climbing && pad.ThumbSticks.Left.Y < 0.2 && pad.ThumbSticks.Left.Y > -0.2 && kb.IsKeyUp(Keys.W) && kb.IsKeyUp(Keys.S))
            {
                _velocity.Y = 0;
            }
            if (_actionState == ActionState.Climbing && _pressingInteract == false && ((pad.Buttons.X == ButtonState.Pressed && _oldPad.Buttons.X != ButtonState.Pressed) || (kb.IsKeyDown(Keys.E) && oldKb.IsKeyUp(Keys.E))))
            {
                _actionState = ActionState.Airborne;
                _pressingInteract = true;
            }

            // ----- // ----- < Vertical Speed Clamp >
            if (_velocity.Y > _maxAirSpeed)
                _velocity.Y = _maxAirSpeed;
            else if (_velocity.Y < -_maxAirSpeed)
                _velocity.Y = -_maxAirSpeed;
            #endregion



            #region Horizontal Movement
            // ----- < Horizontal Movement >
            if (_actionState != ActionState.Airborne &&
                _actionState != ActionState.StandUp &&
                _actionState != ActionState.LightLanding &&
                _actionState != ActionState.HeavyLanding &&
                _actionState != ActionState.PickaxeSwing &&
                _actionState != ActionState.Shovel &&
                _actionState != ActionState.Climbing)
            {
                if (pad.IsConnected)
                {
                    _velocity.X += pad.ThumbSticks.Left.X * 3;
                }

                if (kb.IsKeyDown(Keys.A))
                {
                    _velocity.X -= 3;
                }
                else if (kb.IsKeyDown(Keys.D))
                {
                    _velocity.X += 3;
                }
            }
            if (_actionState == ActionState.Airborne && pad.IsConnected)
            {
                _velocity.X += pad.ThumbSticks.Left.X / 4;
            }
            // ----- < Horizontal Speed Clamp > 
            if (_velocity.X > _maxGroundSpeed) 
                _velocity.X = _maxGroundSpeed;
            else if (_velocity.X < -_maxGroundSpeed) 
                _velocity.X = -_maxGroundSpeed;
            #endregion
            #endregion



            // < Position Update >
            _position.X += _velocity.X;
            _position.Y += _velocity.Y;



            // < Hitbox & Position Updates >
            // ----- < Old Varibles >
            _oldSourcePoint = _sourcePoint;
            _oldHitbox = _hitbox;

            // ----- < Character Draw Rectangle >
            _rectangle = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                _rectangle.Width,
                _rectangle.Height);

            // ----- < Character Hitbox >
            _hitbox = new Rectangle(
                _rectangle.X + 64,
                _rectangle.Y + 16, 
                128, 
                200);

            // ----- < Source Point >
            _sourcePoint = new Vector2(
                _position.X + (_rectangle.Width / 2),
                _position.Y + (_rectangle.Height / 2));

            // ----- < Base Light Draw Rectangle >
            _baseLightBox = new Rectangle(
                (int)_sourcePoint.X - 256, 
                (int)_sourcePoint.Y - 256,
                _baseLightBox.Width,
                _baseLightBox.Height);



            




            // < Drag & Gravity >
            if (_actionState != ActionState.Airborne)
                _velocity.X *= _groundDrag;
            else if (_actionState == ActionState.Airborne) 
                _velocity.X *= _airDrag;


            if (_actionState == ActionState.Airborne)
                _velocity.Y += _gravity;
            else if (_actionState == ActionState.Jumping)
                _velocity.Y *= _airDrag;





            #region Character State Checks
            // < State Checks >
            // ----- < Direction Facing >
            if (_velocity.X > 0.5 && _actionState != ActionState.Climbing)
                _facingRight = true;
            else if (_velocity.X < -0.5 && _actionState != ActionState.Climbing)
                _facingRight = false;
            // ----- < Idle State Check >
            if ((_velocity.X > -0.2 && _velocity.X < 0.2) &&
                _actionState != ActionState.Airborne && 
                _actionState != ActionState.Jumping && 
                _actionState != ActionState.LightLanding &&
                _actionState != ActionState.HeavyLanding &&
                _actionState != ActionState.StandUp &&
                _actionState != ActionState.Climbing)
            {
                _actionState = ActionState.Idle;
            }
            // ----- < Running State Check >
            else if ((_velocity.X < -0.2 || _velocity.X > 0.2) && 
                _actionState != ActionState.Airborne && 
                _actionState != ActionState.Jumping &&
                _actionState != ActionState.LightLanding &&
                _actionState != ActionState.HeavyLanding &&
                _actionState != ActionState.StandUp &&
                _actionState != ActionState.Climbing)
            {
                _actionState = ActionState.Running;
            }

            // ----- < Swing Pickaxe >
            if ((pad.Triggers.Right > 0.8 || 
                cm.LeftButton == ButtonState.Pressed) && 
                (Items == Items.StonePickaxe ||
                Items == Items.StoneMultiTool ||
                Items == Items.IronPickaxe ||
                Items == Items.IronMultiTool ||
                Items == Items.GoldPickaxe) &&
                _actionState != ActionState.Airborne &&
                _actionState != ActionState.Jumping &&
                _actionState != ActionState.LightLanding &&
                _actionState != ActionState.HeavyLanding &&
                _actionState != ActionState.StandUp &&
                _actionState != ActionState.Climbing)
            {
                _actionState = ActionState.PickaxeSwing;
            }
            // ----- < Shovel >
            if ((pad.Triggers.Right > 0.8 ||
                cm.LeftButton == ButtonState.Pressed) &&
                (Items == Items.StoneShovel ||
                Items == Items.IronShovel ||
                Items == Items.GoldShovel) &&
                _actionState != ActionState.Airborne &&
                _actionState != ActionState.Jumping &&
                _actionState != ActionState.LightLanding &&
                _actionState != ActionState.HeavyLanding &&
                _actionState != ActionState.StandUp &&
                _actionState != ActionState.Climbing)
            {
                _actionState = ActionState.Shovel;
            }

            // ----- < Head Light Draw Rectangle >
            if (_facingRight == true)
            {
                _headLightBox = new Rectangle(
                    (int)_sourcePoint.X,
                    (int)_sourcePoint.Y - 225,
                    _headLightBox.Width,
                    _headLightBox.Height);
            }
            else
            {
                _headLightBox = new Rectangle(
                    (int)_sourcePoint.X - _headLightBox.Width,
                    (int)_sourcePoint.Y - 225,
                    _headLightBox.Width,
                    _headLightBox.Height);
            }
            #endregion





            #region Collision Checker
            // < Collision Checker >
            for (int i = 0; i < 400; i++)
            {
                // ----- < Wall Collider >
                if (_hitbox.Intersects(eHB[2, i]))
                {
                    _velocity.X = 0;

                    bool leftSide;
                    if (_hitbox.Center.X < eHB[2, i].Center.X)
                        leftSide = true;
                    else
                        leftSide = false;

                    if (leftSide)
                    {
                        _position.X = eHB[2, i].Left - (_rectangle.Width - (_rectangle.Right - _hitbox.Right));
                    }
                    else
                    {
                        _position.X = eHB[2, i].Right - (_hitbox.Left - _rectangle.Left) + 1;
                    }

                    if (_actionState == ActionState.Running)
                        _actionState = ActionState.Idle;
                    else if (_actionState == ActionState.Jumping || _actionState == ActionState.Airborne)
                        _actionState = ActionState.Airborne;
                    break;
                }
                // ----- < Cave-In Rubble Wall Collider >
                if (_hitbox.Intersects(eHB[13, i]))
                {
                    _velocity.X = 0;

                    bool leftSide;
                    if (_hitbox.Center.X < eHB[13, i].Center.X)
                        leftSide = true;
                    else
                        leftSide = false;

                    if (leftSide)
                    {
                        _position.X = eHB[13, i].Left - (_rectangle.Width - (_rectangle.Right - _hitbox.Right));
                    }
                    else
                    {
                        _position.X = eHB[13, i].Right - (_hitbox.Left - _rectangle.Left) + 1;
                    }

                    if (_actionState == ActionState.Running)
                        _actionState = ActionState.Idle;
                    else if (_actionState == ActionState.Jumping || _actionState == ActionState.Airborne)
                        _actionState = ActionState.Airborne;
                    break;
                }
                // ----- < Floor Collider >
                if (_hitbox.Intersects(eHB[0, i]) && _hitbox.Top < eHB[0, i].Top && _oldHitbox.Left < eHB[0, i].Right && _oldHitbox.Right > eHB[0, i].Left)
                {
                    if (_airTimeCounter < 1)
                        _actionState = ActionState.Idle;
                    else if (_airTimeCounter > 1 && _airTimeCounter < 2)
                        _actionState = ActionState.LightLanding;
                    else if (_airTimeCounter > 2)
                        _actionState = ActionState.HeavyLanding;

                    _airTimeCounter = 0;
                    _velocity.Y = 0;
                    _position.Y = (eHB[0, i].Y - (_rectangle.Height - 40));
                    break;
                }
                // ----- < Ceiling Collider >
                if (_hitbox.Intersects(eHB[1, i]) && _hitbox.Top > eHB[1, i].Top && _oldHitbox.Left < eHB[1, i].Right && _oldHitbox.Right > eHB[1, i].Left)
                {
                    _velocity.Y = 0;
                    _position.Y = (eHB[1, i].Bottom - (_hitbox.Top - _rectangle.Top));
                    _actionState = ActionState.Airborne;
                    break;
                }
                // ----- < Platform Collider >
                if (_hitbox.Intersects(eHB[3, i]) && _hitbox.Bottom < eHB[3, i].Top + 10 && _oldHitbox.Left < eHB[3, i].Right && _oldHitbox.Right > eHB[3, i].Left && !_dropThrough && _actionState != ActionState.Climbing)
                {
                    _actionState = ActionState.Idle;
                    _velocity.Y = 0;
                    _position.Y = (eHB[3, i].Y - (_rectangle.Height - 40));
                    break;
                }
                // ----- < Ladder Collider > 
                if (eHB[4, i].Contains(_sourcePoint) &&
                _pressingInteract == false &&
                ((pad.Buttons.X == ButtonState.Pressed &&
                _oldPad.Buttons.X == ButtonState.Released) ||
                (kb.IsKeyDown(Keys.E) &&
                oldKb.IsKeyUp(Keys.E))))
                {
                    _actionState = ActionState.Climbing;
                    _velocity.Y = 0;
                    _activeLadderHitBox = eHB[4, i];

                    if (eHB[4, i].Center.X < _sourcePoint.X)
                        _facingRight = false;
                    else
                        _facingRight = true;
                    break;
                }
                // ----- < Coal Seam Face-Check >
                if (eHB[6, i].Intersects(_hitbox) && _actionState == ActionState.PickaxeSwing)
                {
                    if (eHB[6, i].Center.X < _sourcePoint.X)
                        _facingRight = false;
                    else
                        _facingRight = true;
                }
                // ----- < Copper Seam Face-Check >
                if (eHB[7, i].Intersects(_hitbox) && _actionState == ActionState.PickaxeSwing)
                {
                    if (eHB[7, i].Center.X < _sourcePoint.X)
                        _facingRight = false;
                    else
                        _facingRight = true;
                }
                // ----- < Iron Seam Face-Check >
                if (eHB[8, i].Intersects(_hitbox) && _actionState == ActionState.PickaxeSwing)
                {
                    if (eHB[8, i].Center.X < _sourcePoint.X)
                        _facingRight = false;
                    else
                        _facingRight = true;
                }
                // ----- < Gold Seam Face-Check >
                if (eHB[9, i].Intersects(_hitbox) && _actionState == ActionState.PickaxeSwing)
                {
                    if (eHB[9, i].Center.X < _sourcePoint.X)
                        _facingRight = false;
                    else
                        _facingRight = true;
                }
                // ----- < Ladder Cap Collider >
                if (eHB[11, i].Contains(_sourcePoint) && _actionState == ActionState.Climbing)
                {
                    _actionState = ActionState.Airborne;
                }
            }
            #endregion





            #region Animation Update
            // < Animation Updates > this statement runs through every update cycle checking if its ready to change the animation frame on the object
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            if (_updateTrigger >= 1)
            {
                _updateTrigger = 0;

                if (_actionState != ActionState.Climbing && _actionState != ActionState.Climbing)
                    _frameCounter++;
                else if (_actionState == ActionState.Climbing && (pad.ThumbSticks.Left.Y > 0.2 || kb.IsKeyDown(Keys.W)))
                {
                    _frameCounter++;
                    _sourceRectangle.X += _sourceRectangle.Width;
                    _handSourceRectangle.X += _handSourceRectangle.Width;
                }
                else if (_actionState == ActionState.Climbing && (pad.ThumbSticks.Left.Y < -0.2 || kb.IsKeyDown(Keys.S)))
                {
                    _frameCounter--;
                    _sourceRectangle.X -= _sourceRectangle.Width;
                    _handSourceRectangle.X -= _handSourceRectangle.Width;
                }


                if (_actionState != ActionState.Airborne && _actionState != ActionState.Climbing)
                {
                    _sourceRectangle.X += _sourceRectangle.Width;
                    _handSourceRectangle.X += _handSourceRectangle.Width;
                }



                // ----- < Animation Reset > once the animation reaches the end of the sheet it will be set back to the start
                if (_frameCounter >= _numberOfFrames)
                {
                    _frameCounter = 0;

                    if (_actionState != ActionState.Airborne && 
                        _actionState != ActionState.LightLanding && 
                        _actionState != ActionState.HeavyLanding && 
                        _actionState != ActionState.StandUp)
                    {
                        _sourceRectangle.X = 0;
                        _handSourceRectangle.X = 0;
                    }
                    if (_actionState == ActionState.Jumping)
                        _actionState = ActionState.Airborne;

                    if (_actionState == ActionState.StandUp)
                    {
                        _actionState = ActionState.Idle;
                        _sourceRectangle.X = 0;
                        _handSourceRectangle.X = 0;
                    }
                    else if (_actionState == ActionState.LightLanding)
                    {
                        _actionState = ActionState.Idle;
                        _sourceRectangle.X = 0;
                        _handSourceRectangle.X = 0;
                    }
                    else if(_actionState == ActionState.HeavyLanding)
                    {
                        _actionState = ActionState.StandUp;
                        _sourceRectangle.X = 0;
                        _handSourceRectangle.X = 0;
                    }
                }
                else if (_sourceRectangle.X < 0)
                {
                    _frameCounter = _numberOfFrames;
                    _sourceRectangle.X = _rectangle.Width * 7;
                    _handSourceRectangle.X = _rectangle.Width * 7;
                }
            }
            #endregion



            if (ActionState == ActionState.Shovel && _frameCounter == 0)
                _shovel.Play();
            if (ActionState == ActionState.PickaxeSwing && _frameCounter == 1)
                _pickaxeBlow.Play();
            


            if (ActionState == ActionState.Running && _frameCounter == 1)
            {
                _walking.Play();
            }
            if (ActionState == ActionState.Running && _frameCounter == 7)
            {
                _walking.Stop();
            }
            if (ActionState != ActionState.Running)
            {
                _walking.Stop();
            }


            // < Animation State Check >
            if (_oldActionState != _actionState)
            {
                switchActionState();
                switchItems(Items);
            }
            if (_oldItems != Items)
                switchItems(Items);





            // < Button Pressed Check >
            if (pad.Buttons.X == ButtonState.Released && kb.IsKeyUp(Keys.E))
                _pressingInteract = false;





            // < Past Tick Varible Update >
            _oldActionState = _actionState;
            _oldItems = Items;
            _oldPad = pad;
        }

        public override void drawme(SpriteBatch sb)
        {
            if (_facingRight == true)
            {
                sb.Draw(_texture, _rectangle, _sourceRectangle, Color.White);
                sb.Draw(_texture, _rectangle, _handSourceRectangle, Color.White);
            }
            else
            {
                sb.Draw(_texture, _rectangle, _sourceRectangle, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                sb.Draw(_texture, _rectangle, _handSourceRectangle, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            }
        }

        public void drawMask(SpriteBatch sb)
        {
            sb.Draw(_baseLight, _baseLightBox, Color.White);

            if (_facingRight == true)
                sb.Draw(_headLight, _headLightBox, Color.White);
            else
                sb.Draw(_headLight, _headLightBox, new Rectangle(0, 0, 783, 332), Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 1); ;
        }



        private void switchActionState()
        {
            switch(_actionState)
            {
                case ActionState.Idle:
                    _numberOfFrames = 8;
                    _sourceRectangle = new Rectangle(
                        0,
                        0,
                        _rectangle.Width,
                        _rectangle.Height);
                    _airTimeCounter = 0;
                    break;


                case ActionState.Running:
                    _numberOfFrames = 8;
                    _sourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 11,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.Jumping:
                    _numberOfFrames = 6;
                    _sourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 22,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.Airborne:
                    _numberOfFrames = 1;
                    _sourceRectangle = new Rectangle(
                        _rectangle.Width * 5,
                        _rectangle.Height * 22,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.LightLanding:
                    _numberOfFrames = 4;
                    _sourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 33,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.HeavyLanding:
                    _numberOfFrames = 6;
                    _sourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 44,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.StandUp:
                    _numberOfFrames = 6;
                    _sourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 55,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.PickaxeSwing:
                    _numberOfFrames = 6;
                    _sourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 66,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.Shovel:
                    _numberOfFrames = 8;
                    _sourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 72,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.Climbing:
                    _numberOfFrames = 8;
                    _sourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 76,
                        _rectangle.Width,
                        _rectangle.Height);
                    _airTimeCounter = 0;
                    break;
            }
            _frameCounter = 0;
        }

        private void switchItems(Items Items)
        {
            switch(Items)
            {
                case Items.Bare:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case Items.StonePickaxe:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 3,
                        _rectangle.Width,
                        _rectangle.Height);

                    if (_actionState == ActionState.PickaxeSwing)
                    {
                        _handSourceRectangle = new Rectangle(
                            0,
                            _handSourceRectangle.Y + (_rectangle.Height * 64),
                            _rectangle.Width,
                            _rectangle.Height);
                    }
                    break;


                case Items.StoneShovel:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 4,
                        _rectangle.Width,
                        _rectangle.Height);

                    if (_actionState == ActionState.Shovel)
                    {
                        _handSourceRectangle = new Rectangle(
                            0,
                            _rectangle.Height * 73,
                            _rectangle.Width,
                            _rectangle.Height);
                    }
                    break;


                case Items.StoneMultiTool:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 5,
                        _rectangle.Width,
                        _rectangle.Height);

                    if (_actionState == ActionState.PickaxeSwing)
                    {
                        _handSourceRectangle = new Rectangle(
                            0,
                            _handSourceRectangle.Y + (_rectangle.Height * 63),
                            _rectangle.Width,
                            _rectangle.Height);
                    }
                    break;


                case Items.IronPickaxe:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 6,
                        _rectangle.Width,
                        _rectangle.Height);

                    if (_actionState == ActionState.PickaxeSwing)
                    {
                        _handSourceRectangle = new Rectangle(
                            0,
                            _handSourceRectangle.Y + (_rectangle.Height * 63),
                            _rectangle.Width,
                            _rectangle.Height);
                    }
                    break;


                case Items.IronShovel:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 7,
                        _rectangle.Width,
                        _rectangle.Height);

                    if (_actionState == ActionState.Shovel)
                    {
                        _handSourceRectangle = new Rectangle(
                            0,
                            _rectangle.Height * 74,
                            _rectangle.Width,
                            _rectangle.Height);
                    }
                    break;


                case Items.IronMultiTool:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 8,
                        _rectangle.Width,
                        _rectangle.Height);

                    if (_actionState == ActionState.PickaxeSwing)
                    {
                        _handSourceRectangle = new Rectangle(
                            0,
                            _handSourceRectangle.Y + (_rectangle.Height * 62),
                            _rectangle.Width,
                            _rectangle.Height);
                    }
                    break;


                case Items.GoldPickaxe:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 9,
                        _rectangle.Width,
                        _rectangle.Height);

                    if (_actionState == ActionState.PickaxeSwing)
                    {
                        _handSourceRectangle = new Rectangle(
                         0,
                         _handSourceRectangle.Y + (_rectangle.Height * 62),
                         _rectangle.Width,
                         _rectangle.Height);
                    }
                    break;


                case Items.GoldShovel:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 10,
                        _rectangle.Width,
                        _rectangle.Height);

                    if (_actionState == ActionState.Shovel)
                    {
                        _handSourceRectangle = new Rectangle(
                            0,
                            _rectangle.Height * 75,
                            _rectangle.Width,
                            _rectangle.Height);
                    }
                    break;


                default:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;
            }

            switch(_actionState)
            {
                case ActionState.Idle:
                    _handSourceRectangle = new Rectangle(
                        256 * _frameCounter,
                        _handSourceRectangle.Y,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.Running:
                    _handSourceRectangle = new Rectangle(
                        256 * _frameCounter,
                        _handSourceRectangle.Y + (_rectangle.Height * 11),
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.Jumping:
                    _handSourceRectangle = new Rectangle(
                        256 * _frameCounter,
                        _handSourceRectangle.Y + (_rectangle.Height * 22),
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.Airborne:
                    _handSourceRectangle = new Rectangle(
                        1280,
                        _handSourceRectangle.Y + (_rectangle.Height * 22),
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.LightLanding:
                    _handSourceRectangle = new Rectangle(
                        256 * _frameCounter,
                        _handSourceRectangle.Y +(_rectangle.Height * 33),
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.HeavyLanding:
                    _handSourceRectangle = new Rectangle(
                        256 * _frameCounter,
                        _handSourceRectangle.Y + (_rectangle.Height * 44),
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.StandUp:
                    _handSourceRectangle = new Rectangle(
                        256 * _frameCounter,
                        _handSourceRectangle.Y + (_rectangle.Height * 55),
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case ActionState.Climbing:
                    _handSourceRectangle = new Rectangle(
                        0,
                        _rectangle.Height * 77,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;
            }
        }
    }
    enum ActionState
    {
        Idle,
        Running,
        Jumping,
        Airborne,
        LightLanding,
        HeavyLanding,
        Dying,
        Dead,
        StandUp,
        Climbing,
        PickaxeSwing,
        Shovel
    }





    class PlayerPointer : StaticGraphic
    {
        private SpriteFont _font;
        private Texture2D _pointerTex;
        private String _flashString, _interactString;
        private String[] _itemString;
        private Color _colour, _flashColour;
        private Color[] _itemColour;
        private Rectangle _pointerSquare, _flashSquare, _slotSource, _flashItemSource;
        private Rectangle[] _itemSource;
        private Items[] _oldHands = new Items[3];
        private MouseState _oldMouse;
        private float _transparenty, _flashTransparety, _playerNum, _timer, _interactTransparenty;
        private bool _pointerPress;

        public PlayerPointer(Rectangle rectPos, Texture2D txr, Texture2D pointerTex, SpriteFont font, Color playerColour, float playerNum) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _texture = txr;
            _pointerTex = pointerTex;
            _font = font;
            _colour = playerColour;
            _playerNum = playerNum;

            _itemSource = new Rectangle[3];
            _itemString = new String[3];
            _itemColour = new Color[3];

            _pointerSquare = new Rectangle(0, 0, (int)(136 / 2), (int)(104 / 2));
            _slotSource = new Rectangle(0, 0, 256, 256);

            _flashString = "";

            _interactString = "";

            _itemString[0] = "";
            _itemString[1] = "";
            _itemString[2] = "";

            _oldHands[0] = Items.Bare;
            _oldHands[1] = Items.Bare;
            _oldHands[2] = Items.Bare;
        }

        public void updateme(GameTime gt, Rectangle playerArea, Items[] playerHands, KeyboardState kb, GamePadState pad, MouseState mouse, Rectangle[,] eHB, ActionState playerActionState, bool kbUsed, bool padUsed, int activeSlot)
        {
            _timer += (float)gt.ElapsedGameTime.TotalSeconds;
            _transparenty -= (float)gt.ElapsedGameTime.TotalSeconds;
            _interactTransparenty -= (float)gt.ElapsedGameTime.TotalSeconds;
            if (_timer > 2) _flashTransparety -= (float)gt.ElapsedGameTime.TotalSeconds;

            #region Draw Square Position Updates
            _rectangle = new Rectangle(
                playerArea.Center.X - ((_rectangle.Width + (_rectangle.Width / 2)) + 10),
                playerArea.Top - _rectangle.Height,
                _rectangle.Width,
                _rectangle.Height);

            _pointerSquare = new Rectangle(
                playerArea.Center.X - (_pointerSquare.Width / 2),
                playerArea.Bottom + 30,
                _pointerSquare.Width,
                _pointerSquare.Height);

            _flashSquare = new Rectangle(
                playerArea.Center.X - (_rectangle.Width / 2),
                playerArea.Top - _rectangle.Height,
                _rectangle.Width,
                _rectangle.Height);
            #endregion

            #region Hand Varibles Switchboard
            for (int i = 0; i < 3; i++)
            {
                if (playerHands[i] != _oldHands[i])
                {
                    switch (playerHands[i])
                    {
                        case Items.Bare:
                            _itemSource[i] = new Rectangle(426, 212, 32, 25);
                            _flashItemSource = new Rectangle(237, 277, 32, 25);
                            _flashString = "";
                            _itemString[i] = "";
                            _flashColour = Color.White;
                            _itemColour[i] = Color.White;
                            break;

                        case Items.GoldPickaxe:
                            _itemSource[i] = new Rectangle(256 * 1, 256 * 0, 256, 256);
                            _flashItemSource = new Rectangle(256 * 1, 256 * 0, 256, 256);
                            _flashString = "Almirs's Pickaxe";
                            _itemString[i] = "Almirs's Pickaxe";
                            _flashColour = Color.Gold;
                            _itemColour[i] = Color.Gold;
                            break;

                        case Items.GoldShovel:
                            _itemSource[i] = new Rectangle(256 * 2, 256 * 0, 256, 256);
                            _flashItemSource = new Rectangle(256 * 2, 256 * 0, 256, 256);
                            _flashString = "The Golden Spoon";
                            _itemString[i] = "The Golden Spoon";
                            _flashColour = Color.Gold;
                            _itemColour[i] = Color.Gold;
                            break;

                        case Items.IronPickaxe:
                            _itemSource[i] = new Rectangle(256 * 0, 256 * 1, 256, 256);
                            _flashItemSource = new Rectangle(256 * 0, 256 * 1, 256, 256);
                            _flashString = "Iron Pickaxe";
                            _itemString[i] = "Iron Pickaxe";
                            _flashColour = Color.Silver;
                            _itemColour[i] = Color.Silver;
                            break;

                        case Items.IronMultiTool:
                            _itemSource[i] = new Rectangle(256 * 1, 256 * 1, 256, 256);
                            _flashItemSource = new Rectangle(256 * 1, 256 * 1, 256, 256);
                            _flashString = "Iron Multi-Tool";
                            _itemString[i] = "Iron Multi-Tool";
                            _flashColour = Color.Silver;
                            _itemColour[i] = Color.Silver;
                            break;

                        case Items.IronShovel:
                            _itemSource[i] = new Rectangle(256 * 2, 256 * 1, 256, 256);
                            _flashItemSource = new Rectangle(256 * 2, 256 * 1, 256, 256);
                            _flashString = "Iron Shovel";
                            _itemString[i] = "Iron Shovel";
                            _flashColour = Color.Silver;
                            _itemColour[i] = Color.Silver;
                            break;

                        case Items.StonePickaxe:
                            _itemSource[i] = new Rectangle(256 * 0, 256 * 2, 256, 256);
                            _flashItemSource = new Rectangle(256 * 0, 256 * 2, 256, 256);
                            _flashString = "Stone Pickaxe";
                            _itemString[i] = "Stone Pickaxe";
                            _flashColour = Color.Gray;
                            _itemColour[i] = Color.Gray;
                            break;

                        case Items.StoneMultiTool:
                            _itemSource[i] = new Rectangle(256 * 1, 256 * 2, 256, 256);
                            _flashItemSource = new Rectangle(256 * 1, 256 * 2, 256, 256);
                            _flashString = "Stone Multi-Tool";
                            _itemString[i] = "Stone Multi-Tool";
                            _flashColour = Color.Gray;
                            _itemColour[i] = Color.Gray;
                            break;

                        case Items.StoneShovel:
                            _itemSource[i] = new Rectangle(256 * 2, 256 * 2, 256, 256);
                            _flashItemSource = new Rectangle(256 * 2, 256 * 2, 256, 256);
                            _flashString = "Stone Shovel";
                            _itemString[i] = "Stone Shovel";
                            _flashColour = Color.Gray;
                            _itemColour[i] = Color.Gray;
                            break;

                        case Items.Coal:
                            _itemSource[i] = new Rectangle(256 * 0, 256 * 3, 256, 256);
                            _flashItemSource = new Rectangle(256 * 0, 256 * 3, 256, 256);
                            _flashString = "Coal";
                            _itemString[i] = "Coal";
                            _flashColour = Color.White;
                            _itemColour[i] = Color.White;
                            break;

                        case Items.Copper:
                            _itemSource[i] = new Rectangle(256 * 1, 256 * 3, 256, 256);
                            _flashItemSource = new Rectangle(256 * 1, 256 * 3, 256, 256);
                            _flashString = "Copper";
                            _itemString[i] = "Copper";
                            _flashColour = new Color(216, 117, 52, 255);
                            _itemColour[i] = new Color(216, 117, 52, 255);
                            break;

                        case Items.Iron:
                            _itemSource[i] = new Rectangle(256 * 2, 256 * 3, 256, 256);
                            _flashItemSource = new Rectangle(256 * 2, 256 * 3, 256, 256);
                            _flashString = "Iron";
                            _itemString[i] = "Iron";
                            _flashColour = Color.Silver;
                            _itemColour[i] = Color.Silver;
                            break;

                        case Items.Gold:
                            _itemSource[i] = new Rectangle(256 * 0, 256 * 4, 256, 256);
                            _flashItemSource = new Rectangle(256 * 0, 256 * 4, 256, 256);
                            _flashString = "Gold";
                            _itemString[i] = "Gold";
                            _flashColour = Color.Gold;
                            _itemColour[i] = Color.Gold;
                            break;

                        default:
                            _itemSource[i] = new Rectangle(426, 212, 32, 25);
                            _flashItemSource = new Rectangle(426, 212, 32, 25);
                            _flashString = "";
                            _itemString[i] = "";
                            break;
                    }
                    _flashTransparety = 1f;
                    _transparenty = 0f;
                    _timer = 0;
                    _oldHands[i] = playerHands[i];
                }
            }
            #endregion

            #region Interact Pop-up
            for (int i = 0; i < 200; i++)
            {
                bool pickCheck = false;
                if (playerHands[activeSlot] == Items.StoneMultiTool ||
                    playerHands[activeSlot] == Items.StonePickaxe ||
                    playerHands[activeSlot] == Items.IronPickaxe ||
                    playerHands[activeSlot] == Items.IronMultiTool ||
                    playerHands[activeSlot] == Items.GoldPickaxe)
                {
                    pickCheck = true;
                }

                bool shovelCheck = false;
                if (playerHands[activeSlot] == Items.StoneMultiTool ||
                    playerHands[activeSlot] == Items.StoneShovel ||
                    playerHands[activeSlot] == Items.IronShovel ||
                    playerHands[activeSlot] == Items.IronMultiTool ||
                    playerHands[activeSlot] == Items.GoldShovel)
                {
                    shovelCheck = true;
                }

                bool oreCheck = false;
                for (int j = 0; j < 3; j++)
                {
                    if (playerHands[j] == Items.Coal || playerHands[j] == Items.Copper || playerHands[j] == Items.Iron || playerHands[j] == Items.Gold)
                    {
                        oreCheck = true;
                    }
                }

                
                if (eHB[4, i].Contains(playerArea.Center))
                {
                    if (playerActionState == ActionState.Climbing)
                    {
                        if (kbUsed == true) _interactString = "E to LET GO";
                        else if (padUsed == true) _interactString = "X to LET GO";
                    }
                    if (playerActionState != ActionState.Climbing)
                    {
                        if (kbUsed == true) _interactString = "E to CLIMB";
                        else if (padUsed == true) _interactString = "X to CLIMB";
                    }

                    _interactTransparenty = 1;
                }
                else if (playerArea.Intersects(eHB[6, i]) && pickCheck == true)
                {
                    if (kbUsed == true) _interactString = "LEFT-MOUSE to MINE coal";
                    else if (padUsed == true) _interactString = "RIGHT-TRIGGER to MINE coal";

                    _interactTransparenty = 1;
                }
                else if (playerArea.Intersects(eHB[7, i]) && pickCheck == true)
                {
                    if (kbUsed == true) _interactString = "LEFT-MOUSE to MINE copper";
                    else if (padUsed == true) _interactString = "RIGHT-TRIGGER to MINE copper";

                    _interactTransparenty = 1;
                }
                else if (playerArea.Intersects(eHB[8, i]) && pickCheck == true)
                {
                    if (kbUsed == true) _interactString = "LEFT-MOUSE to MINE iron";
                    else if (padUsed == true) _interactString = "RIGHT-TRIGGER to MINE iron";

                    _interactTransparenty = 1;
                }
                else if (playerArea.Intersects(eHB[9, i]) && pickCheck == true)
                {
                    if (kbUsed == true) _interactString = "LEFT-MOUSE to MINE gold";
                    else if (padUsed == true) _interactString = "RIGHT-TRIGGER to MINE gold";

                    _interactTransparenty = 1;
                }
                else if (playerArea.Intersects(eHB[10, i]) && oreCheck == true)
                {
                    if (kbUsed == true) _interactString = "E to deposit ore";
                    else if (padUsed == true) _interactString = "X to  deposit ore";

                    _interactTransparenty = 1;
                }
                else if (playerArea.Intersects(eHB[12, i]) && shovelCheck == true)
                {
                    if (kbUsed == true) _interactString = "LEFT-MOUSE to DIG rubble";
                    else if (padUsed == true) _interactString = "RIGHT-TRIGGER to DIG rubble";

                    _interactTransparenty = 1;
                }
            }
            #endregion

            #region Button Registers
            if (pad.Buttons.Y == ButtonState.Pressed || kb.IsKeyDown(Keys.Tab) || pad.Buttons.LeftShoulder == ButtonState.Pressed || pad.Buttons.RightShoulder == ButtonState.Pressed || _oldMouse.ScrollWheelValue != mouse.ScrollWheelValue)
            {
                _transparenty = 2;
                _flashTransparety = 0;
            }

            if (pad.Buttons.Y == ButtonState.Pressed || kb.IsKeyDown(Keys.Tab))
            {
                _pointerPress = true;
            }
            else if (_transparenty <= 0)
            {
                _pointerPress = false;
            }
            #endregion

            _oldMouse = mouse;
        }

        public void physicalItemInteractPromptFinder(Rectangle itemBox, Rectangle playerBox, Items item, Items[] playerHand, bool kbUsed, bool padUsed)
        {
            if (playerBox.Intersects(itemBox))
            {
                string itemName = "";

                switch (item)
                {
                    case Items.StoneMultiTool:
                        itemName = "stone multi-tool";
                        _interactTransparenty = 1;
                        break;

                    case Items.StonePickaxe:
                        itemName = "stone pickaxe";
                        _interactTransparenty = 1;
                        break;

                    case Items.StoneShovel:
                        itemName = "stone shovel";
                        _interactTransparenty = 1;
                        break;

                    case Items.IronMultiTool:
                        itemName = "iron multi-tool";
                        _interactTransparenty = 1;
                        break;

                    case Items.IronPickaxe:
                        itemName = "iron pickaxe";
                        _interactTransparenty = 1;
                        break;

                    case Items.IronShovel:
                        itemName = "iron shovel";
                        _interactTransparenty = 1;
                        break;

                    case Items.GoldPickaxe:
                        itemName = "Almirs's pickaxe";
                        _interactTransparenty = 1;
                        break;

                    case Items.GoldShovel:
                        itemName = "The Golden Spoon";
                        _interactTransparenty = 1;
                        break;

                    case Items.Coal:
                        itemName = "coal";
                        _interactTransparenty = 1;
                        break;

                    case Items.Copper:
                        itemName = "copper";
                        _interactTransparenty = 1;
                        break;

                    case Items.Iron:
                        itemName = "iron";
                        _interactTransparenty = 1;
                        break;

                    case Items.Gold:
                        itemName = "gold";
                        _interactTransparenty = 1;
                        break;
                }

                if (kbUsed == true) _interactString = "E to PICKUP " + itemName;
                else if (padUsed == true) _interactString = "X to PICKUP " + itemName;
            }
        }

        public void drawme(SpriteBatch sb, int activeSlot)
        {
            #region Hand Bar Draw
            for (int i = 0; i < 3; i++)
            {
                if (i != activeSlot)
                {
                    sb.Draw(_texture, new Rectangle(_rectangle.X + ((_rectangle.Width * i) + (10 * i)), _rectangle.Y, _rectangle.Width, _rectangle.Height), _slotSource, (Color.LightGray * 0.8f) * _transparenty);
                    sb.Draw(_texture, new Rectangle(_rectangle.X + ((_rectangle.Width * i) + (10 * i)), _rectangle.Y, _rectangle.Width, _rectangle.Height), _itemSource[i], Color.White * _transparenty);
                }
                if (i == activeSlot)
                {
                    sb.Draw(_texture, new Rectangle(_rectangle.X + ((_rectangle.Width * i) + (10 * i)), _rectangle.Y, _rectangle.Width, _rectangle.Height), _slotSource, (_colour * 0.8f) * _transparenty);
                    sb.Draw(_texture, new Rectangle(_rectangle.X + ((_rectangle.Width * i) + (10 * i)), _rectangle.Y, _rectangle.Width, _rectangle.Height), _itemSource[i], Color.White * _transparenty);

                    Vector2 itemTextSize = _font.MeasureString(_itemString[i]);
                    sb.DrawString(_font, _itemString[i], new Vector2((_flashSquare.Center.X - (itemTextSize.X / 2)) + 5, (_flashSquare.Top - itemTextSize.Y) + 5), Color.Black * _transparenty);
                    sb.DrawString(_font, _itemString[i], new Vector2(_flashSquare.Center.X - (itemTextSize.X / 2), _flashSquare.Top - itemTextSize.Y), _itemColour[i] * _transparenty);
                }
            }
            #endregion

            #region Item Flash Draw
            if (_flashItemSource != new Rectangle(237, 277, 32, 25))
            {
                sb.Draw(_texture, _flashSquare, _slotSource, (Color.White * 0.8f) * _flashTransparety);
                sb.Draw(_texture, _flashSquare, _flashItemSource, (Color.White * 0.8f) * _flashTransparety);

                Vector2 flashStringSize = _font.MeasureString(_flashString);
                sb.DrawString(_font, _flashString, new Vector2((_flashSquare.Center.X - (flashStringSize.X / 2)) + 5, (_flashSquare.Top - flashStringSize.Y) + 5), Color.Black * _flashTransparety);
                sb.DrawString(_font, _flashString, new Vector2(_flashSquare.Center.X - (flashStringSize.X / 2), _flashSquare.Top - flashStringSize.Y), _flashColour * _flashTransparety);
            }
            #endregion

            #region Player Pointer Draw
            if (_pointerPress == true)
            {
                sb.Draw(_pointerTex, _pointerSquare, _colour * _transparenty);

                Vector2 playertextSize = _font.MeasureString("Player " + _playerNum);
                sb.DrawString(_font, "Player " + _playerNum, new Vector2(_pointerSquare.Center.X - (playertextSize.X / 2) + 5, (_pointerSquare.Bottom + 5) + 5), Color.Black * _transparenty);
                sb.DrawString(_font, "Player " + _playerNum, new Vector2(_pointerSquare.Center.X - (playertextSize.X / 2), _pointerSquare.Bottom + 5), _colour * _transparenty);
            }
            #endregion

            #region Interact Prompt Draw
            if (_pointerPress == false)
            {
                Vector2 interactSize = _font.MeasureString(_interactString);
                sb.DrawString(_font, _interactString, new Vector2(_pointerSquare.Center.X - (interactSize.X / 2) + 5, (_pointerSquare.Bottom + 5) + 5), Color.Black * _interactTransparenty);
                sb.DrawString(_font, _interactString, new Vector2(_pointerSquare.Center.X - (interactSize.X / 2), _pointerSquare.Bottom + 5), _colour * _interactTransparenty);
            }
            #endregion
        }

        public void drawmeLevelSelect(SpriteBatch sb)
        {
            if (_pointerPress == true)
            {
                sb.Draw(_pointerTex, _pointerSquare, _colour * _transparenty);

                Vector2 playertextSize = _font.MeasureString("Player " + _playerNum);
                sb.DrawString(_font, "Player " + _playerNum, new Vector2(_pointerSquare.Center.X - (playertextSize.X / 2) + 5, (_pointerSquare.Bottom + 5) + 5), Color.Black * _transparenty);
                sb.DrawString(_font, "Player " + _playerNum, new Vector2(_pointerSquare.Center.X - (playertextSize.X / 2), _pointerSquare.Bottom + 5), _colour * _transparenty);
            }
            if (_pointerPress == false)
            {
                Vector2 interactSize = _font.MeasureString(_interactString);
                sb.DrawString(_font, _interactString, new Vector2(_pointerSquare.Center.X - (interactSize.X / 2) + 5, (_pointerSquare.Bottom + 5) + 5), Color.Black * _interactTransparenty);
                sb.DrawString(_font, _interactString, new Vector2(_pointerSquare.Center.X - (interactSize.X / 2), _pointerSquare.Bottom + 5), _colour * _interactTransparenty);
            }
        }
    }





    class InteractionBar : StaticGraphic
    {
        private Texture2D _MovingBar;
        private Rectangle _MovingBarSquare;
        private float _barLengthValue, _barValue, _barLengthPixel, _multiplyingValue;
        private Color _colour;



        public InteractionBar(Rectangle rectPos, Texture2D txr, Texture2D pixel, Color colour) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _texture = txr;
            _MovingBar = pixel;
            _colour = colour;
            _barLengthPixel = 238;
            _barLengthValue = 0;
            _barValue = 0;
        }



        public void updateme(Rectangle playerArea, float barLength, float barValue)
        {
            _barLengthValue = barLength;
            _barValue = barValue;

            _multiplyingValue = _barLengthPixel / _barLengthValue;

            _rectangle = new Rectangle(
                playerArea.Center.X - (_rectangle.Width / 2),
                playerArea.Bottom + 10,
                _rectangle.Width,
                _rectangle.Height);

            _MovingBarSquare = new Rectangle(
                _rectangle.X + 9,
                _rectangle.Y + 9,
                (int)(_barValue * _multiplyingValue),
                22);
        }



        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_MovingBar, _MovingBarSquare, _colour);
            sb.Draw(_texture, _rectangle, Color.White);
        }
    }





    class BuyWheel : StaticGraphic
    {
        public int wheelSlot
        { get { return _wheelSlot; } }

        private Color _colour;
        private Rectangle _cycleSource, _baseSource, _registerSquare;
        private Texture2D _itemTexture;
        private SpriteFont _font;
        private int _wheelSlot;
        private string _itemNamePrice;
        private Vector2 _centre;

        public BuyWheel(Rectangle rectPos, Texture2D txr, Texture2D itemsTexture, Color colour, SpriteFont font) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _baseSource = rectPos;
            _cycleSource = new Rectangle();
            _texture = txr;
            _itemTexture = itemsTexture;
            _colour = colour;
            _font = font;

            _itemNamePrice = "";
            _centre = new Vector2(960, 540);
            _registerSquare = new Rectangle(
                (int)_centre.X - _rectangle.Width,
                (int)_centre.Y - _rectangle.Height,
                _rectangle.Width * 3,
                _rectangle.Height * 3);
        }



        public void updatemeP1(Rectangle pArea, GamePadState pad, MouseState mouse, bool keyboardLastUsed, bool padLastUsed)
        {
            _rectangle = new Rectangle(
                pArea.Center.X - (_rectangle.Width / 2),
                pArea.Center.Y - (_rectangle.Height / 2),
                _rectangle.Width,
                _rectangle.Height);



            if (keyboardLastUsed && _registerSquare.Contains(mouse.Position))
            {
                // < Top Left >
                if (mouse.X < _centre.X && mouse.Y < _centre.Y && (mouse.Y - _centre.Y) > (mouse.X - _centre.X))
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 0,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 1;
                    _itemNamePrice = "$5 - Stone Pickaxe";
                }
                if (mouse.X < _centre.X && mouse.Y < _centre.Y && (mouse.Y - _centre.Y) < (mouse.X - _centre.X))
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 0,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 2;
                    _itemNamePrice = "$7 - Stone Multi-Tool";
                }
                // < Top Right >
                if (mouse.X > _centre.X && mouse.Y < _centre.Y && -(mouse.Y - _centre.Y) > (mouse.X - _centre.X))
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 0,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 3;
                    _itemNamePrice = "$5 - Stone Shovel";
                }
                if (mouse.X > _centre.X && mouse.Y < _centre.Y && -(mouse.Y - _centre.Y) < (mouse.X - _centre.X))
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 4;
                    _itemNamePrice = "$15 - Iron Pickaxe";
                }
                // < Bottom Right >
                if (mouse.X > _centre.X && mouse.Y > _centre.Y && (mouse.Y - _centre.Y) < (mouse.X - _centre.X))
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 5;
                    _itemNamePrice = "$17 - Iron Multi-Tool";
                }
                if (mouse.X > _centre.X && mouse.Y > _centre.Y && (mouse.Y - _centre.Y) > (mouse.X - _centre.X))
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 0,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 6;
                    _itemNamePrice = "$15 - Iron Shovel";
                }
                // < Bottom Left >
                if (mouse.X < _centre.X && mouse.Y > _centre.Y && (mouse.Y - _centre.Y) > (mouse.X - _centre.X))
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 7;
                    _itemNamePrice = "$25 - Almirs's Pickaxe";
                }
                if (mouse.X < _centre.X && mouse.Y > _centre.Y && (mouse.Y - _centre.Y) < -(mouse.X - _centre.X))
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 8;
                    _itemNamePrice = "$25 - The Golden Spoon";
                }
            }




            if (padLastUsed)
            {
                if (pad.ThumbSticks.Right.X < 0 && pad.ThumbSticks.Right.Y < 0.5f && pad.ThumbSticks.Right.Y > 0)
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 0,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 1;
                    _itemNamePrice = "$7 - Stone Pickaxe";
                }
                if (pad.ThumbSticks.Right.X < 0 && pad.ThumbSticks.Right.Y > 0.5f)
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 0,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 2;
                    _itemNamePrice = "$5 - Stone Multi-Tool";
                }
                if (pad.ThumbSticks.Right.X > 0 && pad.ThumbSticks.Right.Y > 0.5f)
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 0,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 3;
                    _itemNamePrice = "$7 - Stone Shovel";
                }
                if (pad.ThumbSticks.Right.X > 0 && pad.ThumbSticks.Right.Y < 0.5f && pad.ThumbSticks.Right.Y > 0)
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 4;
                    _itemNamePrice = "$17 - Iron Pickaxe";
                }
                if (pad.ThumbSticks.Right.X > 0 && pad.ThumbSticks.Right.Y > -0.5f && pad.ThumbSticks.Right.Y < 0)
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 5;
                    _itemNamePrice = "$15 - Iron Multi-Tool";
                }
                if (pad.ThumbSticks.Right.X > 0 && pad.ThumbSticks.Right.Y < -0.5f)
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 0,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 6;
                    _itemNamePrice = "$17 - Iron Shovel";
                }
                if (pad.ThumbSticks.Right.X < 0 && pad.ThumbSticks.Right.Y < -0.5f)
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 7;
                    _itemNamePrice = "$25 - Almirs's Pickaxe";
                }
                if (pad.ThumbSticks.Right.X < 0 && pad.ThumbSticks.Right.Y > -0.5f && pad.ThumbSticks.Right.Y < 0)
                {
                    _cycleSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    _wheelSlot = 8;
                    _itemNamePrice = "$25 - The Golden Spoon";
                }
            }
        }



        public void updatemeGENERIC(Rectangle playerArea, GamePadState pad)
        {
            if (pad.ThumbSticks.Right.X < 0 && pad.ThumbSticks.Right.Y < 0.5f && pad.ThumbSticks.Right.Y > 0)
            {
                _cycleSource = new Rectangle(
                    _rectangle.Width * 1,
                    _rectangle.Height * 0,
                    _rectangle.Width,
                    _rectangle.Height);
                _wheelSlot = 1;
                _itemNamePrice = "$7 - Stone Pickaxe";
            }
            if (pad.ThumbSticks.Right.X < 0 && pad.ThumbSticks.Right.Y > 0.5f)
            {
                _cycleSource = new Rectangle(
                    _rectangle.Width * 2,
                    _rectangle.Height * 0,
                    _rectangle.Width,
                    _rectangle.Height);
                _wheelSlot = 2;
                _itemNamePrice = "$5 - Stone Multi-Tool";
            }
            if (pad.ThumbSticks.Right.X > 0 && pad.ThumbSticks.Right.Y > 0.5f)
            {
                _cycleSource = new Rectangle(
                    _rectangle.Width * 0,
                    _rectangle.Height * 1,
                    _rectangle.Width,
                    _rectangle.Height);
                _wheelSlot = 3;
                _itemNamePrice = "$7 - Stone Shovel";
            }
            if (pad.ThumbSticks.Right.X > 0 && pad.ThumbSticks.Right.Y < 0.5f && pad.ThumbSticks.Right.Y > 0)
            {
                _cycleSource = new Rectangle(
                    _rectangle.Width * 1,
                    _rectangle.Height * 1,
                    _rectangle.Width,
                    _rectangle.Height);
                _wheelSlot = 4;
                _itemNamePrice = "$17 - Iron Pickaxe";
            }
            if (pad.ThumbSticks.Right.X > 0 && pad.ThumbSticks.Right.Y > -0.5f && pad.ThumbSticks.Right.Y < 0)
            {
                _cycleSource = new Rectangle(
                    _rectangle.Width * 2,
                    _rectangle.Height * 1,
                    _rectangle.Width,
                    _rectangle.Height);
                _wheelSlot = 5;
                _itemNamePrice = "$15 - Iron Multi-Tool";
            }
            if (pad.ThumbSticks.Right.X > 0 && pad.ThumbSticks.Right.Y < -0.5f)
            {
                _cycleSource = new Rectangle(
                    _rectangle.Width * 0,
                    _rectangle.Height * 2,
                    _rectangle.Width,
                    _rectangle.Height);
                _wheelSlot = 6;
                _itemNamePrice = "$17 - Iron Shovel";
            }
            if (pad.ThumbSticks.Right.X < 0 && pad.ThumbSticks.Right.Y < -0.5f)
            {
                _cycleSource = new Rectangle(
                    _rectangle.Width * 1,
                    _rectangle.Height * 2,
                    _rectangle.Width,
                    _rectangle.Height);
                _wheelSlot = 7;
                _itemNamePrice = "$27 - Almirs's Pickaxe";
            }
            if (pad.ThumbSticks.Right.X < 0 && pad.ThumbSticks.Right.Y > -0.5f && pad.ThumbSticks.Right.Y < 0)
            {
                _cycleSource = new Rectangle(
                    _rectangle.Width * 2,
                    _rectangle.Height * 2,
                    _rectangle.Width,
                    _rectangle.Height);
                _wheelSlot = 8;
                _itemNamePrice = "$25 - The Golden Spoon";
            }
        }



        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, _baseSource, Color.White);
            sb.Draw(_texture, _rectangle, _cycleSource, _colour);
            sb.Draw(_itemTexture, _rectangle, Color.White);

            Vector2 stringSize = _font.MeasureString(_itemNamePrice);
            sb.DrawString(_font, _itemNamePrice, new Vector2(_rectangle.Center.X - (stringSize.X / 2), _rectangle.Top - stringSize.Y), Color.Black);
            sb.DrawString(_font, _itemNamePrice, new Vector2(_rectangle.Center.X - (stringSize.X / 2) + 3, _rectangle.Top - stringSize.Y + 3), _colour);
        }
    }


    class P1Crown : StaticGraphic
    {
        public P1Crown(Rectangle rectPos, Texture2D txr) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _texture = txr;
        }

        public void updateme(Rectangle player)
        {
            _rectangle = new Rectangle(
                player.Center.X - (_rectangle.Width / 2),
                player.Top - (_rectangle.Height + 50),
                _rectangle.Width,
                _rectangle.Height);
        }
    }
}
