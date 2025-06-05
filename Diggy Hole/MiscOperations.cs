using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Diggy_Hole
{
    class WipeTransition : StaticAnimated2D
    {
        public bool wipeFinished
        { get { return _wipeFinished; } }

        public bool screenLock
        { get { return _wipeToggleLock; } }

        private float _fullScreenCounter;
        private int _numOfRows, _rowCounter;
        private bool _wipeToggleLock, _wipeFinished;

        public WipeTransition(Texture2D spriteSheet, float fps, Rectangle srcRect, Rectangle rect) : base(spriteSheet, fps, srcRect, rect)
        {
            _texture = spriteSheet;
            _framesPerSecond = fps;
            _sourceRectangle = srcRect;
            _rectangle = rect;
            _numOfRows = 3;
        }

        public override void updateme(GameTime gt)
        {
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            if (_wipeToggleLock == true)
                _fullScreenCounter += (float)gt.ElapsedGameTime.TotalSeconds;

            if (_updateTrigger >= 1 && _wipeFinished == false)
            {
                if (_wipeToggleLock == false)
                {
                    _updateTrigger = 0;
                    _sourceRectangle.X += _sourceRectangle.Width;

                    // ------- < Animation Reset > once the animation reaches the end of the sheet it will be set back to the start
                    if (_sourceRectangle.X >= _texture.Width && _rowCounter < 3)
                    {
                        _sourceRectangle.Y += _sourceRectangle.Height;
                        _sourceRectangle.X = 0;
                        _rowCounter++;
                    }
                    else if (_sourceRectangle.X >= _texture.Width && _rowCounter == _numOfRows)
                    {
                        _wipeToggleLock = true;
                        _sourceRectangle.X = _sourceRectangle.Width * 7;
                        _sourceRectangle.Y = _sourceRectangle.Height * 3;
                    }
                }
                if (_wipeToggleLock == true && _fullScreenCounter > 1)
                {
                    _updateTrigger = 0;
                    _sourceRectangle.X -= _sourceRectangle.Width;

                    // ------- < Animation Reset > once the animation reaches the end of the sheet it will be set back to the start
                    if (_sourceRectangle.X < 0 && _rowCounter > 0)
                    {
                        _sourceRectangle.Y -= _sourceRectangle.Height;
                        _sourceRectangle.X = _sourceRectangle.Width * 7;
                        _rowCounter--;
                    }
                    if (_sourceRectangle.X < 0 && _rowCounter == 0)
                    {
                        _wipeFinished = true;
                        _sourceRectangle = new Rectangle(839, 315, 0, 0);
                    }
                }
            }
        }

        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, _sourceRectangle, Color.White);
        }
    }



    class SplashSequence : StaticGraphic
    {
        public bool finishedSplash
        { get { return _finished; } }

        private Texture2D _vegaLogo, _monogameLogo;
        private float _backTransparency, _frontTransparency;
        private int _segment, _endSegment;
        private Rectangle _monogameSquare, _vegaSquare;
        private bool _rubberBand, _finished;

        public SplashSequence(Rectangle rectPos, Texture2D txr, Texture2D monogameLogo, Texture2D vegaLogo) : base(rectPos, txr)
        {
            _texture = txr;
            _rectangle = rectPos;

            _vegaLogo = vegaLogo;
            _vegaSquare = new Rectangle(0, 0, 500, 728);

            _monogameLogo = monogameLogo;
            _monogameSquare = new Rectangle(0, 0, 500, 500);

            _backTransparency = -1;
            _endSegment = 4;
        }

        public void updateme(GameTime gt)
        {
            _backTransparency += (float)gt.ElapsedGameTime.TotalSeconds;

            if (_backTransparency > 1 && _segment % 2 == 0)
                _frontTransparency += (float)gt.ElapsedGameTime.TotalSeconds;

            if (_segment % 2 != 0)
                _frontTransparency -= (float)gt.ElapsedGameTime.TotalSeconds;

            if (_frontTransparency > 2 && _rubberBand == false)
            {
                _rubberBand = true;
                _segment++;
            }
            else if (_frontTransparency < 0 && _rubberBand == true)
            {
                _segment++;
                _rubberBand = false;
            }

            if (_segment == _endSegment)
                _finished = true;
        }

        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, Color.White * _backTransparency);
            if (_segment == 0 || _segment == 1)
                sb.Draw(_monogameLogo, new Rectangle((_rectangle.Width / 2) - (_monogameSquare.Width / 2), (_rectangle.Height / 2) - (_monogameSquare.Height / 2), _monogameSquare.Width, _monogameSquare.Height), Color.White * _frontTransparency);
            else if (_segment == 2 || _segment == 3)
                sb.Draw(_vegaLogo, new Rectangle((_rectangle.Width / 2) - (_vegaSquare.Width / 2), (_rectangle.Height / 2) - 468, _vegaSquare.Width, _vegaSquare.Height), Color.White * _frontTransparency);
        }
    }



    class PysicalItem : MovingGraphic
    {
        public Items Item
        { get { return _item; } }

        public Rectangle Hitbox
        { get { return _rectangle; } }

        private bool _grounded;
        private float _airDrag, _groundDrag, _gravity;
        private Rectangle _itemSource, _oldRectangle;
        private Items _item;

        public PysicalItem(Texture2D spriteSheet, Rectangle rect, Vector2 vel, Items item) : base(spriteSheet, rect)
        {
            _texture = spriteSheet;
            _rectangle = rect;
            _oldRectangle = _rectangle;
            _velocity = vel;
            _item = item;

            _airDrag = 0.65f;
            _groundDrag = 0.65f;
            _gravity = 1f;

            _grounded = false;

            switch (item)
            {
                case Items.GoldPickaxe:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 0,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.GoldShovel:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 0,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.IronPickaxe:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 0,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.IronMultiTool:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.IronShovel:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 1,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.StonePickaxe:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 0,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.StoneMultiTool:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case Items.StoneShovel:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 2,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;


                case Items.Coal:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 0,
                        _rectangle.Height * 3,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.Copper:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 1,
                        _rectangle.Height * 3,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.Iron:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 2,
                        _rectangle.Height * 3,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;

                case Items.Gold:
                    _itemSource = new Rectangle(
                        _rectangle.Width * 0,
                        _rectangle.Height * 4,
                        _rectangle.Width,
                        _rectangle.Height);
                    break;
            }
        }

        public void updateme(Rectangle[,] eHB)
        {
            // < Position Modifier >
            _position.X += _velocity.X;
            _position.Y += _velocity.Y;


            // < Hitbox & Position Updates >
            _rectangle = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                _rectangle.Width,
                _rectangle.Height);


            // < Drag & Gravity >
            if (_grounded == true)
                _velocity.X *= _groundDrag;
            else if (_grounded == false)
                _velocity.X *= _airDrag;

            if (_grounded == false)
                _velocity.Y += _gravity;
            else if (_grounded == true)
                _velocity.Y *= _airDrag;


            // < Collision Checker >
            for (int i = 0; i < 400; i++)
            {
                // ----- < Wall Collider >
                if (_rectangle.Intersects(eHB[2, i]))
                {
                    _velocity.X = 0;

                    bool leftSide;
                    if (_rectangle.Center.X < eHB[2, i].Center.X)
                        leftSide = true;
                    else
                        leftSide = false;

                    if (leftSide)
                    {
                        _position.X = eHB[2, i].Left - (_rectangle.Width - (_rectangle.Right - _rectangle.Right));
                    }
                    else
                    {
                        _position.X = eHB[2, i].Right - (_rectangle.Left - _rectangle.Left) + 1;
                    }
                }
                // ----- < Floor Collider >
                if (_rectangle.Intersects(eHB[0, i]) && _rectangle.Top < eHB[0, i].Top && _oldRectangle.Left < eHB[0, i].Right && _oldRectangle.Right > eHB[0, i].Left)
                {
                    _velocity.Y = 0;
                    _position.Y = (eHB[0, i].Y - (_rectangle.Height - 40));
                    break;
                }
                // ----- < Ceiling Collider >
                if (_rectangle.Intersects(eHB[1, i]) && _rectangle.Top > eHB[1, i].Top && _oldRectangle.Left < eHB[1, i].Right && _oldRectangle.Right > eHB[1, i].Left)
                {
                    _velocity.Y = 0;
                    _position.Y = (eHB[1, i].Bottom - (_rectangle.Top - _rectangle.Top));
                    break;
                }
                // ----- < Platform Collider >
                if (_rectangle.Intersects(eHB[3, i]) && _rectangle.Bottom < eHB[3, i].Top + 5 && _oldRectangle.Left < eHB[3, i].Right && _oldRectangle.Right > eHB[3, i].Left)
                {
                    _velocity.Y = 0;
                    _position.Y = (eHB[3, i].Y - (_rectangle.Height - 40));
                    break;
                }
            }
        }

        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, _itemSource, Color.White);
        }
    }



    class Lantern : StaticGraphic
    {
        private Texture2D _light;
        private Vector2 _lightOffset;
        private Rectangle _lightSquare;

        public Lantern(Rectangle rectPos, Texture2D txr, Texture2D light) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _texture = txr;
            _light = light;

            _lightOffset = new Vector2(_rectangle.Center.X, _rectangle.Center.Y);
            _lightSquare = new Rectangle((int)(_lightOffset.X - 720), (int)(_lightOffset.Y - 250), 1440, 450);
        }
        
        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, Color.White);
        }

        public void drawMask(SpriteBatch sb)
        {
            sb.Draw(_light, _lightSquare, Color.White);
        }
    }



    class CursorHand : StaticGraphic
    {
        private Rectangle _itemSource, _itemSourceBoxDefalt;
        private Items _oldHand;

        public CursorHand(Rectangle rectPos, Texture2D txr) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _texture = txr;
            _itemSourceBoxDefalt = rectPos;
            _oldHand = Items.GoldPickaxe;
        }

        public void updateme(MouseState mouse, Items hand)
        {
            _rectangle.X = mouse.X - (_rectangle.Width / 2);
            _rectangle.Y = mouse.Y - (_rectangle.Height / 2);

            if (_oldHand != hand)
            {
                switch (hand)
                {
                    case Items.Bare:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 0,
                            _itemSourceBoxDefalt.Height * 0,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.GoldPickaxe:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 1,
                            _itemSourceBoxDefalt.Height * 0,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.GoldShovel:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 2,
                            _itemSourceBoxDefalt.Height * 0,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.IronPickaxe:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 0,
                            _itemSourceBoxDefalt.Height * 1,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.IronMultiTool:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 1,
                            _itemSourceBoxDefalt.Height * 1,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.IronShovel:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 2,
                            _itemSourceBoxDefalt.Height * 1,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.StonePickaxe:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 0,
                            _itemSourceBoxDefalt.Height * 2,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.StoneMultiTool:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 1,
                            _itemSourceBoxDefalt.Height * 2,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;


                    case Items.StoneShovel:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 2,
                            _itemSourceBoxDefalt.Height * 2,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;


                    case Items.Coal:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 0,
                            _itemSourceBoxDefalt.Height * 3,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.Copper:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 1,
                            _itemSourceBoxDefalt.Height * 3,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.Iron:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 2,
                            _itemSourceBoxDefalt.Height * 3,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;

                    case Items.Gold:
                        _itemSource = new Rectangle(
                            _itemSourceBoxDefalt.Width * 0,
                            _itemSourceBoxDefalt.Height * 4,
                            _itemSourceBoxDefalt.Width,
                            _itemSourceBoxDefalt.Height);
                        break;
                }
            }

            _oldHand = hand;
        }


        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, _itemSource, Color.White);
        }
    }



    class TutorialObject : StaticGraphic
    {
        public int stage
        { get { return _stage; } }

        public Rectangle[] stageRect
        { get { return _stageRectangles; } }

        public bool finishedTutorial
        { get { return _finishedTutorial; } }

        private string[] _dialogList, _dialogListKeyboard, _splitDialog;
        private string _drawnString;
        private Rectangle _mapCellSize;
        private Rectangle[] _stageRectangles;
        private int _stage, _oldStage, _numberOfDialogSegments, _dialogSegment;
        private SpriteFont _font;
        private Vector2 _stringSize;
        private float _counter;
        private bool _finishedTutorial, _kbUsed;

        private KeyboardState oldKB;

        public TutorialObject(Rectangle rectPos, Texture2D txr, SpriteFont font) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _texture = txr;
            _font = font;

            _mapCellSize = new Rectangle(0, 0, 512, 512);

            _stageRectangles = new Rectangle[]
            {
                new Rectangle(_mapCellSize.Width * 9, _mapCellSize.Height * 4, _mapCellSize.Width, _mapCellSize.Height),
                new Rectangle(_mapCellSize.Width * 7, _mapCellSize.Height * 3, _mapCellSize.Width, _mapCellSize.Height),
                new Rectangle(_mapCellSize.Width * 6, _mapCellSize.Height * 6, _mapCellSize.Width, _mapCellSize.Height),
                new Rectangle(_mapCellSize.Width * 4, _mapCellSize.Height * 6, _mapCellSize.Width, _mapCellSize.Height),
            };



            _dialogList = new string[] 
            { 
                "Welcome to the mines, new hand",
                "We need you haul out some goodies for us in\nthese mine shafts",
                "First lets get those legs moving\n\nuse LEFT-STICK to move & A to jump",
                "\nNow lets get some tools,\nopen up the buy menu with LEFT-TRIGGER,\nnavigate with RIGHT-STICK and\nbuy a STONE PICKAXE with X",
                "\nWe dropped the pickaxe next to the deposit cart\n\nNow head straight down the corridor,\nwe will start there",
                "\nSee that coal seam in the wall?\nI need you to dig out two lumps of coal for me",
                "Blast! the tunnel collaped behind us!\nclimb that ladder, see if we can go around",
                "Drop down through the platform here\nby holding the LEFT-STICK down\n\ndont worry its not a long drop",
                "Now lets get that coal back to the depost cart",
                "\nRight, now I want you to buy a stone multi-tool\n\nthey take a bit longer to use but can clear\ncave-ins and mine ore seams",
                "\nDrop your pickaxe with B,\nyou wont need it for a time\n\nNavigate your hand bar with LB and RB",
                "\nGreat you have made good headway so far\n\nhead back down the corridor\nand take the ladder down",
                "Take that muti-tool of yours and\ndig out this cave-in here",
                "Take a chunk of copper from\nthat seam there and get it\nback to the cart",
                "\nNow thats done I say you now know everything\nyou need to know\nabout the jobs we will send you on\n\nGet in and get what we tell you to get",
                "\nIts probably at this point\nI need to tell you that you\nneed to do this quickly as the shaft\nthat you enter by is soon to collapse",
                "But if we started off by telling you that\nyou wouldn't have stayed",
                "Now get to work before everything caves-in!"
            };

            _dialogListKeyboard = new string[]
            {
                "Welcome to the mines, new hand",
                "We need you haul out some goodies for us in\nthese mine shafts",
                "First lets get those legs moving\n\nuse WASD to move & SPACEBAR to jump",
                "\nNow lets get some tools,\nopen up the buy menu with C,\nnavigate with MOUSE and\nbuy a STONE PICKAXE with RIGHT-MOUSE",
                "\nWe dropped the pickaxe next to the deposit cart\n\nNow head straight down the corridor,\nwe will start there",
                "\nSee that coal seam in the wall?\nI need you to dig out two lumps of coal for me",
                "Blast! the tunnel collaped behind us!\nclimb that ladder, see if we can go around",
                "Drop down through the platform here\nby holding down\n\ndont worry its not a long drop",
                "Now lets get that coal back to the depost cart",
                "\nRight, now I want you to buy a stone multi-tool\n\nthey take a bit longer to use but can clear\ncave-ins and mine ore seams",
                "\nDrop your pickaxe with Q,\nyou wont need it for a time\n\nNavigate your hand bar with SCROLL-WHEEL",
                "\nGreat you have made good headway so far\n\nhead back down the corridor\nand take the ladder down",
                "Take that muti-tool of yours and\ndig out this cave-in here",
                "Take a chunk of copper from\nthat seam there and get it\nback to the cart",
                "\nNow thats done I say you now know everything\nyou need to know\nabout the jobs we will send you on\n\nGet in and get what we tell you to get",
                "\nIts probably at this point\nI need to tell you that you\nneed to do this quickly as the shaft\nthat you enter by is soon to collapse",
                "But if we started off by telling you that\nyou wouldn't have stayed",
                "Now get to work before everything caves-in!"
            };

            _splitDialog = _dialogList[_stage].Split(" ");
            _numberOfDialogSegments = _splitDialog.Length;
            _stringSize = _font.MeasureString(_dialogList[_stage]);
            _drawnString = "";

            _counter = -3;
        }

        public void updateme(GameTime gt, int money, bool kbUsed)
        {
            _counter += (float)gt.ElapsedGameTime.TotalSeconds;

            if (_counter >= 0.1f && _dialogSegment < _numberOfDialogSegments)
            {
                if (_dialogSegment > 0)
                    _drawnString += " " + _splitDialog[_dialogSegment];
                else
                    _drawnString += _splitDialog[_dialogSegment];

                _counter = 0;
                _dialogSegment++;
            }

            if (_counter >= 3 && _stage < 2)
            {
                nextStage();
            }
            if (_counter >= 5 && _stage == 2)
            {
                nextStage();
            }
            if (_stage == 8 && money >= 6)
            {
                nextStage();
            }
            if (_stage == 13 && money >= 6)
            {
                nextStage();
            }
            if (_counter >= 5 && _stage >= 14 && _stage < 17)
            {
                nextStage();
            }
            if (_stage == 17 && _counter >= 5)
            {
                _finishedTutorial = true;
            }

            _kbUsed = kbUsed;
            _oldStage = _stage;
        }

        public void nextStage()
        {
            _counter = 0;
            _stage++;

            if (_kbUsed == true)
            {
                _splitDialog = _dialogListKeyboard[_stage].Split(" ");
                _numberOfDialogSegments = _splitDialog.Length;
                _stringSize = _font.MeasureString(_dialogListKeyboard[_stage]);
            }
            else
            {
                _splitDialog = _dialogList[_stage].Split(" ");
                _numberOfDialogSegments = _splitDialog.Length;
                _stringSize = _font.MeasureString(_dialogList[_stage]);
            }

            _drawnString = "";
            _dialogSegment = 0;
        }

        public override void  drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, Color.White);

            sb.DrawString(_font, _drawnString, new Vector2(_rectangle.Right + 20, _rectangle.Center.Y - (_stringSize.Y / 2) + 50), Color.Black);
            sb.DrawString(_font, _drawnString, new Vector2(_rectangle.Right + 23, _rectangle.Center.Y - (_stringSize.Y / 2) + 53), Color.White);
        }
    }



    class FlashCard : StaticGraphic
    {
        private string _title;
        private Texture2D _hollowStar;
        private Rectangle _entranceRectangle;
        private int _numOfStars;
        private float _trans;
        private SpriteFont _largeFont, _smallFont;



        public FlashCard(Rectangle rectPos, Texture2D txr, Texture2D hollowStar, Rectangle entranceRect, String title, int numberOfStars, SpriteFont largeFont, SpriteFont smallFont) : base(rectPos, txr)
        {
            _texture = txr;
            _hollowStar = hollowStar;
            _rectangle = rectPos;
            _numOfStars = numberOfStars;
            _entranceRectangle = entranceRect;
            _title = title;
            _largeFont = largeFont;
            _smallFont = smallFont;

        }

        public void updateme(GameTime gt, Rectangle player)
        {
            _trans -= (float)gt.ElapsedGameTime.TotalSeconds;

            if (player.Intersects(_entranceRectangle))
                _trans = 2;
        }

        public void drawme(SpriteBatch sb, bool kblastUsed)
        {
            Vector2 drawBasePoint = new Vector2(
                ((_entranceRectangle.Center.X - (_rectangle.Width / 2)) - 30) - _rectangle.Width, 
                _entranceRectangle.Y - _rectangle.Height);

            for (int i = 0; i < 3; i++)
            {
                if (i + 1 <= _numOfStars)
                {
                    sb.Draw(_texture, 
                        new Rectangle(
                            (int)drawBasePoint.X + ((_rectangle.Width + 30) * i),
                            (int)drawBasePoint.Y,
                            _rectangle.Width,
                            _rectangle.Height),
                        Color.White  * _trans);
                }
                else
                    sb.Draw(_hollowStar,
                        new Rectangle(
                            (int)drawBasePoint.X + ((_rectangle.Width + 30) * i),
                            (int)drawBasePoint.Y,
                            _rectangle.Width,
                            _rectangle.Height),
                        Color.White * _trans);
            }



            Vector2 titleSize = _largeFont.MeasureString(_title);
            sb.DrawString(_largeFont, 
                _title, 
                new Vector2(_entranceRectangle.Center.X - (titleSize.X / 2), 
                (_entranceRectangle.Top - _rectangle.Height - 10) - (titleSize.Y)), 
                Color.Black * _trans);
            sb.DrawString(_largeFont,
                _title,
                new Vector2(_entranceRectangle.Center.X + 5 - (titleSize.X / 2),
                (_entranceRectangle.Top - _rectangle.Height - 15) - (titleSize.Y)),
                Color.White * _trans);



            if (kblastUsed == true)
            {
                Vector2 enterSize = _smallFont.MeasureString("press E to ENTER level");
                sb.DrawString(
                    _smallFont, 
                    "press E to ENTER level", 
                    new Vector2(_entranceRectangle.Center.X - (enterSize.X / 2), _entranceRectangle.Bottom + enterSize.Y + 5), 
                    Color.Black * _trans);
                sb.DrawString(
                    _smallFont,
                    "press E to ENTER level",
                    new Vector2(_entranceRectangle.Center.X - (enterSize.X / 2) + 3, _entranceRectangle.Bottom + enterSize.Y + 8),
                    Color.White * _trans);
            }
            else
            {
                Vector2 enterSize = _smallFont.MeasureString("press X to ENTER level");
                sb.DrawString(
                    _smallFont,
                    "press X to ENTER level",
                    new Vector2(_entranceRectangle.Center.X - (enterSize.X / 2), _entranceRectangle.Bottom + enterSize.Y + 10),
                    Color.Black * _trans);
                sb.DrawString(
                    _smallFont,
                    "press X to ENTER level",
                    new Vector2(_entranceRectangle.Center.X - (enterSize.X / 2) + 3, _entranceRectangle.Bottom + enterSize.Y + 13),
                    Color.White * _trans);
            }
        }
    }



    class titleCard : StaticGraphic
    {
        private SpriteFont _fontLittle;
        private float _counter;
        private Texture2D _title;

        public titleCard(Rectangle rectPos, Texture2D txr, Texture2D title,SpriteFont fontLittle) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _texture = txr;
            _title = title;
            _fontLittle = fontLittle;
        }

        public void updateme(GameTime gt)
        {
            _counter += (float)gt.ElapsedGameTime.TotalSeconds;
        }

        public void drawme(SpriteBatch sb, bool keyboadUsed)
        {
            sb.Draw(_texture, _rectangle, Color.White);
            sb.Draw(_title, _rectangle, Color.White);




            if (keyboadUsed == true)
            {
                Vector2 startSize = _fontLittle.MeasureString("press SPACEBAR to START!");
                if ((int)_counter % 2 == 0)
                {
                    sb.DrawString(_fontLittle, "press SPACEBAR to START!", new Vector2((_rectangle.Width / 2) - (startSize.X / 2) - 5, ((_rectangle.Height / 2) + (_rectangle.Height / 5)) - (startSize.Y / 2) - 5), Color.Black);
                    sb.DrawString(_fontLittle, "press SPACEBAR to START!", new Vector2((_rectangle.Width / 2) - (startSize.X / 2), ((_rectangle.Height / 2) + (_rectangle.Height / 5)) - (startSize.Y / 2)), Color.White);
                }

                Vector2 exitSize = _fontLittle.MeasureString("press ESCAPE to SAVE & EXIT");
                sb.DrawString(_fontLittle, "press ESCAPE to SAVE & EXIT", new Vector2((_rectangle.Width / 2) - (exitSize.X / 2) - 5, ((_rectangle.Height / 2) + (_rectangle.Height / 5) + 85) - (startSize.Y / 2) - 5), Color.Black);
                sb.DrawString(_fontLittle, "press ESCAPE to SAVE & EXIT", new Vector2((_rectangle.Width / 2) - (exitSize.X / 2), ((_rectangle.Height / 2) + (_rectangle.Height / 5) + 85) - (startSize.Y / 2)), Color.White);

                Vector2 restartSize = _fontLittle.MeasureString("press R to RESET GAME");
                sb.DrawString(_fontLittle, "press R to to RESET GAME", new Vector2((_rectangle.Width / 2) - (restartSize.X / 2) - 5, ((_rectangle.Height / 2) + (_rectangle.Height / 5) + 170) - (restartSize.Y / 2) - 5), Color.Black);
                sb.DrawString(_fontLittle, "press R to to RESET GAME", new Vector2((_rectangle.Width / 2) - (restartSize.X / 2), ((_rectangle.Height / 2) + (_rectangle.Height / 5) + 170) - (restartSize.Y / 2)), Color.White);
            }
            else
            {
                Vector2 startSize = _fontLittle.MeasureString("press A to START!");
                if ((int)_counter % 2 == 0)
                {
                    sb.DrawString(_fontLittle, "press A to START!", new Vector2((_rectangle.Width / 2) - (startSize.X / 2) - 5, ((_rectangle.Height / 2) + (_rectangle.Height / 5)) - (startSize.Y / 2) - 5), Color.Black);
                    sb.DrawString(_fontLittle, "press A to START!", new Vector2((_rectangle.Width / 2) - (startSize.X / 2), ((_rectangle.Height / 2) + (_rectangle.Height / 5)) - (startSize.Y / 2)), Color.White);
                }

                Vector2 exitSize = _fontLittle.MeasureString("press B to SAVE & EXIT");
                sb.DrawString(_fontLittle, "press B to SAVE & EXIT", new Vector2((_rectangle.Width / 2) - (exitSize.X / 2) - 5, ((_rectangle.Height / 2) + (_rectangle.Height / 5) + 85) - (startSize.Y / 2) - 5), Color.Black);
                sb.DrawString(_fontLittle, "press B to SAVE & EXIT", new Vector2((_rectangle.Width / 2) - (exitSize.X / 2), ((_rectangle.Height / 2) + (_rectangle.Height / 5) + 85) - (startSize.Y / 2)), Color.White);
                
                Vector2 restartSize = _fontLittle.MeasureString("press Y to RESET GAME");
                sb.DrawString(_fontLittle, "press Y to to RESET GAME", new Vector2((_rectangle.Width / 2) - (restartSize.X / 2) - 5, ((_rectangle.Height / 2) + (_rectangle.Height / 5) + 170) - (restartSize.Y / 2) - 5), Color.Black);
                sb.DrawString(_fontLittle, "press Y to to RESET GAME", new Vector2((_rectangle.Width / 2) - (restartSize.X / 2), ((_rectangle.Height / 2) + (_rectangle.Height / 5) + 170) - (restartSize.Y / 2)), Color.White);
            }
            
        }
    }



    [Serializable]
    public struct ProgressData
    {
        public bool[] LevelsComplete;
        public int[] LevelStars;
        public bool PlayedBefore;

        public ProgressData()
        {
            LevelsComplete = new bool[5];
            LevelStars = new int[5];
            PlayedBefore = false;
        }
    }

    class ProgressManager
    {
        private ProgressData _data;
        private string _filename;


        public ProgressData Data
        {
            get { return _data; }
        }


        public ProgressManager(string filename)
        {
            _filename = filename;
            _data = new ProgressData();

            // Check to see if the save exists
            if (!File.Exists(_filename))
            {
                setDefault();
            }
            else
                Load();
        }

        ~ProgressManager()
        {
            // When the scoreManager get destroyed, save the Scores
            Save();
        }

        // Just fill the table will some default values.
        private void setDefault()
        {
            _data.LevelsComplete[0] = false;
            _data.LevelsComplete[1] = false;
            _data.LevelsComplete[2] = false;
            _data.LevelsComplete[3] = false;
            _data.LevelsComplete[4] = false;

            _data.LevelStars[0] = 0;
            _data.LevelStars[1] = 0;
            _data.LevelStars[2] = 0;
            _data.LevelStars[3] = 0;
            _data.LevelStars[4] = 0;

            _data.PlayedBefore = false;
        }

        public void addData(bool[] levelsComplete, int[] levelStars, bool playedBefore)
        {
            for (int i = 0; i < 5; i++)
            {
                _data.LevelsComplete = levelsComplete;
                _data.LevelStars = levelStars;
            }

            _data.PlayedBefore = playedBefore;
        }

        public void Save()
        {
            FileStream stream;

            try 
            {
                // Open the file, creating it if necessary
                stream = File.Open(_filename, FileMode.OpenOrCreate);

                // Convert the object to XML data and put it in the stream
                XmlSerializer serializer = new XmlSerializer(typeof(ProgressData));
                serializer.Serialize(stream, _data);

                // Close the file
                stream.Close();
            }
            catch (Exception error)
            {
                Debug.WriteLine("Save has failed because of: " + error.Message);
            }
        }

        public void Load()
        {
            FileStream stream;

            try
            {
                // Open the file - but read only mode!
                stream = File.Open(_filename, FileMode.OpenOrCreate, FileAccess.Read);
                // Read the data from the file
                XmlSerializer serializer = new XmlSerializer(typeof(ProgressData));
                _data = (ProgressData)serializer.Deserialize(stream);
            }
            catch (Exception error) // The code in "catch" is what happens if the "try" fails.
            {
                setDefault();
                Debug.WriteLine("Load has failed because of: " + error.Message);
            }
        }
    }
}
