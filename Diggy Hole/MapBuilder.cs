using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Diggy_Hole
{
    internal class PhysicalMap : StaticGraphic
    {
        public Rectangle[,] Hitboxs
        { get { return _hitboxs; } }

        public Vector2 StartPosition
        { get { return _startPosition; } }

        public Vector2[] HitboxTypes
        { get { return _hitboxTypes; } }

        public bool forceCaveInRoggleLock
        { get { return _forceCaveInToggleLock; } }

        private int[,] _map;
        private int[] _caveInStage;
        private Texture2D _pixel, _light;
        private Color _baseColour;
        private Rectangle[,] _hitboxs, _tileSheetSource;
        private Rectangle _lightRect;
        private Vector2[] _hitboxTypes, _caveInCellTag;
        private Vector2 _mapSize, _floors, _ceilings, _walls, _platforms, _ladders, _levelEntrances, _coalSeams, _copperSeams, _ironSeams, _goldSeams, _depositCart, _ladderCaps, _startPosition, _caveIns, _caveInRubbleWall;
        private float[] _caveInCounter;
        private int _maxTime, _timeSections;
        private bool _entranceToggleLock, _forceCaveInToggleLock;
        private bool[] _caveInToggleLock;
        private SoundEffectInstance _caveInInst, _clearCaveInInst, _rockFallInst;


        public PhysicalMap(
            Texture2D txr, 
            Texture2D pixel, 
            Texture2D light, 
            SoundEffectInstance caveIn, 
            SoundEffectInstance clearCaveIn,
            SoundEffectInstance rockFall,
            Rectangle rect, 
            int[,] map, 
            Vector2 mapSize, 
            float maxTimer) 
            : 
            base(rect, txr)
        {
            _rectangle = rect;
            _texture = txr;
            _pixel = pixel;
            _light = light;
            _caveInInst = caveIn;
            _clearCaveInInst = clearCaveIn;
            _rockFallInst = rockFall; _rockFallInst.Volume = 0.1f;
            _map = map;
            _mapSize.X = mapSize.Y;
            _mapSize.Y = mapSize.X;
            _maxTime = (int)maxTimer;
            _timeSections = _maxTime / 4;
            _caveInToggleLock = new bool[25];
            _tileSheetSource = new Rectangle[(int)mapSize.X, (int)mapSize.Y];
            _lightRect = new Rectangle(0, 0, 1440, 449);
            _hitboxs = new Rectangle[14, 1000];
            _hitboxTypes = new Vector2[15];
            _caveInCounter = new float[25];
            _caveInCellTag = new Vector2[25];
            _caveInStage = new int[25];
            _baseColour = new Color(20, 20, 20, 255);




            _floors = new Vector2(0, 0);
            _ceilings = new Vector2(1, 0);
            _walls = new Vector2(2, 0);
            _platforms = new Vector2(3, 0);
            _ladders = new Vector2(4, 0);
            _levelEntrances = new Vector2(5, 0);
            _coalSeams = new Vector2(6, 0);
            _copperSeams = new Vector2(7, 0);
            _ironSeams = new Vector2(8, 0);
            _goldSeams = new Vector2(9, 0);
            _depositCart = new Vector2(10, 0);
            _ladderCaps = new Vector2(11, 0);
            _caveIns = new Vector2(12, 0);
            _caveInRubbleWall = new Vector2(13, 0);




            for (int i = 0; i < _mapSize.Y; i++)
            {
                for (int j = 0; j < _mapSize.X; j++)
                {
                    switch (_map[i, j])
                    {
                        #region Corridor - 1
                        case 1:
                            // Corridor Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 0),
                                0 + (_rectangle.Height * 0),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    _rectangle.Width,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     _rectangle.Width,
                                     40);
                            _ceilings.Y++;
                            break;
                        #endregion 


                        #region Floor - 2
                        case 2:
                            // Floor Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 1),
                                0 + (_rectangle.Height * 0),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    _rectangle.Width,
                                    40);
                            _floors.Y++;
                            break;
                        #endregion


                        #region Ceiling - 3
                        case 3:
                            // Ceilling Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 2),
                                0 + (_rectangle.Height * 0),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     _rectangle.Width,
                                     40);
                            _ceilings.Y++;
                            break;
                        #endregion


                        #region Dead End - 4
                        case 4:
                            // Dead End Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 3),
                                0 + (_rectangle.Height * 0),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i ) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 208,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Cavern Bottom - 5
                        case 5:
                            // Slope Left Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 4),
                                0 + (_rectangle.Height * 0),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    _rectangle.Width,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 16,
                                    64,
                                    40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Cavern Top - 6
                        case 6:
                            // Slope Right Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 5),
                                0 + (_rectangle.Height * 0),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 16,
                                     488,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     496);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Stone Block - 7
                        case 7:
                            // Stone Block Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 0),
                                0 + (_rectangle.Height * 1),
                                _rectangle.Width,
                                _rectangle.Height);
                            break;
                        #endregion


                        #region Level Entrance - 8
                        case 8:
                            // Level Entrance Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (+(_rectangle.Width * 1),
                                0 + (_rectangle.Height * 1),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    _rectangle.Width,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     _rectangle.Width,
                                     40);
                            _ceilings.Y++;

                            // Entrance Hitbox
                            _hitboxs[(int)_levelEntrances.X, (int)_levelEntrances.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 152,
                                     (_rectangle.Height * i) + 96,
                                     200,
                                     248);
                            _levelEntrances.Y++;

                            // 


                            break;
                        #endregion


                        #region Entrance Shaft Bottom - 9
                        case 9:
                            // Entrance Shaft Bottom Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 2),
                                0 + (_rectangle.Height * 1),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 24,
                                    (_rectangle.Height * i) + 344,
                                    488,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 16,
                                     64,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     384);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;

                            // Deposit Cart Hitbox
                            _hitboxs[(int)_depositCart.X, (int)_depositCart.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 112,
                                     (_rectangle.Height * i) + 152,
                                     240,
                                     152);
                            _depositCart.Y++;

                            // Level Start Postion
                            _startPosition = new Vector2(
                                (_rectangle.Width * j) + (_rectangle.Width / 2),
                                (_rectangle.Height * i) + (_rectangle.Height / 2));
                            break;
                        #endregion


                        #region Entrance Shaft Middle - 10
                        case 10:
                            // Entrance Shaft Middle Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 3),
                                0 + (_rectangle.Height * 1),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     512);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     512);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Iron - 11
                        case 11:
                            // Iron Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 4),
                                0 + (_rectangle.Height * 1),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 208,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;

                            // Ore Hitbox
                            _hitboxs[(int)_ironSeams.X, (int)_ironSeams.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 111,
                                     (_rectangle.Height * i) + 56,
                                     97,
                                     288);
                            _ironSeams.Y++;
                            break;
                        #endregion


                        #region Copper - 12
                        case 12:
                            // Copper Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 5),
                                0 + (_rectangle.Height * 1),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 208,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;

                            // Ore Hitbox
                            _hitboxs[(int)_copperSeams.X, (int)_copperSeams.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 111,
                                     (_rectangle.Height * i) + 56,
                                     97,
                                     288);
                            _copperSeams.Y++;
                            break;
                        #endregion


                        #region Ladder Bottom L - 13
                        case 13:
                            // Ladder Bottom L Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 0),
                                0 + (_rectangle.Height * 2),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    488,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 16,
                                    64,
                                    40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     384);
                            _walls.Y++;

                            // Ladder Hitbox
                            _hitboxs[(int)_ladders.X, (int)_ladders.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i),
                                     192,
                                     344);
                            _ladders.Y++;
                            break;
                        #endregion


                        #region Ladder Bottom T - 14
                        case 14:
                            // Ladder Bottom T Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 1),
                                0 + (_rectangle.Height * 2),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    _rectangle.Width,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox 1
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 16,
                                    64,
                                    40);
                            _ceilings.Y++;

                            // Ceiling Hitbox 2
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 16,
                                     64,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;

                            // Ladder Hitbox
                            _hitboxs[(int)_ladders.X, (int)_ladders.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i),
                                     192,
                                     344);
                            _ladders.Y++;
                            break;
                        #endregion


                        #region Ladder Middle - 15
                        case 15:
                            // Ladder Middle Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 2),
                                0 + (_rectangle.Height * 2),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     512);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     512);
                            _walls.Y++;

                            // Ladder Hitbox
                            _hitboxs[(int)_ladders.X, (int)_ladders.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i),
                                     192,
                                     512);
                            _ladders.Y++;
                            break;
                        #endregion


                        #region Ladder Top L - 16
                        case 16:
                            // Ladder Top L Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 3),
                                0 + (_rectangle.Height * 2),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    64,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) +16,
                                     488,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 344,
                                     40,
                                     168);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     496);
                            _walls.Y++;

                            // Platform Hitbox
                            _hitboxs[(int)_platforms.X, (int)_platforms.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 64,
                                     (_rectangle.Height * i) + 344,
                                     384,
                                     40);
                            _platforms.Y++;

                            // Ladder Hitbox
                            _hitboxs[(int)_ladders.X, (int)_ladders.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i) + 256,
                                     192,
                                     256);
                            _ladders.Y++;

                            // Ladder Cap Hitbox
                            _hitboxs[(int)_ladderCaps.X, (int)_ladderCaps.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i) + 232,
                                     192,
                                     24);
                            _ladderCaps.Y++;
                            break;
                        #endregion


                        #region Ladder Top T - 17
                        case 17:
                            // Ladder Top T Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 4),
                                0 + (_rectangle.Height * 2),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox 1
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    64,
                                    40);
                            _floors.Y++;

                            // Floor Hitbox 2
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 448,
                                    (_rectangle.Height * i) + 344,
                                    64,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     _rectangle.Width,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 344,
                                     40,
                                     168);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 344,
                                     40,
                                     168);
                            _walls.Y++;

                            // Platform Hitbox
                            _hitboxs[(int)_platforms.X, (int)_platforms.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 64,
                                     (_rectangle.Height * i) + 344,
                                     384,
                                     40);
                            _platforms.Y++;

                            // Ladder Hitbox
                            _hitboxs[(int)_ladders.X, (int)_ladders.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i) + 256,
                                     192,
                                     256);
                            _ladders.Y++;

                            // Ladder Cap Hitbox
                            _hitboxs[(int)_ladderCaps.X, (int)_ladderCaps.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i) + 232,
                                     192,
                                     24);
                            _ladderCaps.Y++;
                            break;
                        #endregion


                        #region Coal - 18
                        case 18:
                            // Coal Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 5),
                                0 + (_rectangle.Height * 2),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 208,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;

                            // Ore Hitbox
                            _hitboxs[(int)_coalSeams.X, (int)_coalSeams.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 111,
                                     (_rectangle.Height * i) + 56,
                                     97,
                                     288);
                            _coalSeams.Y++;
                            break;
                        #endregion


                        #region Shaft L Bottom - 19
                        case 19:
                            // Shaft L Bottom Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 0),
                                0 + (_rectangle.Height * 3),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    488,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 16,
                                    64,
                                    40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     56);

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     512);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Shaft T Bottom - 20
                        case 20:
                            // Shaft T Bottom
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 1),
                                0 + (_rectangle.Height * 3),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    _rectangle.Width,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox 1
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 16,
                                    64,
                                    40);
                            _ceilings.Y++;

                            // Ceiling Hitbox 2
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 16,
                                     64,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Shaft Middle - 21
                        case 21:
                            // Shaft Middle
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 2),
                                0 + (_rectangle.Height * 3),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     512);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     512);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Shaft L Top - 22
                        case 22:
                            // Shaft L Top
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 3),
                                0 + (_rectangle.Height * 3),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    64,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     448,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 344,
                                     40,
                                     168);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     512);
                            _walls.Y++;

                            // Platform Hitbox
                            _hitboxs[(int)_platforms.X, (int)_platforms.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 64,
                                     (_rectangle.Height * i) + 344,
                                     384,
                                     40);
                            _platforms.Y++;
                            break;
                        #endregion


                        #region Shaft T Top - 23
                        case 23:
                            // Shaft T Top Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 4),
                                0 + (_rectangle.Height * 3),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox 1
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    64,
                                    40);
                            _floors.Y++;

                            // Floor Hitbox 2
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 448,
                                    (_rectangle.Height * i) + 344,
                                    64,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     _rectangle.Width,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 344,
                                     40,
                                     168);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 344,
                                     40,
                                     168);
                            _walls.Y++;

                            // Platform Hitbox
                            _hitboxs[(int)_platforms.X, (int)_platforms.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 64,
                                     (_rectangle.Height * i) + 344,
                                     384,
                                     40);
                            _platforms.Y++;
                            break;
                        #endregion


                        #region Gold - 24
                        case 24:
                            // Gold Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 5),
                                0 + (_rectangle.Height * 3),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 208,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;

                            // Ore Hitbox
                            _hitboxs[(int)_goldSeams.X, (int)_goldSeams.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 111,
                                     (_rectangle.Height * i) + 56,
                                     97,
                                     288);
                            _goldSeams.Y++;
                            break;
                        #endregion


                        #region Dead End - Reversed - 25
                        case 25:
                            // Dead End - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 0),
                                0 + (_rectangle.Height * 4),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 264,
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Shaft L Bottom - Reversed - 26
                        case 26:
                            // Shaft L Bottom - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 1),
                                0 + (_rectangle.Height * 4),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 24,
                                    (_rectangle.Height * i) + 344,
                                    488,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 16,
                                     64,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     384);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;
                            break;
                        #endregion


                        #region Shaft L Top - Reversed - 27
                        case 27:
                            // Shaft L Top - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 2),
                                0 + (_rectangle.Height * 4),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 448,
                                    (_rectangle.Height * i) + 344,
                                    64,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 16,
                                     488,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     496);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 344,
                                     40,
                                     168);
                            _walls.Y++;

                            // Platform Hitbox
                            _hitboxs[(int)_platforms.X, (int)_platforms.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 64,
                                     (_rectangle.Height * i) + 344,
                                     384,
                                     40);
                            _platforms.Y++;
                            break;
                        #endregion


                        #region Coal - Reversed - 28
                        case 28:
                            // Coal - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 3),
                                0 + (_rectangle.Height * 4),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 264,
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;

                            // Ore Hitbox
                            _hitboxs[(int)_coalSeams.X, (int)_coalSeams.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 304,
                                     (_rectangle.Height * i) + 56,
                                     97,
                                     288);
                            _coalSeams.Y++;
                            break;
                        #endregion


                        #region Copper - Reversed - 29
                        case 29:
                            // Copper - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 4),
                                0 + (_rectangle.Height * 4),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 264,
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;

                            // Ore Hitbox
                            _hitboxs[(int)_copperSeams.X, (int)_copperSeams.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 304,
                                     (_rectangle.Height * i) + 56,
                                     97,
                                     288);
                            _copperSeams.Y++;
                            break;
                        #endregion


                        #region Gold - Reversed - 30
                        case 30:
                            // Gold - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 5),
                                0 + (_rectangle.Height * 4),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 264,
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;

                            // Ore Hitbox
                            _hitboxs[(int)_goldSeams.X, (int)_goldSeams.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 304,
                                     (_rectangle.Height * i) + 56,
                                     97,
                                     288);
                            _goldSeams.Y++;
                            break;
                        #endregion


                        #region Iron - Reversed - 31
                        case 31:
                            // Iron - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 0),
                                0 + (_rectangle.Height * 5),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 264,
                                    (_rectangle.Height * i) + 344,
                                    248,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     248,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 264,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     368);
                            _walls.Y++;

                            // Ore Hitbox
                            _hitboxs[(int)_ironSeams.X, (int)_ironSeams.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 304,
                                     (_rectangle.Height * i) + 56,
                                     97,
                                     288);
                            _ironSeams.Y++;
                            break;
                        #endregion


                        #region Lader L Bottom - Reversed - 32
                        case 32:
                            // Ladder L Bottom - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 1),
                                0 + (_rectangle.Height * 5),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 24,
                                    (_rectangle.Height * i) + 344,
                                    488,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i) + 16,
                                     64,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i),
                                     40,
                                     384);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 448,
                                     (_rectangle.Height * i),
                                     40,
                                     56);
                            _walls.Y++;

                            // Ladder Hitbox
                            _hitboxs[(int)_ladders.X, (int)_ladders.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i),
                                     192,
                                     344);
                            _ladders.Y++;
                            break;
                        #endregion


                        #region Ladder L Top - Reversed - 33
                        case 33:
                            // Ladder L Top - Reversed Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 2),
                                0 + (_rectangle.Height * 5),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 448,
                                    (_rectangle.Height * i) + 344,
                                    64,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 16,
                                     488,
                                     40);
                            _ceilings.Y++;

                            // Wall Hitbox 1
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 24,
                                     (_rectangle.Height * i) + 16,
                                     40,
                                     496);
                            _walls.Y++;

                            // Wall Hitbox 2
                            _hitboxs[(int)_walls.X, (int)_walls.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j) + 448,
                                    (_rectangle.Height * i) + 344,
                                    40,
                                    168);
                            _walls.Y++;

                            // Platform Hitbox
                            _hitboxs[(int)_platforms.X, (int)_platforms.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 64,
                                     (_rectangle.Height * i) + 344,
                                     384,
                                     40);
                            _platforms.Y++;

                            // Ladder Hitbox
                            _hitboxs[(int)_ladders.X, (int)_ladders.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i) + 256,
                                     192,
                                     256);
                            _ladders.Y++;

                            // Ladder Cap Hitbox
                            _hitboxs[(int)_ladderCaps.X, (int)_ladderCaps.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 160,
                                     (_rectangle.Height * i) + 232,
                                     192,
                                     24);
                            _ladderCaps.Y++;
                            break;
                        #endregion


                        #region Cave-In - 34
                        case 34:
                            // Cave-In Sprite
                            _tileSheetSource[i, j] = new Rectangle
                                (0 + (_rectangle.Width * 0),
                                0 + (_rectangle.Height * 6),
                                _rectangle.Width,
                                _rectangle.Height);

                            // Floor Hitbox
                            _hitboxs[(int)_floors.X, (int)_floors.Y] =
                                new Rectangle(
                                    (_rectangle.Width * j),
                                    (_rectangle.Height * i) + 344,
                                    _rectangle.Width,
                                    40);
                            _floors.Y++;

                            // Ceiling Hitbox
                            _hitboxs[(int)_ceilings.X, (int)_ceilings.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 16,
                                     _rectangle.Width,
                                     40);
                            _ceilings.Y++;

                            // Cave-in Hitbox
                            _hitboxs[(int)_caveIns.X, (int)_caveIns.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j),
                                     (_rectangle.Height * i) + 56,
                                     1,
                                     1);
                            _caveInCounter[(int)_caveIns.Y] = 0;
                            _caveInStage[(int)_caveIns.Y] = 0;
                            _caveInCellTag[(int)_caveIns.Y] = new Vector2(i, j);
                            _caveIns.Y++;

                            // Cave-in Wall Hitbox
                            _hitboxs[(int)_caveInRubbleWall.X, (int)_caveInRubbleWall.Y] =
                                 new Rectangle(
                                     (_rectangle.Width * j) + 64,
                                     (_rectangle.Height * i) + 16,
                                     1,
                                     1);
                            _caveInRubbleWall.Y++;
                            break;
                            #endregion
                    }



                    // Tile Trim - This Trims the border pixels and is needed to remove graphical artifacts from the drawn cells
                    _tileSheetSource[i, j] = new Rectangle(
                        _tileSheetSource[i, j].X + 1,
                        _tileSheetSource[i, j].Y + 1,
                        _tileSheetSource[i, j].Width - 2,
                        _tileSheetSource[i, j].Height - 2);
                }
            }

            _hitboxTypes = new Vector2[] { 
                _floors, 
                _ceilings, 
                _walls, 
                _platforms, 
                _ladders,
                _levelEntrances,
                _coalSeams,
                _copperSeams,
                _ironSeams,
                _goldSeams,
                _depositCart,
                _caveIns};
        }



        public virtual void updatemeHOME(bool[] levelsComplete)
        {
            for (int i = 0; i < _caveIns.Y; i++)
            {
                if (levelsComplete[i] == false)
                {
                    _hitboxs[12, i] = new Rectangle(
                        _hitboxs[12, i].X,
                        _hitboxs[12, i].Y,
                        512,
                        288);

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        368);

                    _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y] = new Rectangle(
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Width * 4,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Y,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Width,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Height);
                }
                else
                {
                    // < Cave-In Box Reset >
                    // ----- < Cave-In Interact >
                    _hitboxs[12, i] = new Rectangle(
                        _hitboxs[12, i].X,
                        _hitboxs[12, i].Y,
                        1,
                        1);
                    // ----- < Cave-In Wall >
                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        1,
                        1);
                    // ----- < Cave-In Sprite >
                    _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y] = new Rectangle(
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].X - _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Width,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Y,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Width,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Height);
                }
            }
        }



        public virtual void updatemeTUTORIAL(GameTime gt, float timer, int tutorialStage, bool tutorialFinished)
        {
            for (int i = 0; i < _caveIns.Y; i++)
            {
                if (i != 0 && tutorialFinished == true) _caveInCounter[i] += (float)gt.ElapsedGameTime.TotalSeconds;

                if ((int)timer % _timeSections == 0 && (int)timer != _maxTime && _entranceToggleLock == false)
                {
                    _caveInStage[0]++;
                    _entranceToggleLock = true;
                }
                else if ((int)timer % _timeSections != 0)
                    _entranceToggleLock = false;



                if (_caveInCounter[i] > 7)
                {
                    _caveInStage[i]++;
                    _caveInCounter[i] = 0;
                    if (_caveInStage[i] > 4)
                        _caveInStage[i] = 4;
                }
                if (_caveInStage[i] == 1)
                {
                    _rockFallInst.Play();

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        88);
                }
                if (_caveInStage[i] == 2)
                {
                    _rockFallInst.Play();

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        104);
                }
                if (_caveInStage[i] == 3)
                {
                    _rockFallInst.Play();

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        120);
                }
                if (_caveInStage[i] == 4)
                {
                    if (_caveInToggleLock[i] == false)
                    {
                        _caveInToggleLock[i] = true;
                        _caveInInst.Play();
                    }

                    _hitboxs[12, i] = new Rectangle(
                        _hitboxs[12, i].X,
                        _hitboxs[12, i].Y,
                        512,
                        288);

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        368);
                }
                _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y] = new Rectangle(
                        0 + (512 * _caveInStage[i]),
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Y,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Width,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Height);
            }
        }


        public virtual void updatemeLEVEL(GameTime gt, float timer)
        {
            for (int i = 0; i < _caveIns.Y; i++)
            {
                if (i != 0) _caveInCounter[i] += (float)gt.ElapsedGameTime.TotalSeconds;


                if ((int)timer % _timeSections == 0 && (int)timer != _maxTime && _entranceToggleLock == false)
                {
                    _caveInStage[0]++;
                    _entranceToggleLock = true;
                }
                else if ((int)timer % _timeSections != 0)
                    _entranceToggleLock = false;



                if (_caveInCounter[i] > 7)
                {
                    _caveInStage[i]++;
                    _caveInCounter[i] = 0;
                    if (_caveInStage[i] > 4)
                        _caveInStage[i] = 4;
                }
                if (_caveInStage[i] == 1)
                {
                    _rockFallInst.Play();

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        88);
                }
                if (_caveInStage[i] == 2)
                {
                    _rockFallInst.Play();

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        104);
                }
                if (_caveInStage[i] == 3)
                {
                    _rockFallInst.Play();

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        120);
                }
                if (_caveInStage[i] == 4)
                {
                    if (_caveInToggleLock[i] == false)
                    {
                        _caveInToggleLock[i] = true;
                        _caveInInst.Play();
                    }

                    _hitboxs[12, i] = new Rectangle(
                        _hitboxs[12, i].X,
                        _hitboxs[12, i].Y,
                        512,
                        288);

                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        384,
                        368);
                }
                _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y] = new Rectangle(
                        0 + (512 * _caveInStage[i]),
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Y,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Width,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Height);
            }
        }



        public void caveInClear(Rectangle interactedBox)
        {
            for (int i = 0; i < _caveIns.Y; i++)
            {
                if (_hitboxs[12, i] == interactedBox)
                {
                    _clearCaveInInst.Play();
                    _caveInToggleLock[i] = false;

                    // < Cave-In Box Reset >
                    // ----- < Cave-In Interact >
                    _hitboxs[12, i] = new Rectangle(
                        _hitboxs[12, i].X,
                        _hitboxs[12, i].Y,
                        1,
                        1);
                    // ----- < Cave-In Wall >
                    _hitboxs[13, i] = new Rectangle(
                        _hitboxs[13, i].X,
                        _hitboxs[13, i].Y,
                        1,
                        1);
                    // ----- < Cave-In Sprite >
                    _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y] = new Rectangle(
                        0,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Y,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Width,
                        _tileSheetSource[(int)_caveInCellTag[i].X, (int)_caveInCellTag[i].Y].Height);

                    // < Counter/Staging Reset >
                    _caveInStage[i] = 0;
                    _caveInCounter[i] = 0;
                }
            }
        }



        public void TutorialForceCavein(int section)
        {

            _forceCaveInToggleLock = true;

            _caveInStage[section] = 4;

            _hitboxs[12, section] = new Rectangle(
                        _hitboxs[12, section].X,
                        _hitboxs[12, section].Y,
                        512,
                        288);

            _hitboxs[13, section] = new Rectangle(
                _hitboxs[13, section].X,
                _hitboxs[13, section].Y,
                384,
                368);

            _tileSheetSource[(int)_caveInCellTag[section].X, (int)_caveInCellTag[section].Y] = new Rectangle(
                512 * 4,
                _tileSheetSource[(int)_caveInCellTag[section].X, (int)_caveInCellTag[section].Y].Y,
                _tileSheetSource[(int)_caveInCellTag[section].X, (int)_caveInCellTag[section].Y].Width,
                _tileSheetSource[(int)_caveInCellTag[section].X, (int)_caveInCellTag[section].Y].Height);
        }

        public void unlockToggleLock()
        {
            _forceCaveInToggleLock = false;
        }
        

         
        public override void drawme(SpriteBatch sb)
        {
            for (int i = 0; i < _mapSize.Y; i++)
            {
                for (int j = 0; j < _mapSize.X; j++)
                {
                    sb.Draw(
                        _pixel, 
                        new Rectangle(
                            _rectangle.Width * j,
                            _rectangle.Height * i,
                            _rectangle.Width,
                            _rectangle.Height), 
                        _tileSheetSource[i, j], _baseColour);

                    sb.Draw(
                        _texture,
                        new Rectangle(
                            _rectangle.Width * j,
                            _rectangle.Height * i,
                            _rectangle.Width,
                            _rectangle.Height),
                        _tileSheetSource[i, j], Color.White);
                }
            }
        }



        public void drawMask(SpriteBatch sb)
        {
            for (int i = 0; i < _levelEntrances.Y; i++)
            {
                sb.Draw(_light, 
                    new Rectangle(
                        _hitboxs[(int)_levelEntrances.X, i].Center.X - (_lightRect.Width / 2), 
                        _hitboxs[(int)_levelEntrances.X, i].Center.Y - (_lightRect.Height / 2),
                        _lightRect.Width,
                        _lightRect.Height), Color.White);
            } 
        }
    }
}
