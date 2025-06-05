using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diggy_Hole
{
    // < Static Graphic > This is used directly or as a base for the simplist game objects
    class StaticGraphic
    {
        protected Rectangle _rectangle;
        protected Texture2D _texture;

        public StaticGraphic(Rectangle rectPos, Texture2D txr)
        {
            _rectangle = rectPos;
            _texture = txr;
        }



        public virtual void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, Color.White);
        }
    }



    class MovingGraphic : StaticGraphic
    {
        // < Movement Varibles >
        protected Vector2 _position, _velocity;


        public MovingGraphic(Texture2D spriteSheet, Rectangle rect) : base(rect, spriteSheet)
        {
            _rectangle = rect;
            _position = new Vector2(rect.X, rect.Y);
        }


        public override void drawme(SpriteBatch sb)
        {
            _rectangle.X = (int)_position.X;
            _rectangle.Y = (int)_position.Y;

            sb.Draw(_texture, new Vector2(_rectangle.X, _rectangle.Y), Color.White);
        }
    }



    // < Static Animated > 
    class StaticAnimated2D : StaticGraphic
    {
        // < Animation Varibles >
        protected Vector2 _position, _origin;
        protected float _scale, _updateTrigger, _framesPerSecond;
        protected Rectangle _sourceRectangle;


        public StaticAnimated2D(Texture2D spriteSheet, float fps, Rectangle SrcRect, Rectangle rect) : base(rect, spriteSheet)
        {
            _rectangle = rect;
            _position = new Vector2(rect.X, rect.Y);
            _sourceRectangle = new Rectangle(0, 0, SrcRect.Width, SrcRect.Height);
            _origin = new Vector2(SrcRect.Width / 2, SrcRect.Height / 2);
            _updateTrigger = 0;
            _framesPerSecond = fps;
            _scale = 1;
        }


        public virtual void updateme(GameTime gt)
        {
            // < Animation Updates > this statement runs through every update cycle checking if its ready to change the animation frame on the object
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            if (_updateTrigger >= 1)
            {
                _updateTrigger = 0;
                _sourceRectangle.X += _sourceRectangle.Width;

                // ------- < Animation Reset > once the animation reaches the end of the sheet it will be set back to the start
                if (_sourceRectangle.X >= _texture.Width)
                {
                    _sourceRectangle.X = 0;
                }
            }
        }


        public override void drawme(SpriteBatch sb)
        {
            _rectangle.X = (int)_position.X;
            _rectangle.Y = (int)_position.Y;

            sb.Draw(_texture, new Vector2(_rectangle.X, _rectangle.Y), _sourceRectangle, Color.White, 0, _origin, _scale, SpriteEffects.None, 0);
        }
    }



    // < Moving Animated >
    class MovingAnimated2D : StaticAnimated2D
    {
        // < Movement & Rotation Varibles >
        protected Vector2 _velocity;
        protected float _rotation;


        public MovingAnimated2D(Texture2D spriteSheet, float fps, Rectangle SrcRect, Rectangle rect) : base(spriteSheet, fps, SrcRect, rect)
        {
            _rectangle = rect;
            _position = new Vector2(rect.X, rect.Y);
            _velocity = Vector2.Zero;
            _sourceRectangle = new Rectangle(0, 0, SrcRect.Width, SrcRect.Height);
            _updateTrigger = 0;
            _framesPerSecond = fps;
            _rotation = 0;
            _origin = new Vector2(SrcRect.Width / 2, SrcRect.Height / 2);
            _scale = 1;
        }


        public virtual void updateme(GameTime gt)
        {
            // < Animation Updates > this statement runs through every update cycle checking if its ready to change the animation frame on the object
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            if (_updateTrigger >= 1)
            {
                _updateTrigger = 0;
                _sourceRectangle.X += _sourceRectangle.Width;

                // ------- < Animation Reset > once the animation reaches the end of the sheet it will be set back to the start
                if (_sourceRectangle.X >= _texture.Width)
                {
                    _sourceRectangle.X = 0;
                }
            }
        }


        public override void drawme(SpriteBatch sb)
        {
            _rectangle.X = (int)_position.X;
            _rectangle.Y = (int)_position.Y;

            sb.Draw(_texture, new Vector2(_rectangle.X, _rectangle.Y), _sourceRectangle, Color.White, _rotation, _origin, _scale, SpriteEffects.None, 0);
        }
    }
}
