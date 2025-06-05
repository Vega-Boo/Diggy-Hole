using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Diggy_Hole
{
    // < Game Camera >
    struct Camera2d
    {
        public Vector2 Position;
        public float Zoom;
        public Vector2 Velocity;


        public Matrix getCam()
        {
            Matrix temp;
            temp = Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
            temp *= Matrix.CreateScale(Zoom);
            return temp;
        }
    }



    public class Game1 : Game
    {
        // < Core > 
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        gameState _gameState = gameState.Splash;
        Rectangle _screen = new Rectangle(0, 0, 1920, 1080);
        Vector2 _screenSize = new Vector2(1920, 1080);
        bool _isGamePaused, _playedBefore;
        public static readonly Random RNG = new Random();
        SpriteFont _mainFont, _biggerMainFont;





        #region Player Varibles
        // < Players >
        // ----- < Objects >
        PlayerCharacters p1, p2, p3, p4;
        PlayerPointer p1Pointer, p2Pointer, p3Pointer, p4Pointer;
        BuyWheel p1BuyWheel, p2BuyWheel, p3BuyWheel, p4BuyWheel;
        InteractionBar p1InteractBar, p2InteractBar, p3InteractBar, p4InteractBar;

        // ----- < States >
        Items[] _p1Hand, _p2Hand, _p3Hand, _p4Hand;

        // ----- < Local Varibles >
        private int[] _activeSlot, _oldActiveSlot;
        private Rectangle _baseCharacterCell = new Rectangle(0, 0, 256, 256);
        private float[] _playerInteractTime, _playerInteractCounter;
        private bool p2Connected, p3Connected, p4Connected;
        private int _numberOfPlayers;



        // < Controls >
        // ----- < Mouse & Keyboard Controls >
        KeyboardState kb, oldKb, blankKB;
        MouseState curr_mouse, old_mouse, blankMouse;
        private bool _keyboardLastUsed;
        CursorHand cursorHand;

        // ----- < Gamepad Controls >
        GamePadState p1Pad, p2Pad, p3Pad, p4Pad, oldP1Pad, oldP2Pad, oldP3Pad, oldP4Pad;
        private bool _gamePadLastUsed;
        #endregion





        // < Camera >
        Camera2d cam;
        private Rectangle _playerCamArea = new Rectangle(0, 0, 1536, 864);
        private Rectangle _playerCamAreaSource = new Rectangle(0, 0, 1536, 864);
        private bool _singleplayerFixedCam;





        #region Level Varibles
        // < Levels >
        // ----- < Objects >
        PhysicalMap physicalMap;
        TutorialObject tutorialObject;
        FlashCard[] levelFlashCard = new FlashCard[5];

        // ----- < Lighting >
        private List<Lantern> lanterns;
        private RenderTarget2D _preCanvas, _lightMask;
        private Effect _lightShader;

        // ----- < Sound >
        // ----- // ----- < SFX Base >
        private SoundEffect _pickaxeHit, _shoveling, _walking, _depositCart, _clearCaveIn, _caveIn, _rockFall;
        // ----- // ----- < SFX Instance >
        private SoundEffectInstance _pickaxeInst, _shovelingInst, _walkingInst, _depositCartInst, _clearCaveInInst, _caveInInst, _rockFallInst;
        // ----- // ----- < Music >
        private Song _titleMusic, _levelBackgroundSounds;

        // ----- < General Local Varibles >
        private int _level, _money, _newMoney;
        private int[] _levelStars = new int[5];
        private int[,] _map;
        private float _currentTime, _newMoneyTransparentsy;
        private Rectangle _baseMapCell = new Rectangle(0, 0, 512, 512);
        private Color _defaltColour = new Color(48, 48, 48, 255);
        private Vector2 _lastLevelEntered = new Vector2(1280, 2304);
        private bool _loadLock;

        // ----- < Unique Level Varibles >
        // ----- // ----- < LevelSelect >
        private bool[] _levelsComplete = new bool[5];

        private Vector2[] _levelSelectLights = new Vector2[3]
        {
            new Vector2(2, 4),
            new Vector2(9, 6),
            new Vector2(7, 9),
        };

        /* Level Select Map
        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 9, 1, 8, 1,34, 1, 8,17,18, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7, 7, 7,15, 7, 7, 7, 7},
        {7, 7, 7, 7,29,17, 8, 34,1,14, 1,18, 7, 7},
        {7, 7, 7, 7, 7,32,34,17, 8,11, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7,33,34,13, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7,32, 1, 8,24, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        */

        // ----- // ----- < Tutorial >
        private int _tutorialTime = 180;
        private int _tutorialGoal1 = 15;
        private int _tutorialGoal2 = 20;
        private int _tutorialGoal3 = 25;

        private Vector2[] _tutorialLights = new Vector2[6]
        {
            new Vector2(7, 3),
            new Vector2(2, 4),
            new Vector2(6, 4),
            new Vector2(9, 4),
            new Vector2(4, 6),
            new Vector2(6, 6),
        };

        /* Tutorial Map
        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7,10, 7, 7, 7, 7,27, 1,16, 7, 7, 7},
        {7, 7, 9, 1,34, 1,17,20,34,14,18, 7, 7},
        {7, 7, 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7},
        {7, 7, 7,29, 1,34,14, 1, 4, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        */

        // ----- // ----- < 2 >
        private int _level2Time = 230;
        private int[] _level2Goal1 = new int[2] { 4, 2 };
        private int[] _level2Goal2 = new int[2] { 6, 3 };
        private int[] _level2Goal3 = new int[2] { 8, 4 };
        private int[] _level2OreCollected = new int[2];

        private Vector2[] _level2Lights = new Vector2[4]
        {
            new Vector2(5, 3),
            new Vector2(2, 4),
            new Vector2(4, 4),
            new Vector2(7, 5),
        };

        /* Level 2 Map
        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7,10, 7,33,18, 7, 7, 7, 7, 7},
        { 7, 7,10, 7,32,23, 1,16, 7, 7, 7},
        { 7, 7, 9,34,23,19, 7,15, 7, 7, 7},
        { 7, 7, 7,29,20,34, 1,14, 4, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        */

        // ----- // ----- < 3 >
        private int _level3Time = 250;
        private Vector2[] _level3Spawns = new Vector2[22]
        {
            new Vector2(6, 2),
            new Vector2(7, 4),
            new Vector2(8, 4),
            new Vector2(9, 4),
            new Vector2(11, 4),
            new Vector2(6, 5),
            new Vector2(13, 6),
            new Vector2(8, 7),
            new Vector2(9, 7),
            new Vector2(5, 8),
            new Vector2(6, 8),
            new Vector2(11, 8),
            new Vector2(4, 9),
            new Vector2(13, 9),
            new Vector2(4, 10),
            new Vector2(5, 10),
            new Vector2(6, 10),
            new Vector2(8, 10),
            new Vector2(10, 10),
            new Vector2(11, 10),
            new Vector2(7, 12),
            new Vector2(9, 12),
        };

        private int[] _level3NumberOfOres = new int[4] { 30, 25, 18, 7 };
        private int _level3OreCollected;
        private int _level3Goal1 = 10;
        private int _level3Goal2 = 15;
        private int _level3Goal3 = 20;

        private Vector2[] _level3Lights = new Vector2[4]
        {
            new Vector2(3, 6),
            new Vector2(7, 6),
            new Vector2(13, 6),
            new Vector2(9, 12),
        };

        /* Level 3 Map
        {7, 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7,10, 7,33, 1, 4, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7,10, 7,15, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7,10, 7,32,23, 1, 1, 1,17, 1, 4, 7, 7, 7, 7},
        {7, 7, 7,10, 7, 7,26,22, 7, 7,15, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 9, 1,34,17,20,16,33,14,17,34, 1, 4, 7, 7},
        {7, 7, 7, 7, 7, 7,15, 7,32,13, 7,15, 7, 7, 7, 7, 7},
        {7, 7,25,34,23, 1,14, 4, 7, 7,33,14,34,22, 7, 7, 7},
        {7, 7, 7, 7,26,22, 7, 7, 7, 7,15, 7,27,19, 7, 7, 7},
        {7, 7, 7,25, 1,20, 1,17, 1,34,14, 1,19, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7,25,14,34, 1, 4, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        */

        // ----- // ----- < 4 >
        private int _level4Time = 400;
        private int[] _level4Goal1 = new int[4] { 4, 2, 1, 25};
        private int[] _level4Goal2 = new int[4] { 6, 3, 2, 40};
        private int[] _level4Goal3 = new int[4] { 8, 4, 4, 60};
        private int[] _level4OreCollected = new int[3];

        private Vector2[] _level4Lights = new Vector2[3]
        {
            new Vector2(2, 3),
            new Vector2(9, 5),
            new Vector2(6, 7),
        };

        /* Level 4 Map
        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7, 9,34,16, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7, 7, 7,32,16,31,23,34, 1,34,17, 4, 7, 7},
        { 7, 7, 7, 7, 7,15, 7,21, 7, 7,33,14,16, 7, 7},
        { 7, 7, 7, 7, 7,32, 1,20,17, 1,14, 4,15, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7, 7,15, 7, 7,33,13, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7,28,14,34,34,14,12, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        */

        // ----- // ----- < 5 >
        private int _level5Time = 640;
        private int[] _level5Goal1 = new int[5] { 4, 4, 2, 1, 50 };
        private int[] _level5Goal2 = new int[5] { 6, 5, 3, 2, 80 };
        private int[] _level5Goal3 = new int[5] { 8, 6, 4, 4, 110 };
        private int[] _level5OreCollected = new int[4];

        private Vector2[] _level5Lights = new Vector2[1]
        {
            new Vector2(2, 3),
        };

        /* Level 5 Map
        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7, 9,34,16, 7,33,34,23,12, 7, 7, 7, 7, 7},
        { 7, 7, 7, 7,15, 7,15, 7,21, 7, 7, 7, 7, 7, 7},
        { 7, 7,33,34,14, 1,14, 1,20,18, 7, 7, 7, 7, 7},
        { 7, 7,15, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7,32, 1,17, 1,17, 4,27, 1,16, 7, 7, 7, 7},
        { 7, 7, 7, 7,15, 7,32, 1,19, 7,15, 7, 7, 7, 7},
        { 7, 7,31,34,14, 1,34,22, 7,33,13, 7, 7, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7,21, 7,15, 7, 7, 7, 7, 7},
        { 7, 7, 7, 7, 7, 7,30,20,34,13, 7, 7, 7, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
        */
        #endregion





        // < Misc Objects & Varibles >
        SplashSequence splashSequence;
        titleCard titleCard;
        List<PysicalItem> pysicalItem;
        P1Crown p1Crown;
        ProgressManager progressManager;
        readonly string levelProgressFilename = "levelProgress.lst";





        // < Pause Menu >
        Texture2D _overlay, _controlsTexture;
        bool _isEscapePressed, _controlScreenUp;
        SpriteFont _pauseFont;





        // < Screen Transition >
        WipeTransition wipeTransition;
        bool _transitioning;
        gameState _targetScreen;
        int _targetLevel;





        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }



        protected override void Initialize()
        {
            // < Set Window Size >
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();



            // < Build Lighting Canvases >
            var pp = GraphicsDevice.PresentationParameters;
            _preCanvas = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            _lightMask = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            lanterns = new List<Lantern>();



            // < Misc >
            cam.Position = Vector2.Zero;
            old_mouse = curr_mouse = Mouse.GetState();
            pysicalItem = new List<PysicalItem>();



            // < Player Initalization >
            // ----- < Build Hands >
            _p1Hand = new Items[3];
            _p2Hand = new Items[3];
            _p3Hand = new Items[3];
            _p4Hand = new Items[3];

            // ----- < Build Active Slots >
            _activeSlot = new int[4] { 0, 0, 0, 0 };
            _oldActiveSlot = new int[4] { 0, 0, 0, 0 };

            // ----- < Set Hand Defalts >
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    _p1Hand[j] = Items.Bare;
                    _p2Hand[j] = Items.Bare;
                    _p3Hand[j] = Items.Bare;
                    _p4Hand[j] = Items.Bare;
                }
            }

            // ----- < Build Interaction >
            _playerInteractCounter = new float[4];
            _playerInteractTime = new float[4];



            // < Load Player Progress >
            progressManager = new ProgressManager(levelProgressFilename);

            for (int i = 0; i < 5; i++)
            {
                _levelsComplete[i] = progressManager.Data.LevelsComplete[i];
                _levelStars[i] = progressManager.Data.LevelStars[i];
                _playedBefore = progressManager.Data.PlayedBefore;
            }



            // < Check for Connected Controllers >
            if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                p2Connected = true;
            if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                p3Connected = true;
            if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                p4Connected = true;

            base.Initialize();
        }





        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // < Functional Objects >
            splashSequence = new SplashSequence(
                _screen,
                Content.Load<Texture2D>("Screens/screenBackground"),
                Content.Load<Texture2D>("Screens/MonoGame2"),
                Content.Load<Texture2D>("Screens/Vega"));

            tutorialObject = new TutorialObject(
                new Rectangle(10, 578, 421, 452),
                Content.Load<Texture2D>("Tutorial/Miner Mike Scaled"),
                Content.Load<SpriteFont>("Fonts/MainFont"));

            titleCard = new titleCard(
                _screen,
                Content.Load<Texture2D>("Screens/screenBackground"),
                Content.Load<Texture2D>("Screens/Title2"),
                Content.Load<SpriteFont>("Fonts/MainFont"));

            p1Crown = new P1Crown(
                new Rectangle(0, 0, 168, 144),
                Content.Load<Texture2D>("HUD/Crown"));

            // < Textures >
            _controlsTexture = Content.Load<Texture2D>("Screens/ControlScheme");

            // < Game Fonts >
            _mainFont = Content.Load<SpriteFont>("Fonts/MainFont");
            _biggerMainFont = Content.Load<SpriteFont>("Fonts/BiggestFont");
            _pauseFont = Content.Load<SpriteFont>("Fonts/PausedFont");

            // < Shaders >
            _overlay = Content.Load<Texture2D>("Debug/pixel");
            _lightShader = Content.Load<Effect>("Shaders/LightShader");
             
            // < Sound >
            // ----- < Music >
            _titleMusic = Content.Load<Song>("Music/music1");
            _levelBackgroundSounds = Content.Load<Song>("Music/mineAmbience");

            // ----- < SFX >
            _caveIn = Content.Load<SoundEffect>("SFX/caveIn");
            _clearCaveIn = Content.Load<SoundEffect>("SFX/clearCaveIn");
            _depositCart = Content.Load<SoundEffect>("SFX/depositCart");
            _pickaxeHit = Content.Load<SoundEffect>("SFX/pickaxeBlow");
            _shoveling = Content.Load<SoundEffect>("SFX/shoveling");
            _walking = Content.Load<SoundEffect>("SFX/walking2");
            _rockFall = Content.Load<SoundEffect>("SFX/rockFall");

            // ----- < SFX Instance >
            _caveInInst = _caveIn.CreateInstance(); _caveInInst.Volume = 0.35f;
            _clearCaveInInst = _clearCaveIn.CreateInstance(); _clearCaveInInst.Volume = 0.40f;
            _depositCartInst = _depositCart.CreateInstance(); _depositCartInst.Volume = 0.20f;
            _pickaxeInst = _pickaxeHit.CreateInstance(); _pickaxeInst.Volume = 0.50f;
            _shovelingInst = _shoveling.CreateInstance(); _shovelingInst.Volume = 0.65f;
            _walkingInst = _walking.CreateInstance(); _walkingInst.Volume = 0.10f;
            _rockFallInst = _rockFall.CreateInstance(); _rockFallInst.Volume = 0.10f;

            // < Build Inital Level >
            levelSwitch(0);
        }





        protected override void Update(GameTime gameTime)
        {
            // < Gather Player Input >
            kb = Keyboard.GetState();
            curr_mouse = Mouse.GetState();
            p1Pad = GamePad.GetState(PlayerIndex.One);
            p2Pad = GamePad.GetState(PlayerIndex.Two);
            p3Pad = GamePad.GetState(PlayerIndex.Three);
            p4Pad = GamePad.GetState(PlayerIndex.Four);



            #region Player 1 Master Controls
            // < Pause Toggle >
            if ((kb.IsKeyDown(Keys.Escape) && oldKb.IsKeyUp(Keys.Escape)) || (p1Pad.Buttons.Start == ButtonState.Pressed && oldP1Pad.Buttons.Start == ButtonState.Released))
            {
                _isGamePaused = !_isGamePaused;
            }
            // < Pause Exit >
            if (((kb.IsKeyDown(Keys.E) && oldKb.IsKeyUp(Keys.E)) || (p1Pad.Buttons.A == ButtonState.Pressed && oldP1Pad.Buttons.A == ButtonState.Released)) && _isGamePaused == true)
            {
                if (_gameState == gameState.LevelSelect)
                    Transition(gameState.Title, 0);
                if (_gameState != gameState.LevelSelect)
                    Transition(gameState.LevelSelect, 0);
            }
            // < Restart >
            if (((kb.IsKeyDown(Keys.R) && oldKb.IsKeyUp(Keys.R)) || (p1Pad.Buttons.B == ButtonState.Pressed && oldP1Pad.Buttons.B == ButtonState.Released)) && _isGamePaused == true && _gameState != gameState.LevelSelect)
            {
                Transition(_targetScreen, _targetLevel);
            }
            // < Control Pop-up Control >
            if (((kb.IsKeyDown(Keys.F) && oldKb.IsKeyUp(Keys.F)) || (p1Pad.Buttons.Y == ButtonState.Pressed && oldP1Pad.Buttons.Y == ButtonState.Released)) && _isGamePaused == true)
            {
                _controlScreenUp = !_controlScreenUp;
            }
            #endregion


            // < Update Switchboard >
            switch (_gameState)
            {
                case gameState.Splash:
                    updateSplash(gameTime);
                    break;

                case gameState.Title:
                    updateTitle(gameTime);
                    break;

                case gameState.LevelSelect:
                    updateLevelSelect(gameTime);
                    break;

                case gameState.Tutorial:
                    updateTutorial(gameTime);
                    break;

                case gameState.Level:
                    updateLevel(gameTime);
                    break;
            }



            // < Screen/Level Transition Update Control >
            if (_transitioning == true)
            {
                wipeTransition.updateme(gameTime);

                // < Screen/Level Switch >
                if (wipeTransition.screenLock == true && _loadLock == false)
                {
                    _gameState = _targetScreen;
                    levelSwitch(_targetLevel);
                    _loadLock = true;
                    _isGamePaused = false;
                }    

                // < Transition Unlock >
                if (wipeTransition.wipeFinished == true)
                {
                    _transitioning = false;
                    _loadLock = false;
                }
            }



            // < Backing Track Control >
            if (_gameState == gameState.Title && MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Volume = 0.30f;
                MediaPlayer.Play(_titleMusic);
            }
            if ((_gameState == gameState.LevelSelect || _gameState == gameState.Tutorial || _gameState == gameState.Level) && MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Volume = 0.20f;
                MediaPlayer.Play(_levelBackgroundSounds);
            }



            // < What Input Was Last Used >
            // ----- < Was the Keyboard Last Used? >
            if ((kb != oldKb || curr_mouse != old_mouse) && p1Pad == oldP1Pad && p2Connected == false && p3Connected == false && p4Connected == false)
            {
                _keyboardLastUsed = true;
                _gamePadLastUsed = false;
            }
            // ----- < Was the Gamepad Last Used? >
            if ((kb == oldKb  || curr_mouse == old_mouse) && p1Pad != oldP1Pad)
            {
                _gamePadLastUsed = true;
                _keyboardLastUsed = false;
            }



            // < Check number of Players >
            int p2, p3, p4;
            if (p2Connected == true)
                p2 = 1;
            else
                p2 = 0;
            if (p3Connected == true)
                p3 = 1;
            else
                p3 = 0;
            if (p4Connected == true)
                p4 = 1;
            else
                p4 = 0;
            _numberOfPlayers = 1 + p2 + p3 + p4;



            // < New -> Old Varible Updates >
            oldP1Pad = p1Pad;
            oldP2Pad = p2Pad;
            oldP3Pad = p3Pad;
            oldP4Pad = p4Pad;
            _oldActiveSlot = _activeSlot;
            old_mouse = curr_mouse;
            oldKb = kb;



            base.Update(gameTime);
        }





        void updateSplash(GameTime gameTime)
        {
            // < Splash Screen Control >
            splashSequence.updateme(gameTime);

            // < Splash End Transition >
            if (splashSequence.finishedSplash == true && _transitioning == false)
            {
                Transition(gameState.Title, 0);
            }
        }





        void updateTitle(GameTime gameTime)
        {
            // < Title Screen Control >
            titleCard.updateme(gameTime);



            // < Title Screen Music Control >
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(_titleMusic);
            }



            // < Title Screen Navigation Control >
            // ----- < Start Game >
            if ((kb.IsKeyDown(Keys.Space) || p1Pad.IsButtonDown(Buttons.A)) && _transitioning == false)
            {
                if (_playedBefore == true)
                    Transition(gameState.LevelSelect, 0);
                if (_playedBefore == false)
                    Transition(gameState.Tutorial, 1);
            }
            // ----- < Save & Exit Game >
            if (p1Pad.Buttons.B == ButtonState.Pressed || kb.IsKeyDown(Keys.Escape))
            {
                progressManager.addData(_levelsComplete, _levelStars, _playedBefore);
                progressManager.Save();
                Exit();
            }
            // ----- < Reset Game Progress >
            if (p1Pad.Buttons.Y == ButtonState.Pressed || kb.IsKeyDown(Keys.R))
            {
                ResetGame();
                if (_transitioning == false)
                    Transition(gameState.Splash, 0);
            }         



            // < Stylised Mouse Cursor >
            cursorHand.updateme(curr_mouse, _p1Hand[_activeSlot[0]]);
        }





        void updateLevelSelect(GameTime gameTime)
        {
            // < Ambiant Sound Control >
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(_levelBackgroundSounds);
            }



            // < Camera >
            // ----- < Multiplayer Camera >
            if (p2Connected == true || p3Connected == true || p4Connected == true)
            {
                // ----- / ----- < Multiplayer Camera Calculation Varibles >
                Vector2 playerCentrePoint = p1.SourcePoint;
                var min = p1.SourcePoint;
                var max = p1.SourcePoint;
                int playersInPlay = 0;

                // ----- / ----- < Gather Player Points >
                min = Vector2.Min(min, p1.SourcePoint);
                max = Vector2.Max(max, p1.SourcePoint);
                playerCentrePoint = p1.SourcePoint;
                playersInPlay++;

                if (p2Connected == true)
                {
                    min = Vector2.Min(min, p2.SourcePoint);
                    max = Vector2.Max(max, p2.SourcePoint);
                    playerCentrePoint += p2.SourcePoint;
                    playersInPlay++;
                }
                if (p3Connected == true)
                {
                    min = Vector2.Min(min, p3.SourcePoint);
                    max = Vector2.Max(max, p3.SourcePoint);
                    playerCentrePoint += p3.SourcePoint;
                    playersInPlay++;
                }
                if (p4Connected == true)
                {
                    min = Vector2.Min(min, p4.SourcePoint);
                    max = Vector2.Max(max, p4.SourcePoint);
                    playerCentrePoint += p4.SourcePoint;
                    playersInPlay++;
                }

                // ----- / ----- < Generate Camera Zoning > 
                var boundsSize = max - min;
                playerCentrePoint /= playersInPlay;
                var vZoom = (_screenSize - new Vector2(768, 432)) / boundsSize;

                // ----- / ----- / ----- < Move to Target Zoom > 
                float targetZoom = MathHelper.Clamp(MathF.Min(vZoom.X, vZoom.Y), 0.2f, 1);
                float diferance = cam.Zoom - targetZoom;
                cam.Zoom -= diferance * 0.05f;

                // ----- / ----- / ----- < Move to Target Position > 
                Vector2 camCentre = new Vector2(
                    (-playerCentrePoint.X + _screen.Width / (2 * cam.Zoom)),
                    (-playerCentrePoint.Y + _screen.Height / (2 * cam.Zoom)));

                Vector2 direction = new Vector2(
                    camCentre.X - cam.Position.X,
                    camCentre.Y - cam.Position.Y);

                cam.Velocity += direction * 0.05f;
                cam.Position += cam.Velocity;
                cam.Velocity *= 0.1f;
            }
            else
            {
                // ----- < Single Player Cam >
                cam.Zoom = 1;
                cam.Position.X = (-p1.SourcePoint.X + _screen.Width / (2 * cam.Zoom));
                cam.Position.Y = (-p1.SourcePoint.Y + _screen.Height / (2 * cam.Zoom));
            }





            // < Level Control >
            physicalMap.updatemeHOME(_levelsComplete);





            // < Level Information Control >
            for (int i = 0; i < levelFlashCard.Length; i++)
            {
                levelFlashCard[i].updateme(gameTime, p1.Hitbox);
            }


            if (_isGamePaused == false && _transitioning == false)
            {
                #region Player 1
                #region Player 1 Controls
                // < Level Enter Controls >
                // ----- < Enter Tutorial >
                if (p1.Hitbox.Intersects(physicalMap.Hitboxs[5, 0]) && _transitioning == false)
                {
                    _lastLevelEntered.X = physicalMap.Hitboxs[5, 0].Center.X;
                    _lastLevelEntered.Y = physicalMap.Hitboxs[5, 0].Center.Y + 36;

                    if ((kb.IsKeyDown(Keys.E) && oldKb.IsKeyUp(Keys.E)) || (p1Pad.Buttons.X == ButtonState.Pressed && oldP1Pad.Buttons.X == ButtonState.Released))
                        Transition(gameState.Tutorial, 1);
                }
                // ----- < Enter Level 2 >
                if (p1.Hitbox.Intersects(physicalMap.Hitboxs[5, 1]) && _transitioning == false)
                {
                    _lastLevelEntered.X = physicalMap.Hitboxs[5, 1].Center.X;
                    _lastLevelEntered.Y = physicalMap.Hitboxs[5, 1].Center.Y + 36;

                    if ((kb.IsKeyDown(Keys.E) && oldKb.IsKeyUp(Keys.E)) || (p1Pad.Buttons.X == ButtonState.Pressed && oldP1Pad.Buttons.X == ButtonState.Released))
                        Transition(gameState.Level, 2);
                }
                // ----- < Enter Level 3 >
                if (p1.Hitbox.Intersects(physicalMap.Hitboxs[5, 2]) && _transitioning == false)
                {
                    _lastLevelEntered.X = physicalMap.Hitboxs[5, 2].Center.X;
                    _lastLevelEntered.Y = physicalMap.Hitboxs[5, 2].Center.Y + 36;

                    if ((kb.IsKeyDown(Keys.E) && oldKb.IsKeyUp(Keys.E)) || (p1Pad.Buttons.X == ButtonState.Pressed && oldP1Pad.Buttons.X == ButtonState.Released))
                        Transition(gameState.Level, 3);
                }
                // ----- < Enter Level 4 >
                if (p1.Hitbox.Intersects(physicalMap.Hitboxs[5, 3]) && _transitioning == false)
                {
                    _lastLevelEntered.X = physicalMap.Hitboxs[5, 3].Center.X;
                    _lastLevelEntered.Y = physicalMap.Hitboxs[5, 3].Center.Y + 36;

                    if ((kb.IsKeyDown(Keys.E) && oldKb.IsKeyUp(Keys.E)) || (p1Pad.Buttons.X == ButtonState.Pressed && oldP1Pad.Buttons.X == ButtonState.Released))
                        Transition(gameState.Level, 4);
                }
                // ----- < Enter Level 5 >
                if (p1.Hitbox.Intersects(physicalMap.Hitboxs[5, 4]) && _transitioning == false)
                {
                    _lastLevelEntered.X = physicalMap.Hitboxs[5, 4].Center.X;
                    _lastLevelEntered.Y = physicalMap.Hitboxs[5, 4].Center.Y + 36;

                    if ((kb.IsKeyDown(Keys.E) && oldKb.IsKeyUp(Keys.E)) || (p1Pad.Buttons.X == ButtonState.Pressed && oldP1Pad.Buttons.X == ButtonState.Released))
                        Transition(gameState.Level, 5);
                }
                #endregion



                // < Player Update >
                p1.updateme(kb, oldKb, curr_mouse, p1Pad, gameTime, physicalMap.Hitboxs, _p1Hand[_activeSlot[0]]);
                p1Crown.updateme(p1.Hitbox);
                #endregion



                #region Player 2
                // < Add/Remove Player >
                // ----- < Add Player >
                if (p2Pad.Buttons.X == ButtonState.Pressed && oldP2Pad.Buttons.X != ButtonState.Pressed && !p2Connected)
                {
                    p2Connected = true;

                    Vector2 charLoadPos;
                    if (_level == 0)
                        charLoadPos = _lastLevelEntered;
                    else
                        charLoadPos = physicalMap.StartPosition;

                    int r = RNG.Next(-168, 168);

                    p2 = new PlayerCharacters(
                       Content.Load<Texture2D>("Dwarfs/2"),
                       Content.Load<Texture2D>("Dwarfs/playerBaseLight"),
                       Content.Load<Texture2D>("Dwarfs/HeadLamp2"),
                       _pickaxeHit,
                       _shoveling,
                       _walking,
                       4,
                       _baseCharacterCell,
                       new Rectangle(
                           ((int)charLoadPos.X - 128) + r,
                           ((int)charLoadPos.Y - 128),
                           _baseCharacterCell.Width,
                           _baseCharacterCell.Height),
                       _p1Hand[_activeSlot[1]]);

                    p2Pointer = new PlayerPointer(
                        new Rectangle(
                            0,
                            0,
                            128,
                            128),
                        Content.Load<Texture2D>("HUD/Item Sheet"),
                        Content.Load<Texture2D>("HUD/Pointer"),
                        Content.Load<SpriteFont>("Fonts/MainFont"),
                        new Color(78, 100, 182, 255),
                        2);

                    p2InteractBar = new InteractionBar(
                        new Rectangle(0, 0, 256, 40),
                        Content.Load<Texture2D>("HUD/Bar"),
                        Content.Load<Texture2D>("Debug/pixel"),
                        new Color(78, 100, 182, 255));

                    p2BuyWheel = new BuyWheel(
                        new Rectangle(0, 0, 512, 512),
                        Content.Load<Texture2D>("HUD/WheelSheet"),
                        Content.Load<Texture2D>("HUD/ItemWheel"),
                        new Color(78, 100, 182, 255),
                        Content.Load<SpriteFont>("Fonts/MainFont"));
                }
                // ----- < Remove Player >
                if (p2Pad.Buttons.B == ButtonState.Pressed)
                {
                    p2Connected = false;
                }



                // < Player Update >
                if (p2Connected)
                    p2.updateme(blankKB, blankKB, blankMouse, p2Pad, gameTime, physicalMap.Hitboxs, _p2Hand[_activeSlot[1]]);
                #endregion



                #region Player 3
                // < Add/Remove Player >
                // ----- < Add Player >
                if (p3Pad.IsButtonDown(Buttons.X) && oldP3Pad.Buttons.X != ButtonState.Pressed && !p3Connected)
                {
                    p3Connected = true;

                    Vector2 charLoadPos;
                    if (_level == 0)
                        charLoadPos = _lastLevelEntered;
                    else
                        charLoadPos = physicalMap.StartPosition;

                    int r = RNG.Next(-168, 168);

                    p3 = new PlayerCharacters(
                       Content.Load<Texture2D>("Dwarfs/3"),
                       Content.Load<Texture2D>("Dwarfs/playerBaseLight"),
                       Content.Load<Texture2D>("Dwarfs/HeadLamp2"), 
                       _pickaxeHit,
                       _shoveling,
                       _walking,
                       4,
                       _baseCharacterCell,
                       new Rectangle(
                           ((int)charLoadPos.X - 128) + r,
                           ((int)charLoadPos.Y - 128),
                           _baseCharacterCell.Width,
                           _baseCharacterCell.Height),
                       _p1Hand[_activeSlot[2]]);

                    p3Pointer = new PlayerPointer(
                        new Rectangle(
                             0,
                             0,
                             128,
                             128),
                        Content.Load<Texture2D>("HUD/Item Sheet"),
                        Content.Load<Texture2D>("HUD/Pointer"),
                        Content.Load<SpriteFont>("Fonts/MainFont"),
                        new Color(108, 63, 125, 255),
                        3);

                    p3InteractBar = new InteractionBar(
                        new Rectangle(0, 0, 256, 40),
                        Content.Load<Texture2D>("HUD/Bar"),
                        Content.Load<Texture2D>("Debug/pixel"),
                        new Color(108, 63, 125, 255));

                    p3BuyWheel = new BuyWheel(
                        new Rectangle(0, 0, 512, 512),
                        Content.Load<Texture2D>("HUD/WheelSheet"),
                        Content.Load<Texture2D>("HUD/ItemWheel"),
                        new Color(108, 63, 125, 255),
                        Content.Load<SpriteFont>("Fonts/MainFont"));
                }
                // ----- < Remove Player >
                if (p3Pad.Buttons.B == ButtonState.Pressed)
                {
                    p3Connected = false;
                }



                // < Player Update >
                if (p3Connected)
                    p3.updateme(blankKB, blankKB, blankMouse, p3Pad, gameTime, physicalMap.Hitboxs, _p3Hand[_activeSlot[2]]);
                #endregion



                #region Player 4
                // < Add/Remove Player >
                // ----- < Add Player >
                if (p4Pad.IsButtonDown(Buttons.X) && oldP4Pad.Buttons.X != ButtonState.Pressed && !p4Connected)
                {
                    p4Connected = true;

                    Vector2 charLoadPos;
                    if (_level == 0)
                        charLoadPos = _lastLevelEntered;
                    else
                        charLoadPos = physicalMap.StartPosition;

                    int r = RNG.Next(-168, 168);

                    p4 = new PlayerCharacters(
                       Content.Load<Texture2D>("Dwarfs/4"),
                       Content.Load<Texture2D>("Dwarfs/playerBaseLight"),
                       Content.Load<Texture2D>("Dwarfs/HeadLamp2"), 
                       _pickaxeHit,
                       _shoveling,
                       _walking,
                       4,
                       _baseCharacterCell,
                       new Rectangle(
                           ((int)charLoadPos.X - 128) + r,
                           ((int)charLoadPos.Y - 128),
                           _baseCharacterCell.Width,
                           _baseCharacterCell.Height),
                       _p1Hand[_activeSlot[3]]);

                    p4Pointer = new PlayerPointer(
                        new Rectangle(
                            0,
                            0,
                            128,
                            128),
                        Content.Load<Texture2D>("HUD/Item Sheet"),
                        Content.Load<Texture2D>("HUD/Pointer"),
                        Content.Load<SpriteFont>("Fonts/MainFont"),
                        new Color(229, 221, 57, 255),
                        4);

                    p4InteractBar = new InteractionBar(
                        new Rectangle(0, 0, 256, 40),
                        Content.Load<Texture2D>("HUD/Bar"),
                        Content.Load<Texture2D>("Debug/pixel"),
                        new Color(229, 221, 57, 255));

                    p4BuyWheel = new BuyWheel(
                        new Rectangle(0, 0, 512, 512),
                        Content.Load<Texture2D>("HUD/WheelSheet"),
                        Content.Load<Texture2D>("HUD/ItemWheel"),
                        new Color(229, 221, 57, 255),
                        Content.Load<SpriteFont>("Fonts/MainFont"));
                }
                // ----- < Remove Player >
                if (p4Pad.IsButtonDown(Buttons.B))
                {
                    p4Connected = false;
                }



                // < Player Update >
                if (p4Connected)
                    p4.updateme(blankKB, blankKB, blankMouse, p4Pad, gameTime, physicalMap.Hitboxs, _p3Hand[_activeSlot[3]]);
                #endregion



                // < World Items Update >
                for (int i = 0; i < pysicalItem.Count; i++)
                {
                    pysicalItem[i].updateme(physicalMap.Hitboxs);
                }

                

                #region Player 1 Hud
                p1Pointer.updateme(gameTime, p1.Hitbox, _p1Hand, kb, p1Pad, curr_mouse, physicalMap.Hitboxs, p1.ActionState, _keyboardLastUsed, _gamePadLastUsed, _activeSlot[0]);
                for (int i = 0; i < pysicalItem.Count; i++)
                { p1Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p1.Hitbox, pysicalItem[i].Item, _p1Hand, _keyboardLastUsed, _gamePadLastUsed); }
                p1InteractBar.updateme(p1.Hitbox, _playerInteractTime[0], _playerInteractCounter[0]);
                #endregion



                #region Player 2 Hud
                p2Pointer.updateme(gameTime, p2.Hitbox, _p2Hand, blankKB, p2Pad, blankMouse, physicalMap.Hitboxs, p2.ActionState, false, true, _activeSlot[1]);
                for (int i = 0; i < pysicalItem.Count; i++)
                { p2Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p2.Hitbox, pysicalItem[i].Item, _p2Hand, false, true); }
                p2InteractBar.updateme(p2.Hitbox, _playerInteractTime[1], _playerInteractCounter[1]);
                #endregion



                #region Player 3 Hud
                p3Pointer.updateme(gameTime, p3.Hitbox, _p3Hand, blankKB, p3Pad, blankMouse, physicalMap.Hitboxs, p3.ActionState, false, true, _activeSlot[2]);
                for (int i = 0; i < pysicalItem.Count; i++)
                { p3Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p3.Hitbox, pysicalItem[i].Item, _p3Hand, false, true); }
                p3InteractBar.updateme(p3.Hitbox, _playerInteractTime[2], _playerInteractCounter[3]);
                #endregion



                #region Player 4 Hud
                p4Pointer.updateme(gameTime, p4.Hitbox, _p4Hand, blankKB, p4Pad, blankMouse, physicalMap.Hitboxs, p4.ActionState, false, true, _activeSlot[3]);
                for (int i = 0; i < pysicalItem.Count; i++)
                { p4Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p4.Hitbox, pysicalItem[i].Item, _p4Hand, false, true); }
                p4InteractBar.updateme(p4.Hitbox, _playerInteractTime[3], _playerInteractCounter[3]);
                #endregion
            }

            cursorHand.updateme(curr_mouse, _p1Hand[_activeSlot[0]]);
        }





        void updateTutorial(GameTime gameTime)
        {
            if (_isGamePaused == false && tutorialObject.finishedTutorial == true)
                _currentTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;



            // < Camera >
            if (p2Connected == true || p3Connected == true || p4Connected == true)
            {
                Vector2 playerCentrePoint = p1.SourcePoint;
                var min = p1.SourcePoint;
                var max = p1.SourcePoint;
                int playersInPlay = 0;

                // ----- / ----- < Gather Player Points >
                min = Vector2.Min(min, p1.SourcePoint);
                max = Vector2.Max(max, p1.SourcePoint);
                playerCentrePoint = p1.SourcePoint;
                playersInPlay++;

                if (p2Connected == true)
                {
                    min = Vector2.Min(min, p2.SourcePoint);
                    max = Vector2.Max(max, p2.SourcePoint);
                    playerCentrePoint += p2.SourcePoint;
                    playersInPlay++;
                }
                if (p3Connected == true)
                {
                    min = Vector2.Min(min, p3.SourcePoint);
                    max = Vector2.Max(max, p3.SourcePoint);
                    playerCentrePoint += p3.SourcePoint;
                    playersInPlay++;
                }
                if (p4Connected == true)
                {
                    min = Vector2.Min(min, p4.SourcePoint);
                    max = Vector2.Max(max, p4.SourcePoint);
                    playerCentrePoint += p4.SourcePoint;
                    playersInPlay++;
                }

                // ----- / ----- < Generate Camera Zoning > 
                var boundsSize = max - min;
                playerCentrePoint /= playersInPlay;
                var vZoom = (_screenSize - new Vector2(768, 432)) / boundsSize;

                float targetZoom = MathHelper.Clamp(MathF.Min(vZoom.X, vZoom.Y), 0.2f, 1);
                float diferance = cam.Zoom - targetZoom;
                cam.Zoom -= diferance * 0.05f;

                Vector2 camCentre = new Vector2(
                    (-playerCentrePoint.X + _screen.Width / (2 * cam.Zoom)),
                    (-playerCentrePoint.Y + _screen.Height / (2 * cam.Zoom)));

                Vector2 direction = new Vector2(
                    camCentre.X - cam.Position.X,
                    camCentre.Y - cam.Position.Y);

                cam.Velocity += direction * 0.05f;
                cam.Position += cam.Velocity;
                cam.Velocity *= 0.1f;
            }
            else
            {
                // ----- < Single Player Cam >
                cam.Zoom = 1;
                cam.Position.X = (-p1.SourcePoint.X + _screen.Width / (2 * cam.Zoom));
                cam.Position.Y = (-p1.SourcePoint.Y + _screen.Height / (2 * cam.Zoom));
            }





            if (_isGamePaused == false)
            {
                #region Player 1
                #region Player 1 Controls
                // < Item Bar Control >
                // ----- < Next Hand Item Left >
                if ((p1Pad.Buttons.LeftShoulder == ButtonState.Pressed && oldP1Pad.Buttons.LeftShoulder != ButtonState.Pressed) || (curr_mouse.ScrollWheelValue < (old_mouse.ScrollWheelValue - 10)))
                {
                    _activeSlot[0]--;

                    if (_activeSlot[0] < 0)
                        _activeSlot[0] = 2;
                }
                // ----- < Next Hand Item Right >
                else if (p1Pad.Buttons.RightShoulder == ButtonState.Pressed && oldP1Pad.Buttons.RightShoulder != ButtonState.Pressed || (curr_mouse.ScrollWheelValue > (old_mouse.ScrollWheelValue + 10)))
                {
                    _activeSlot[0]++;

                    if (_activeSlot[0] > 2)
                        _activeSlot[0] = 0;
                }



                // < Item Drop Controls >
                if (p1Pad.Buttons.B == ButtonState.Pressed && oldP1Pad.Buttons.B == ButtonState.Released || kb.IsKeyDown(Keys.Q))
                {
                    if (_p1Hand[_activeSlot[0]] != Items.Bare)
                    {
                        ItemDrop(_p1Hand[_activeSlot[0]], p1.SourcePoint, p1.Velocity);
                        _p1Hand[_activeSlot[0]] = Items.Bare;
                    }
                }



                // < Item Pickup >
                for (int i = 0; i < pysicalItem.Count; i++)
                {
                    if (p1.Hitbox.Intersects(pysicalItem[i].Hitbox) && (p1Pad.Buttons.X == ButtonState.Pressed || kb.IsKeyDown(Keys.E)) && _p1Hand[_activeSlot[0]] == Items.Bare)
                    {
                        _p1Hand[_activeSlot[0]] = pysicalItem[i].Item;
                        pysicalItem.RemoveAt(i);
                    }
                }



                // < Tutorial Stage Checks >
                if (p1.Hitbox.Intersects(tutorialObject.stageRect[0]) && tutorialObject.stage == 4)
                {
                    tutorialObject.nextStage();
                }
                if ((_p1Hand[0] == Items.StonePickaxe || _p1Hand[0] == Items.Coal) &&
                    (_p1Hand[1] == Items.StonePickaxe || _p1Hand[1] == Items.Coal) &&
                    (_p1Hand[2] == Items.StonePickaxe || _p1Hand[2] == Items.Coal) &&
                    tutorialObject.stage == 5)
                {
                    tutorialObject.nextStage();
                }
                if (p1.Hitbox.Intersects(tutorialObject.stageRect[1]) && tutorialObject.stage == 6)
                {
                    tutorialObject.nextStage();
                }
                if (!p1.Hitbox.Intersects(tutorialObject.stageRect[1]) && tutorialObject.stage == 7)
                {
                    tutorialObject.nextStage();
                }
                if ((_p1Hand[0] == Items.StoneMultiTool ||
                    _p1Hand[1] == Items.StoneMultiTool ||
                    _p1Hand[2] == Items.StoneMultiTool) &&
                    tutorialObject.stage == 10)
                {
                    tutorialObject.nextStage();
                }
                if (p1.Hitbox.Intersects(tutorialObject.stageRect[2]) && tutorialObject.stage == 11)
                {
                    tutorialObject.nextStage();
                }
                if (p1.Hitbox.Intersects(tutorialObject.stageRect[3]) && tutorialObject.stage == 12)
                {
                    tutorialObject.nextStage();
                }



                // < Deposit Cart Interact/Sell >
                if (p1.Hitbox.Intersects(physicalMap.Hitboxs[10, 0]) && (p1Pad.Buttons.X == ButtonState.Pressed || kb.IsKeyDown(Keys.E)))
                {
                    _newMoney = 0;
                    _newMoneyTransparentsy = 0;

                    for (int i = 0; i < 3; i++)
                    {
                        switch (_p1Hand[i])
                        {
                            case Items.Bare:
                                _p1Hand[i] = Items.Bare;
                                break;

                            case Items.Coal:
                                _money += 3;
                                _newMoney += 3;
                                _newMoneyTransparentsy = 2;
                                _p1Hand[i] = Items.Bare;
                                _depositCart.Play();
                                break;

                            case Items.Copper:
                                _money += 5;
                                _newMoney += 5;
                                _newMoneyTransparentsy = 2;
                                _p1Hand[i] = Items.Bare;
                                _depositCart.Play();
                                break;

                            case Items.Iron:
                                _money += 7;
                                _newMoney += 7; 
                                _newMoneyTransparentsy = 2;
                                _p1Hand[i] = Items.Bare;
                                _depositCart.Play();
                                break;

                            case Items.Gold:
                                _money += 10;
                                _newMoney += 10;
                                _newMoneyTransparentsy = 2;
                                _p1Hand[i] = Items.Bare;
                                _depositCart.Play();
                                break;
                        }
                    }
                }



                // < Ore Seam Interaction Controls >
                for (int i = 0; i < 14; i++)
                {
                    // ----- < Coal Drop >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[6, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StonePickaxe))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            if (_p1Hand[0] == Items.Bare)
                                _p1Hand[0] = Items.Coal;
                            else if (_p1Hand[1] == Items.Bare)
                                _p1Hand[1] = Items.Coal;
                            else if (_p1Hand[2] == Items.Bare)
                                _p1Hand[2] = Items.Coal;
                            else
                                ItemDrop(Items.Coal, new Vector2(physicalMap.Hitboxs[6, i].Center.X, physicalMap.Hitboxs[6, i].Center.Y), Vector2.Zero);

                            _playerInteractCounter[0] = 0;
                            break;
                        }
                    }
                    // < ----- Copper Drop >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[7, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StonePickaxe))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            if (_p1Hand[0] == Items.Bare)
                                _p1Hand[0] = Items.Copper;
                            else if (_p1Hand[1] == Items.Bare)
                                _p1Hand[1] = Items.Copper;
                            else if (_p1Hand[2] == Items.Bare)
                                _p1Hand[2] = Items.Copper;
                            else
                                ItemDrop(Items.Copper, new Vector2(physicalMap.Hitboxs[7, i].Center.X, physicalMap.Hitboxs[7, i].Center.Y), Vector2.Zero);
                            _playerInteractCounter[0] = 0;
                            break;
                        }
                    }
                    // ----- < Iron Drop >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[8, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StonePickaxe))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            if (_p1Hand[0] == Items.Bare)
                                _p1Hand[0] = Items.Iron;
                            else if (_p1Hand[1] == Items.Bare)
                                _p1Hand[1] = Items.Iron;
                            else if (_p1Hand[2] == Items.Bare)
                                _p1Hand[2] = Items.Iron;
                            else
                                ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[8, i].Center.X, physicalMap.Hitboxs[8, i].Center.Y), Vector2.Zero);
                            _playerInteractCounter[0] = 0;
                            break;
                        }
                    }
                    // ----- < Gold Drop >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[9, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StonePickaxe))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            if (_p1Hand[0] == Items.Bare)
                                _p1Hand[0] = Items.Gold;
                            else if (_p1Hand[1] == Items.Bare)
                                _p1Hand[1] = Items.Gold;
                            else if (_p1Hand[2] == Items.Bare)
                                _p1Hand[2] = Items.Gold;
                            else
                                ItemDrop(Items.Gold, new Vector2(physicalMap.Hitboxs[9, i].Center.X, physicalMap.Hitboxs[9, i].Center.Y), Vector2.Zero);
                            _playerInteractCounter[0] = 0;
                            break;
                        }
                    }
                    // ----- < Shovel Cave-In >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[12, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldShovel ||
                        _p1Hand[_activeSlot[0]] == Items.IronShovel ||
                        _p1Hand[_activeSlot[0]] == Items.StoneShovel ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            _playerInteractCounter[0] = 0;
                            physicalMap.caveInClear(physicalMap.Hitboxs[12, i]);
                            break;
                        }
                    }
                }



                // < Buy Wheel >
                if ((p1Pad.Triggers.Left >= 0.9f && (p1Pad.Buttons.X == ButtonState.Pressed && oldP1Pad.Buttons.X == ButtonState.Released)) || (kb.IsKeyDown(Keys.C) && (curr_mouse.LeftButton == ButtonState.Pressed && old_mouse.LeftButton == ButtonState.Released)))
                {
                    if (tutorialObject.stage >= 3 && p1BuyWheel.wheelSlot == 1)
                    {
                        BuyWheel(p1BuyWheel.wheelSlot);
                        tutorialObject.nextStage();
                    }
                    if (tutorialObject.stage >= 9 && p1BuyWheel.wheelSlot == 2)
                    {
                        BuyWheel(p1BuyWheel.wheelSlot);
                        tutorialObject.nextStage();
                    }
                    if (tutorialObject.finishedTutorial == true)
                    {
                        BuyWheel(p1BuyWheel.wheelSlot);
                    }
                }
             


                // < Interact Reset >
                if (p1Pad.Triggers.Right < 0.9f && curr_mouse.LeftButton == ButtonState.Released)
                    _playerInteractCounter[0] = 0;



                _playerInteractTime[0] = HandItemStatCheck(_p1Hand[_activeSlot[0]]);
                #endregion

                p1.updateme(kb, oldKb, curr_mouse, p1Pad, gameTime, physicalMap.Hitboxs, _p1Hand[_activeSlot[0]]);
                #endregion



                if (p2Connected == true)
                {
                    #region Player 2
                    #region Player 2 Controls
                    // < Item Bar Control >
                    // ----- < Next Hand Item Left >
                    if (p2Pad.Buttons.LeftShoulder == ButtonState.Pressed && oldP2Pad.Buttons.LeftShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[1]--;

                        if (_activeSlot[1] < 0)
                            _activeSlot[1] = 2;
                    }
                    // ----- < Next Hand Item Right >
                    else if (p2Pad.Buttons.RightShoulder == ButtonState.Pressed && oldP2Pad.Buttons.RightShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[1]++;

                        if (_activeSlot[1] > 2)
                            _activeSlot[1] = 0;
                    }



                    // < Item Drop Controls >
                    if (p2Pad.Buttons.B == ButtonState.Pressed && oldP2Pad.Buttons.B == ButtonState.Released)
                    {
                        if (_p2Hand[_activeSlot[1]] != Items.Bare)
                        {
                            ItemDrop(_p2Hand[_activeSlot[1]], p2.SourcePoint, p2.Velocity);
                            _p2Hand[_activeSlot[1]] = Items.Bare;
                        }
                    }



                    // < Item Pickup >
                    for (int i = 0; i < pysicalItem.Count; i++)
                    {
                        if (p2.Hitbox.Intersects(pysicalItem[i].Hitbox) && p2Pad.Buttons.X == ButtonState.Pressed && _p2Hand[_activeSlot[1]] == Items.Bare)
                        {
                            _p2Hand[_activeSlot[1]] = pysicalItem[i].Item;
                            pysicalItem.RemoveAt(i);
                        }
                    }



                    // < Tutorial Stage Checks >
                    if (p2.Hitbox.Intersects(tutorialObject.stageRect[0]) && tutorialObject.stage == 4)
                    {
                        tutorialObject.nextStage();
                    }
                    if ((_p2Hand[0] == Items.StonePickaxe || _p2Hand[0] == Items.Coal) &&
                        (_p2Hand[1] == Items.StonePickaxe || _p2Hand[1] == Items.Coal) &&
                        (_p2Hand[2] == Items.StonePickaxe || _p2Hand[2] == Items.Coal) &&
                        tutorialObject.stage == 5)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p2.Hitbox.Intersects(tutorialObject.stageRect[1]) && tutorialObject.stage == 6)
                    {
                        tutorialObject.nextStage();
                    }
                    if (!p2.Hitbox.Intersects(tutorialObject.stageRect[1]) && tutorialObject.stage == 7)
                    {
                        tutorialObject.nextStage();
                    }
                    if ((_p2Hand[0] == Items.StoneMultiTool ||
                        _p2Hand[1] == Items.StoneMultiTool ||
                        _p2Hand[2] == Items.StoneMultiTool) &&
                        tutorialObject.stage == 10)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p2.Hitbox.Intersects(tutorialObject.stageRect[2]) && tutorialObject.stage == 11)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p2.Hitbox.Intersects(tutorialObject.stageRect[3]) && tutorialObject.stage == 12)
                    {
                        tutorialObject.nextStage();
                    }



                    // < Deposit Cart Interact/Sell >
                    if (p2.Hitbox.Intersects(physicalMap.Hitboxs[10, 0]) && p2Pad.Buttons.X == ButtonState.Pressed)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            switch (_p2Hand[i])
                            {
                                case Items.Bare:
                                    _p2Hand[i] = Items.Bare;
                                    break;

                                case Items.Coal:
                                    _money += 3;
                                    _newMoney += 3;
                                    _newMoneyTransparentsy = 2;
                                    _p2Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Copper:
                                    _money += 5;
                                    _newMoney += 5;
                                    _newMoneyTransparentsy = 2;
                                    _p2Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Iron:
                                    _money += 7;
                                    _newMoney += 7;
                                    _newMoneyTransparentsy = 2;
                                    _p2Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Gold:
                                    _money += 10;
                                    _newMoney += 10;
                                    _newMoneyTransparentsy = 2;
                                    _p2Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;
                            }
                        }
                    }



                    // < Ore Seam Interaction Controls >
                    for (int i = 0; i < 14; i++)
                    {
                        // ----- < Coal Drop >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[6, i]) &&
                            p2Pad.Triggers.Right > 0.9f &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                if (_p2Hand[0] == Items.Bare)
                                    _p2Hand[0] = Items.Coal;
                                if (_p2Hand[1] == Items.Bare)
                                    _p2Hand[1] = Items.Coal;
                                if (_p2Hand[2] == Items.Bare)
                                    _p2Hand[2] = Items.Coal;
                                else
                                    ItemDrop(Items.Coal, new Vector2(physicalMap.Hitboxs[6, i].Center.X, physicalMap.Hitboxs[6, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[1] = 0;
                                break;
                            }
                        }
                        // < ----- Copper Drop >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[7, i]) &&
                            (p2Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                if (_p2Hand[0] == Items.Bare)
                                    _p2Hand[0] = Items.Copper;
                                if (_p2Hand[1] == Items.Bare)
                                    _p2Hand[1] = Items.Copper;
                                if (_p2Hand[2] == Items.Bare)
                                    _p2Hand[2] = Items.Copper;
                                else
                                    ItemDrop(Items.Copper, new Vector2(physicalMap.Hitboxs[7, i].Center.X, physicalMap.Hitboxs[7, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[1] = 0;
                                break;
                            }
                        }
                        // ----- < Iron Drop >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[8, i]) &&
                            (p2Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                if (_p2Hand[0] == Items.Bare)
                                    _p2Hand[0] = Items.Iron;
                                if (_p2Hand[1] == Items.Bare)
                                    _p2Hand[1] = Items.Iron;
                                if (_p2Hand[2] == Items.Bare)
                                    _p2Hand[2] = Items.Iron;
                                else
                                    ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[8, i].Center.X, physicalMap.Hitboxs[8, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[1] = 0;
                                break;
                            }
                        }
                        // ----- < Gold Drop >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[9, i]) &&
                            (p2Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                if (_p2Hand[0] == Items.Bare)
                                    _p2Hand[0] = Items.Gold;
                                if (_p2Hand[1] == Items.Bare)
                                    _p2Hand[1] = Items.Gold;
                                if (_p2Hand[2] == Items.Bare)
                                    _p2Hand[2] = Items.Gold;
                                else
                                    ItemDrop(Items.Gold, new Vector2(physicalMap.Hitboxs[9, i].Center.X, physicalMap.Hitboxs[9, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[1] = 0;
                                break;
                            }
                        }
                        // ----- < Shovel Cave-In >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[12, i]) &&
                            (p2Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldShovel ||
                            _p2Hand[_activeSlot[1]] == Items.IronShovel ||
                            _p2Hand[_activeSlot[1]] == Items.StoneShovel ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                _playerInteractCounter[1] = 0;
                                physicalMap.caveInClear(physicalMap.Hitboxs[12, i]);
                                break;
                            }
                        }
                    }



                    // < Buy Wheel >
                    if (p2Pad.Triggers.Left >= 0.9f && (p2Pad.Buttons.X == ButtonState.Pressed && oldP2Pad.Buttons.X == ButtonState.Released))
                    {
                        if (tutorialObject.stage >= 3 && p2BuyWheel.wheelSlot == 1)
                        {
                            BuyWheel(p2BuyWheel.wheelSlot);
                            tutorialObject.nextStage();
                        }
                        if (tutorialObject.stage >= 9 && p2BuyWheel.wheelSlot == 2)
                        {
                            BuyWheel(p2BuyWheel.wheelSlot);
                            tutorialObject.nextStage();
                        }
                        if (tutorialObject.finishedTutorial == true)
                        {
                            BuyWheel(p2BuyWheel.wheelSlot);
                        }
                    }



                    // < Interact Reset >
                    if (p2Pad.Triggers.Right < 0.9f)
                        _playerInteractCounter[1] = 0;



                    _playerInteractTime[1] = HandItemStatCheck(_p2Hand[_activeSlot[1]]);
                    #endregion

                    p2.updateme(blankKB, blankKB, blankMouse, p2Pad, gameTime, physicalMap.Hitboxs, _p2Hand[_activeSlot[1]]);
                    #endregion
                }



                if (p3Connected == true)
                {
                    #region Player 3
                    #region Player 3 Controls
                    // < Item Bar Control >
                    // ----- < Next Hand Item Left >
                    if (p3Pad.Buttons.LeftShoulder == ButtonState.Pressed && oldP3Pad.Buttons.LeftShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[2]--;

                        if (_activeSlot[2] < 0)
                            _activeSlot[2] = 2;
                    }
                    // ----- < Next Hand Item Right >
                    else if (p3Pad.Buttons.RightShoulder == ButtonState.Pressed && oldP3Pad.Buttons.RightShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[2]++;

                        if (_activeSlot[2] > 2)
                            _activeSlot[2] = 0;
                    }



                    // < Item Drop Controls >
                    if (p3Pad.Buttons.B == ButtonState.Pressed && oldP3Pad.Buttons.B == ButtonState.Released)
                    {
                        if (_p3Hand[_activeSlot[2]] != Items.Bare)
                        {
                            ItemDrop(_p3Hand[_activeSlot[2]], p3.SourcePoint, p3.Velocity);
                            _p3Hand[_activeSlot[2]] = Items.Bare;
                        }
                    }



                    // < Item Pickup >
                    for (int i = 0; i < pysicalItem.Count; i++)
                    {
                        if (p3.Hitbox.Intersects(pysicalItem[i].Hitbox) && p3Pad.Buttons.X == ButtonState.Pressed && _p3Hand[_activeSlot[2]] == Items.Bare)
                        {
                            _p3Hand[_activeSlot[2]] = pysicalItem[i].Item;
                            pysicalItem.RemoveAt(i);
                        }
                    }



                    // < Tutorial Stage Checks >
                    if (p3.Hitbox.Intersects(tutorialObject.stageRect[0]) && tutorialObject.stage == 4)
                    {
                        tutorialObject.nextStage();
                    }
                    if ((_p3Hand[0] == Items.StonePickaxe || _p3Hand[0] == Items.Coal) &&
                        (_p3Hand[1] == Items.StonePickaxe || _p3Hand[1] == Items.Coal) &&
                        (_p3Hand[2] == Items.StonePickaxe || _p3Hand[2] == Items.Coal) &&
                        tutorialObject.stage == 5)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p3.Hitbox.Intersects(tutorialObject.stageRect[1]) && tutorialObject.stage == 6)
                    {
                        tutorialObject.nextStage();
                    }
                    if (!p3.Hitbox.Intersects(tutorialObject.stageRect[1]) && tutorialObject.stage == 7)
                    {
                        tutorialObject.nextStage();
                    }
                    if ((_p3Hand[0] == Items.StoneMultiTool ||
                        _p3Hand[1] == Items.StoneMultiTool ||
                        _p3Hand[2] == Items.StoneMultiTool) &&
                        tutorialObject.stage == 10)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p3.Hitbox.Intersects(tutorialObject.stageRect[2]) && tutorialObject.stage == 11)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p3.Hitbox.Intersects(tutorialObject.stageRect[3]) && tutorialObject.stage == 12)
                    {
                        tutorialObject.nextStage();
                    }



                    // < Deposit Cart Interact/Sell >
                    if (p3.Hitbox.Intersects(physicalMap.Hitboxs[10, 0]) && p3Pad.Buttons.X == ButtonState.Pressed)
                    {
                        _newMoney = 0;
                        _newMoneyTransparentsy = 0;

                        for (int i = 0; i < 3; i++)
                        {
                            switch (_p3Hand[i])
                            {
                                case Items.Bare:
                                    _p3Hand[i] = Items.Bare;
                                    break;

                                case Items.Coal:
                                    _money += 3;
                                    _newMoney += 3;
                                    _newMoneyTransparentsy = 2;
                                    _p3Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Copper:
                                    _money += 5;
                                    _newMoney += 5;
                                    _newMoneyTransparentsy = 2;
                                    _p3Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Iron:
                                    _money += 7;
                                    _newMoney += 7;
                                    _newMoneyTransparentsy = 2;
                                    _p3Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Gold:
                                    _money += 10;
                                    _newMoney += 10;
                                    _newMoneyTransparentsy = 2;
                                    _p3Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;
                            }
                        }
                    }



                    // < Ore Seam Interaction Controls >
                    for (int i = 0; i < 14; i++)
                    {
                        // ----- < Coal Drop >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[6, i]) &&
                            p3Pad.Triggers.Right > 0.9f &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p3Hand[0] == Items.Bare)
                                    _p3Hand[0] = Items.Coal;
                                if (_p3Hand[1] == Items.Bare)
                                    _p3Hand[1] = Items.Coal;
                                if (_p3Hand[2] == Items.Bare)
                                    _p3Hand[2] = Items.Coal;
                                else
                                    ItemDrop(Items.Coal, new Vector2(physicalMap.Hitboxs[6, i].Center.X, physicalMap.Hitboxs[6, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[2] = 0;
                                break;
                            }
                        }
                        // < ----- Copper Drop >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[7, i]) &&
                            (p3Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p3Hand[0] == Items.Bare)
                                    _p3Hand[0] = Items.Copper;
                                if (_p3Hand[1] == Items.Bare)
                                    _p3Hand[1] = Items.Copper;
                                if (_p3Hand[2] == Items.Bare)
                                    _p3Hand[2] = Items.Copper;
                                else
                                    ItemDrop(Items.Copper, new Vector2(physicalMap.Hitboxs[7, i].Center.X, physicalMap.Hitboxs[7, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[2] = 0;
                                break;
                            }
                        }
                        // ----- < Iron Drop >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[8, i]) &&
                            (p3Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p3Hand[0] == Items.Bare)
                                    _p3Hand[0] = Items.Iron;
                                if (_p3Hand[1] == Items.Bare)
                                    _p3Hand[1] = Items.Iron;
                                if (_p3Hand[2] == Items.Bare)
                                    _p3Hand[2] = Items.Iron;
                                else
                                    ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[8, i].Center.X, physicalMap.Hitboxs[8, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[2] = 0;
                                break;
                            }
                        }
                        // ----- < Gold Drop >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[9, i]) &&
                            (p3Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p3Hand[0] == Items.Bare)
                                    _p3Hand[0] = Items.Gold;
                                if (_p3Hand[1] == Items.Bare)
                                    _p3Hand[1] = Items.Gold;
                                if (_p3Hand[2] == Items.Bare)
                                    _p3Hand[2] = Items.Gold;
                                else
                                    ItemDrop(Items.Gold, new Vector2(physicalMap.Hitboxs[9, i].Center.X, physicalMap.Hitboxs[9, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[2] = 0;
                                break;
                            }
                        }
                        // ----- < Shovel Cave-In >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[12, i]) &&
                            (p3Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldShovel ||
                            _p3Hand[_activeSlot[2]] == Items.IronShovel ||
                            _p3Hand[_activeSlot[2]] == Items.StoneShovel ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                _playerInteractCounter[2] = 0;
                                physicalMap.caveInClear(physicalMap.Hitboxs[12, i]);
                                break;
                            }
                        }
                    }



                    // < Buy Wheel >
                    if (p3Pad.Triggers.Left >= 0.9f && (p3Pad.Buttons.X == ButtonState.Pressed && oldP3Pad.Buttons.X == ButtonState.Released))
                    {
                        if (tutorialObject.stage >= 3 && p3BuyWheel.wheelSlot == 1)
                        {
                            BuyWheel(p3BuyWheel.wheelSlot);
                            tutorialObject.nextStage();
                        }
                        if (tutorialObject.stage >= 9 && p3BuyWheel.wheelSlot == 2)
                        {
                            BuyWheel(p3BuyWheel.wheelSlot);
                            tutorialObject.nextStage();
                        }
                        if (tutorialObject.finishedTutorial == true)
                        {
                            BuyWheel(p3BuyWheel.wheelSlot);
                        }
                    }



                    // < Interact Reset >
                    if (p3Pad.Triggers.Right < 0.9f)
                        _playerInteractCounter[2] = 0;



                    _playerInteractTime[2] = HandItemStatCheck(_p3Hand[_activeSlot[2]]);
                    #endregion

                    p3.updateme(blankKB, blankKB, blankMouse, p3Pad, gameTime, physicalMap.Hitboxs, _p3Hand[_activeSlot[2]]);
                    #endregion
                }



                if (p4Connected == true)
                {
                    #region Player 4
                    #region Player 4 Controls
                    // < Item Bar Control >
                    // ----- < Next Hand Item Left >
                    if (p4Pad.Buttons.LeftShoulder == ButtonState.Pressed && oldP4Pad.Buttons.LeftShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[3]--;

                        if (_activeSlot[3] < 0)
                            _activeSlot[3] = 2;
                    }
                    // ----- < Next Hand Item Right >
                    else if (p4Pad.Buttons.RightShoulder == ButtonState.Pressed && oldP4Pad.Buttons.RightShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[3]++;

                        if (_activeSlot[3] > 2)
                            _activeSlot[3] = 0;
                    }



                    // < Item Drop Controls >
                    if (p4Pad.Buttons.B == ButtonState.Pressed && oldP4Pad.Buttons.B == ButtonState.Released)
                    {
                        if (_p4Hand[_activeSlot[3]] != Items.Bare)
                        {
                            ItemDrop(_p4Hand[_activeSlot[3]], p4.SourcePoint, p4.Velocity);
                            _p4Hand[_activeSlot[3]] = Items.Bare;
                        }
                    }



                    // < Item Pickup >
                    for (int i = 0; i < pysicalItem.Count; i++)
                    {
                        if (p4.Hitbox.Intersects(pysicalItem[i].Hitbox) && p4Pad.Buttons.X == ButtonState.Pressed && _p4Hand[_activeSlot[3]] == Items.Bare)
                        {
                            _p4Hand[_activeSlot[3]] = pysicalItem[i].Item;
                            pysicalItem.RemoveAt(i);
                        }
                    }



                    // < Tutorial Stage Checks >
                    if (p4.Hitbox.Intersects(tutorialObject.stageRect[0]) && tutorialObject.stage == 4)
                    {
                        tutorialObject.nextStage();
                    }
                    if ((_p4Hand[0] == Items.StonePickaxe || _p4Hand[0] == Items.Coal) &&
                        (_p4Hand[1] == Items.StonePickaxe || _p4Hand[1] == Items.Coal) &&
                        (_p4Hand[2] == Items.StonePickaxe || _p4Hand[2] == Items.Coal) &&
                        tutorialObject.stage == 5)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p4.Hitbox.Intersects(tutorialObject.stageRect[1]) && tutorialObject.stage == 6)
                    {
                        tutorialObject.nextStage();
                    }
                    if (!p4.Hitbox.Intersects(tutorialObject.stageRect[1]) && tutorialObject.stage == 7)
                    {
                        tutorialObject.nextStage();
                    }
                    if ((_p4Hand[0] == Items.StoneMultiTool ||
                        _p4Hand[1] == Items.StoneMultiTool ||
                        _p4Hand[2] == Items.StoneMultiTool) &&
                        tutorialObject.stage == 10)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p4.Hitbox.Intersects(tutorialObject.stageRect[2]) && tutorialObject.stage == 11)
                    {
                        tutorialObject.nextStage();
                    }
                    if (p4.Hitbox.Intersects(tutorialObject.stageRect[3]) && tutorialObject.stage == 12)
                    {
                        tutorialObject.nextStage();
                    }



                    // < Deposit Cart Interact/Sell >
                    if (p4.Hitbox.Intersects(physicalMap.Hitboxs[10, 0]) && p4Pad.Buttons.X == ButtonState.Pressed)
                    {
                        _newMoney = 0;
                        _newMoneyTransparentsy = 0;

                        for (int i = 0; i < 3; i++)
                        {
                            switch (_p4Hand[i])
                            {
                                case Items.Bare:
                                    _p4Hand[i] = Items.Bare;
                                    break;

                                case Items.Coal:
                                    _money += 3;
                                    _newMoney += 3;
                                    _newMoneyTransparentsy = 2;
                                    _p4Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Copper:
                                    _money += 5;
                                    _newMoney += 5;
                                    _newMoneyTransparentsy = 2;
                                    _p4Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Iron:
                                    _money += 7;
                                    _newMoney += 7;
                                    _newMoneyTransparentsy = 2;
                                    _p4Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Gold:
                                    _money += 10;
                                    _newMoney += 10;
                                    _newMoneyTransparentsy = 2;
                                    _p4Hand[i] = Items.Bare;
                                    _depositCartInst.Play();
                                    break;
                            }
                        }
                    }



                    // < Ore Seam Interaction Controls >
                    for (int i = 0; i < 14; i++)
                    {
                        // ----- < Coal Drop >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[6, i]) &&
                            p4Pad.Triggers.Right > 0.9f &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p4Hand[0] == Items.Bare)
                                    _p4Hand[0] = Items.Coal;
                                if (_p4Hand[1] == Items.Bare)
                                    _p4Hand[1] = Items.Coal;
                                if (_p4Hand[2] == Items.Bare)
                                    _p4Hand[2] = Items.Coal;
                                else
                                    ItemDrop(Items.Coal, new Vector2(physicalMap.Hitboxs[6, i].Center.X, physicalMap.Hitboxs[6, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[3] = 0;
                                break;
                            }
                        }
                        // < ----- Copper Drop >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[7, i]) &&
                            (p4Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p4Hand[0] == Items.Bare)
                                    _p4Hand[0] = Items.Copper;
                                if (_p4Hand[1] == Items.Bare)
                                    _p4Hand[1] = Items.Copper;
                                if (_p4Hand[2] == Items.Bare)
                                    _p4Hand[2] = Items.Copper;
                                else
                                    ItemDrop(Items.Copper, new Vector2(physicalMap.Hitboxs[7, i].Center.X, physicalMap.Hitboxs[7, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[3] = 0;
                                break;
                            }
                        }
                        // ----- < Iron Drop >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[8, i]) &&
                            (p4Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p4Hand[0] == Items.Bare)
                                    _p4Hand[0] = Items.Iron;
                                if (_p4Hand[1] == Items.Bare)
                                    _p4Hand[1] = Items.Iron;
                                if (_p4Hand[2] == Items.Bare)
                                    _p4Hand[2] = Items.Iron;
                                else
                                    ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[8, i].Center.X, physicalMap.Hitboxs[8, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[3] = 0;
                                break;
                            }
                        }
                        // ----- < Gold Drop >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[9, i]) &&
                            (p4Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p4Hand[0] == Items.Bare)
                                    _p4Hand[0] = Items.Gold;
                                if (_p4Hand[1] == Items.Bare)
                                    _p4Hand[1] = Items.Gold;
                                if (_p4Hand[2] == Items.Bare)
                                    _p4Hand[2] = Items.Gold;
                                else
                                    ItemDrop(Items.Gold, new Vector2(physicalMap.Hitboxs[9, i].Center.X, physicalMap.Hitboxs[9, i].Center.Y), Vector2.Zero);
                                _playerInteractCounter[3] = 0;
                                break;
                            }
                        }
                        // ----- < Shovel Cave-In >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[12, i]) &&
                            (p4Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldShovel ||
                            _p4Hand[_activeSlot[3]] == Items.IronShovel ||
                            _p4Hand[_activeSlot[3]] == Items.StoneShovel ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                _playerInteractCounter[3] = 0;
                                physicalMap.caveInClear(physicalMap.Hitboxs[12, i]);
                                break;
                            }
                        }
                    }



                    // < Buy Wheel >
                    if (p4Pad.Triggers.Left >= 0.9f && (p4Pad.Buttons.X == ButtonState.Pressed && oldP4Pad.Buttons.X == ButtonState.Released))
                    {
                        if (tutorialObject.stage >= 3 && p4BuyWheel.wheelSlot == 1)
                        {
                            BuyWheel(p4BuyWheel.wheelSlot);
                            tutorialObject.nextStage();
                        }
                        if (tutorialObject.stage >= 9 && p4BuyWheel.wheelSlot == 2)
                        {
                            BuyWheel(p4BuyWheel.wheelSlot);
                            tutorialObject.nextStage();
                        }
                        if (tutorialObject.finishedTutorial == true)
                        {
                            BuyWheel(p4BuyWheel.wheelSlot);
                        }
                    }



                    // < Interact Reset >
                    if (p4Pad.Triggers.Right < 0.9f)
                        _playerInteractCounter[3] = 0;

                    _playerInteractTime[3] = HandItemStatCheck(_p4Hand[_activeSlot[3]]);
                    #endregion

                    p4.updateme(blankKB, blankKB, blankMouse, p4Pad, gameTime, physicalMap.Hitboxs, _p4Hand[_activeSlot[3]]);
                    #endregion
                }



                for (int i = 0; i < pysicalItem.Count; i++) { pysicalItem[i].updateme(physicalMap.Hitboxs); }

                physicalMap.updatemeTUTORIAL(gameTime, _currentTime, tutorialObject.stage, tutorialObject.finishedTutorial);

                if (tutorialObject.finishedTutorial == false) 
                    tutorialObject.updateme(gameTime, _money, _keyboardLastUsed);

                if (tutorialObject.stage == 0 && physicalMap.forceCaveInRoggleLock == false) 
                    physicalMap.TutorialForceCavein(2);

                if (tutorialObject.stage == 1 && physicalMap.forceCaveInRoggleLock == true)
                    physicalMap.unlockToggleLock(); 

                if (tutorialObject.stage == 6 && physicalMap.forceCaveInRoggleLock == false)
                { 
                    physicalMap.TutorialForceCavein(1);
                    _caveInInst.Play();
                }

                if (tutorialObject.stage == 7 && physicalMap.forceCaveInRoggleLock == true)
                    physicalMap.unlockToggleLock();



                #region Player 1 Hud
                p1Pointer.updateme(gameTime, p1.Hitbox, _p1Hand, kb, p1Pad, curr_mouse, physicalMap.Hitboxs, p1.ActionState, _keyboardLastUsed, _gamePadLastUsed, _activeSlot[0]);
                for (int i = 0; i < pysicalItem.Count; i++)
                { p1Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p1.Hitbox, pysicalItem[i].Item, _p1Hand, _keyboardLastUsed, _gamePadLastUsed); }
                p1InteractBar.updateme(p1.Hitbox, _playerInteractTime[0], _playerInteractCounter[0]);
                p1BuyWheel.updatemeP1(p1.Hitbox, p1Pad, curr_mouse, _keyboardLastUsed, _gamePadLastUsed);
                #endregion



                if (p2Connected == true)
                {
                    #region Player 2 Hud
                    p2Pointer.updateme(gameTime, p2.Hitbox, _p2Hand, blankKB, p2Pad, blankMouse, physicalMap.Hitboxs, p2.ActionState, false, true, _activeSlot[1]);
                    for (int i = 0; i < pysicalItem.Count; i++)
                    { p2Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p2.Hitbox, pysicalItem[i].Item, _p2Hand, false, true); }
                    p2InteractBar.updateme(p2.Hitbox, _playerInteractTime[1], _playerInteractCounter[1]);
                    p2BuyWheel.updatemeGENERIC(p2.Hitbox, p2Pad);
                    #endregion
                }



                if (p3Connected == true)
                {
                    #region Player 3 Hud
                    p3Pointer.updateme(gameTime, p3.Hitbox, _p3Hand, blankKB, p3Pad, blankMouse, physicalMap.Hitboxs, p3.ActionState, false, true, _activeSlot[2]);
                    for (int i = 0; i < pysicalItem.Count; i++)
                    { p3Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p3.Hitbox, pysicalItem[i].Item, _p3Hand, false, true); }
                    p3InteractBar.updateme(p3.Hitbox, _playerInteractTime[2], _playerInteractCounter[2]);
                    p3BuyWheel.updatemeGENERIC(p3.Hitbox, p3Pad);
                    #endregion
                }



                if (p4Connected == true)
                {
                    #region Player 4 Hud
                    p4Pointer.updateme(gameTime, p4.Hitbox, _p4Hand, blankKB, p4Pad, blankMouse, physicalMap.Hitboxs, p4.ActionState, false, true, _activeSlot[3]);
                    for (int i = 0; i < pysicalItem.Count; i++)
                    { p4Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p4.Hitbox, pysicalItem[i].Item, _p4Hand, false, true); }
                    p4InteractBar.updateme(p4.Hitbox, _playerInteractTime[3], _playerInteractCounter[3]);
                    p4BuyWheel.updatemeGENERIC(p4.Hitbox, p4Pad);
                    #endregion
                }
            }


            // < End Level Star Check >
            if (_currentTime <= -2 && _transitioning == false)
            {
                Transition(gameState.LevelSelect, 0);

                _playedBefore = true;

                // < 1 Star Check >
                if (_money >= (_tutorialGoal1 * _numberOfPlayers))
                {
                    _levelsComplete[0] = true;
                    _levelStars[0] = 1;

                    // < 2 Star Check >
                    if (_money >= _tutorialGoal2 * _numberOfPlayers)
                        _levelStars[0] = 2;

                    // < 3 Star Check >
                    if (_money >= _tutorialGoal3 * _numberOfPlayers)
                        _levelStars[0] = 3;
                }
            }

            cursorHand.updateme(curr_mouse, _p1Hand[_activeSlot[0]]);
        }





        void updateLevel(GameTime gameTime)
        {
            if (_isGamePaused == false)
                _currentTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;



            // < Camera >
            if (p2Connected == true || p3Connected == true || p4Connected == true)
            {
                Vector2 playerCentrePoint = p1.SourcePoint;
                var min = p1.SourcePoint;
                var max = p1.SourcePoint;
                int playersInPlay = 0;

                // ----- / ----- < Gather Player Points >
                min = Vector2.Min(min, p1.SourcePoint);
                max = Vector2.Max(max, p1.SourcePoint);
                playerCentrePoint = p1.SourcePoint;
                playersInPlay++;

                if (p2Connected == true)
                {
                    min = Vector2.Min(min, p2.SourcePoint);
                    max = Vector2.Max(max, p2.SourcePoint);
                    playerCentrePoint += p2.SourcePoint;
                    playersInPlay++;
                }
                if (p3Connected == true)
                {
                    min = Vector2.Min(min, p3.SourcePoint);
                    max = Vector2.Max(max, p3.SourcePoint);
                    playerCentrePoint += p3.SourcePoint;
                    playersInPlay++;
                }
                if (p4Connected == true)
                {
                    min = Vector2.Min(min, p4.SourcePoint);
                    max = Vector2.Max(max, p4.SourcePoint);
                    playerCentrePoint += p4.SourcePoint;
                    playersInPlay++;
                }

                // ----- / ----- < Generate Camera Zoning > 
                var boundsSize = max - min;
                playerCentrePoint /= playersInPlay;
                var vZoom = (_screenSize - new Vector2(768, 432)) / boundsSize;

                float targetZoom = MathHelper.Clamp(MathF.Min(vZoom.X, vZoom.Y), 0.2f, 1);
                float diferance = cam.Zoom - targetZoom;
                cam.Zoom -= diferance * 0.05f;

                Vector2 camCentre = new Vector2(
                    (-playerCentrePoint.X + _screen.Width / (2 * cam.Zoom)),
                    (-playerCentrePoint.Y + _screen.Height / (2 * cam.Zoom)));

                Vector2 direction = new Vector2(
                    camCentre.X - cam.Position.X,
                    camCentre.Y - cam.Position.Y);

                cam.Velocity += direction * 0.05f;
                cam.Position += cam.Velocity;
                cam.Velocity *= 0.1f;
            }
            else
            {
                // ----- < Single Player Cam >
                cam.Zoom = 1;
                cam.Position.X = (-p1.SourcePoint.X + _screen.Width / (2 * cam.Zoom));
                cam.Position.Y = (-p1.SourcePoint.Y + _screen.Height / (2 * cam.Zoom));
            }





            physicalMap.updatemeLEVEL(gameTime, _currentTime);

            if (_isGamePaused == false)
            {
                #region Player 1
                #region Player 1 Controls
                // < Item Bar Control >
                // ----- < Next Hand Item Left >
                if ((p1Pad.Buttons.LeftShoulder == ButtonState.Pressed && oldP1Pad.Buttons.LeftShoulder != ButtonState.Pressed) || (curr_mouse.ScrollWheelValue < (old_mouse.ScrollWheelValue - 10)))
                {
                    _activeSlot[0]--;

                    if (_activeSlot[0] < 0)
                        _activeSlot[0] = 2;
                }
                // ----- < Next Hand Item Right >
                else if (p1Pad.Buttons.RightShoulder == ButtonState.Pressed && oldP1Pad.Buttons.RightShoulder != ButtonState.Pressed || (curr_mouse.ScrollWheelValue > (old_mouse.ScrollWheelValue + 10)))
                {
                    _activeSlot[0]++;

                    if (_activeSlot[0] > 2)
                        _activeSlot[0] = 0;
                }



                // < Item Drop Controls >
                if (p1Pad.Buttons.B == ButtonState.Pressed && oldP1Pad.Buttons.B == ButtonState.Released || kb.IsKeyDown(Keys.Q))
                {
                    if (_p1Hand[_activeSlot[0]] != Items.Bare)
                    {
                        ItemDrop(_p1Hand[_activeSlot[0]], p1.SourcePoint, p1.Velocity);
                        _p1Hand[_activeSlot[0]] = Items.Bare;
                    }
                }



                // < Item Pickup >
                for (int i = 0; i < pysicalItem.Count; i++)
                {
                    if (p1.Hitbox.Intersects(pysicalItem[i].Hitbox) && (p1Pad.Buttons.X == ButtonState.Pressed || kb.IsKeyDown(Keys.E)) && _p1Hand[_activeSlot[0]] == Items.Bare)
                    {
                        _p1Hand[_activeSlot[0]] = pysicalItem[i].Item;
                        pysicalItem.RemoveAt(i);
                    }
                }



                // < Deposit Cart Interact/Sell >
                if (p1.Hitbox.Intersects(physicalMap.Hitboxs[10, 0]) && (p1Pad.Buttons.X == ButtonState.Pressed || kb.IsKeyDown(Keys.E)))
                {
                    _newMoney = 0;
                    _newMoneyTransparentsy = 0;

                    for (int i = 0; i < 3; i++)
                    {
                        switch (_p1Hand[i])
                        {
                            case Items.Bare:
                                _p1Hand[i] = Items.Bare;
                                break;

                            case Items.Coal:
                                _money += 3; 
                                _newMoney += 3;
                                _newMoneyTransparentsy = 2;
                                _p1Hand[i] = Items.Bare;
                                if (_level == 2)
                                    _level2OreCollected[0]++;
                                if (_level == 3)
                                    _level3OreCollected++;
                                if (_level == 4)
                                    _level4OreCollected[0]++;
                                if (_level == 5)
                                    _level5OreCollected[0]++;
                                _depositCartInst.Play();
                                break;

                            case Items.Copper:
                                _money += 5;
                                _newMoney += 5;
                                _newMoneyTransparentsy = 2;
                                _p1Hand[i] = Items.Bare;
                                if (_level == 2)
                                    _level2OreCollected[1]++;
                                if (_level == 3)
                                    _level3OreCollected++;
                                if (_level == 4)
                                    _level4OreCollected[1]++;
                                if (_level == 5)
                                    _level5OreCollected[1]++;
                                _depositCartInst.Play();
                                break;

                            case Items.Iron:
                                _money += 7;
                                _newMoney += 7;
                                _newMoneyTransparentsy = 2;
                                _p1Hand[i] = Items.Bare;
                                if (_level == 3)
                                    _level3OreCollected++;
                                if (_level == 4)
                                    _level4OreCollected[2]++;
                                if (_level == 5)
                                    _level5OreCollected[2]++;
                                _depositCartInst.Play();
                                break;

                            case Items.Gold:
                                _money += 10;
                                _newMoney += 10;
                                _newMoneyTransparentsy = 10;
                                _p1Hand[i] = Items.Bare;
                                if (_level == 3)
                                    _level3OreCollected++;
                                if (_level == 5)
                                    _level5OreCollected[3]++;
                                _depositCartInst.Play();
                                break;
                        }
                    }
                }



                // < Ore Seam Interaction Controls >
                for (int i = 0; i < 14; i++)
                {
                    // ----- < Coal Drop >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[6, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StonePickaxe))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            if (_p1Hand[0] == Items.Bare)
                                _p1Hand[0] = Items.Coal;
                            else if (_p1Hand[1] == Items.Bare)
                                _p1Hand[1] = Items.Coal;
                            else if (_p1Hand[2] == Items.Bare)
                                _p1Hand[2] = Items.Coal;
                            else
                                ItemDrop(Items.Coal, new Vector2(physicalMap.Hitboxs[6, i].Center.X, physicalMap.Hitboxs[6, i].Center.Y), Vector2.Zero);

                            _playerInteractCounter[0] = 0;
                            break;
                        }
                    }
                    // < ----- Copper Drop >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[7, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StonePickaxe))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            if (_p1Hand[0] == Items.Bare)
                                _p1Hand[0] = Items.Copper;
                            else if (_p1Hand[1] == Items.Bare)
                                _p1Hand[1] = Items.Copper;
                            else if (_p1Hand[2] == Items.Bare)
                                _p1Hand[2] = Items.Copper;
                            else
                                ItemDrop(Items.Copper, new Vector2(physicalMap.Hitboxs[7, i].Center.X, physicalMap.Hitboxs[7, i].Center.Y), Vector2.Zero);

                            _playerInteractCounter[0] = 0;
                            break;
                        }
                    }
                    // ----- < Iron Drop >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[8, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StonePickaxe))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            if (_p1Hand[0] == Items.Bare)
                                _p1Hand[0] = Items.Iron;
                            else if (_p1Hand[1] == Items.Bare)
                                _p1Hand[1] = Items.Iron;
                            else if (_p1Hand[2] == Items.Bare)
                                _p1Hand[2] = Items.Iron;
                            else
                                ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[8, i].Center.X, physicalMap.Hitboxs[8, i].Center.Y), Vector2.Zero);

                            _playerInteractCounter[0] = 0;
                            break;
                        }
                    }
                    // ----- < Gold Drop >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[9, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronPickaxe ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StonePickaxe))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            if (_p1Hand[0] == Items.Bare)
                                _p1Hand[0] = Items.Gold;
                            else if (_p1Hand[1] == Items.Bare)
                                _p1Hand[1] = Items.Gold;
                            else if (_p1Hand[2] == Items.Bare)
                                _p1Hand[2] = Items.Gold;
                            else
                                ItemDrop(Items.Gold, new Vector2(physicalMap.Hitboxs[9, i].Center.X, physicalMap.Hitboxs[9, i].Center.Y), Vector2.Zero);

                            _playerInteractCounter[0] = 0;
                            break;
                        }
                    }
                    // ----- < Shovel Cave-In >
                    if (p1.Hitbox.Intersects(physicalMap.Hitboxs[12, i]) &&
                        (p1Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                        (_p1Hand[_activeSlot[0]] == Items.GoldShovel ||
                        _p1Hand[_activeSlot[0]] == Items.IronShovel ||
                        _p1Hand[_activeSlot[0]] == Items.StoneShovel ||
                        _p1Hand[_activeSlot[0]] == Items.IronMultiTool ||
                        _p1Hand[_activeSlot[0]] == Items.StoneMultiTool))
                    {
                        _playerInteractCounter[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_playerInteractCounter[0] > _playerInteractTime[0])
                        {
                            _playerInteractCounter[0] = 0;
                            physicalMap.caveInClear(physicalMap.Hitboxs[12, i]);
                            break;
                        }
                    }
                }



                // < Buy Wheel >
                if ((p1Pad.Triggers.Left >= 0.9f && (p1Pad.Buttons.X == ButtonState.Pressed && oldP1Pad.Buttons.X == ButtonState.Released)) || (kb.IsKeyDown(Keys.C) && (curr_mouse.LeftButton == ButtonState.Pressed && old_mouse.LeftButton == ButtonState.Released)))
                {                    
                    BuyWheel(p1BuyWheel.wheelSlot);             
                }



                // < Interact Reset >
                if (p1Pad.Triggers.Right < 0.9f && curr_mouse.LeftButton == ButtonState.Released)
                    _playerInteractCounter[0] = 0;



                _playerInteractTime[0] = HandItemStatCheck(_p1Hand[_activeSlot[0]]);
                #endregion

                p1.updateme(kb, oldKb, curr_mouse, p1Pad, gameTime, physicalMap.Hitboxs, _p1Hand[_activeSlot[0]]);
                #endregion



                if (p2Connected == true)
                {
                    #region Player 2
                    #region Player 2 Controls
                    // < Item Bar Control >
                    // ----- < Next Hand Item Left >
                    if (p2Pad.Buttons.LeftShoulder == ButtonState.Pressed && oldP2Pad.Buttons.LeftShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[1]--;

                        if (_activeSlot[1] < 0)
                            _activeSlot[1] = 2;
                    }
                    // ----- < Next Hand Item Right >
                    else if (p2Pad.Buttons.RightShoulder == ButtonState.Pressed && oldP2Pad.Buttons.RightShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[1]++;

                        if (_activeSlot[1] > 2)
                            _activeSlot[1] = 0;
                    }



                    // < Item Drop Controls >
                    if (p2Pad.Buttons.B == ButtonState.Pressed && oldP2Pad.Buttons.B == ButtonState.Released)
                    {
                        if (_p2Hand[_activeSlot[1]] != Items.Bare)
                        {
                            ItemDrop(_p2Hand[_activeSlot[1]], p2.SourcePoint, p2.Velocity);
                            _p2Hand[_activeSlot[1]] = Items.Bare;
                        }
                    }



                    // < Item Pickup >
                    for (int i = 0; i < pysicalItem.Count; i++)
                    {
                        if (p2.Hitbox.Intersects(pysicalItem[i].Hitbox) && p2Pad.Buttons.X == ButtonState.Pressed && _p2Hand[_activeSlot[1]] == Items.Bare)
                        {
                            _p2Hand[_activeSlot[1]] = pysicalItem[i].Item;
                            pysicalItem.RemoveAt(i);
                        }
                    }



                    // < Deposit Cart Interact/Sell >
                    if (p2.Hitbox.Intersects(physicalMap.Hitboxs[10, 0]) && p2Pad.Buttons.X == ButtonState.Pressed)
                    {
                        _newMoney = 0;
                        _newMoneyTransparentsy = 0;

                        for (int i = 0; i < 3; i++)
                        {
                            switch (_p2Hand[i])
                            {
                                case Items.Bare:
                                    _p2Hand[i] = Items.Bare;
                                    break;

                                case Items.Coal:
                                    _money += 3;
                                    _newMoney += 3;
                                    _newMoneyTransparentsy = 2;
                                    _p2Hand[i] = Items.Bare;
                                    if (_level == 2)
                                        _level2OreCollected[0]++;
                                    if (_level == 3)
                                        _level3OreCollected++; 
                                    if (_level == 4)
                                        _level4OreCollected[0]++;
                                    if (_level == 5)
                                        _level5OreCollected[0]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Copper:
                                    _money += 5;
                                    _newMoney += 5;
                                    _newMoneyTransparentsy = 2;
                                    _p2Hand[i] = Items.Bare;
                                    if (_level == 2)
                                        _level2OreCollected[1]++;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 4)
                                        _level4OreCollected[1]++;
                                    if (_level == 5)
                                        _level5OreCollected[1]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Iron:
                                    _money += 7;
                                    _newMoney += 7;
                                    _newMoneyTransparentsy = 2;
                                    _p2Hand[i] = Items.Bare;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 4)
                                        _level4OreCollected[2]++;
                                    if (_level == 5)
                                        _level5OreCollected[2]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Gold:
                                    _money += 10;
                                    _newMoney += 10;
                                    _newMoneyTransparentsy = 2;
                                    _p2Hand[i] = Items.Bare;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 5)
                                        _level5OreCollected[3]++;
                                    _depositCartInst.Play();
                                    break;
                            }
                        }
                    }



                    // < Ore Seam Interaction Controls >
                    for (int i = 0; i < 14; i++)
                    {
                        // ----- < Coal Drop >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[6, i]) &&
                            p2Pad.Triggers.Right > 0.9f &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                if (_p2Hand[0] == Items.Bare)
                                    _p2Hand[0] = Items.Coal;
                                if (_p2Hand[1] == Items.Bare)
                                    _p2Hand[1] = Items.Coal;
                                if (_p2Hand[2] == Items.Bare)
                                    _p2Hand[2] = Items.Coal;
                                else
                                    ItemDrop(Items.Coal, new Vector2(physicalMap.Hitboxs[6, i].Center.X, physicalMap.Hitboxs[6, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[1] = 0;
                                break;
                            }
                        }
                        // < ----- Copper Drop >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[7, i]) &&
                            (p2Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                if (_p2Hand[0] == Items.Bare)
                                    _p2Hand[0] = Items.Copper;
                                if (_p2Hand[1] == Items.Bare)
                                    _p2Hand[1] = Items.Copper;
                                if (_p2Hand[2] == Items.Bare)
                                    _p2Hand[2] = Items.Copper;
                                else
                                    ItemDrop(Items.Copper, new Vector2(physicalMap.Hitboxs[7, i].Center.X, physicalMap.Hitboxs[7, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[1] = 0;
                                break;
                            }
                        }
                        // ----- < Iron Drop >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[8, i]) &&
                            (p2Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                if (_p2Hand[0] == Items.Bare)
                                    _p2Hand[0] = Items.Iron;
                                if (_p2Hand[1] == Items.Bare)
                                    _p2Hand[1] = Items.Iron;
                                if (_p2Hand[2] == Items.Bare)
                                    _p2Hand[2] = Items.Iron;
                                else
                                    ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[8, i].Center.X, physicalMap.Hitboxs[8, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[1] = 0;
                                break;
                            }
                        }
                        // ----- < Gold Drop >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[9, i]) &&
                            (p2Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronPickaxe ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                if (_p2Hand[0] == Items.Bare)
                                    _p2Hand[0] = Items.Gold;
                                if (_p2Hand[1] == Items.Bare)
                                    _p2Hand[1] = Items.Gold;
                                if (_p2Hand[2] == Items.Bare)
                                    _p2Hand[2] = Items.Gold;
                                else
                                    ItemDrop(Items.Gold, new Vector2(physicalMap.Hitboxs[9, i].Center.X, physicalMap.Hitboxs[9, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[1] = 0;
                                break;
                            }
                        }
                        // ----- < Shovel Cave-In >
                        if (p2.Hitbox.Intersects(physicalMap.Hitboxs[12, i]) &&
                            (p2Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p2Hand[_activeSlot[1]] == Items.GoldShovel ||
                            _p2Hand[_activeSlot[1]] == Items.IronShovel ||
                            _p2Hand[_activeSlot[1]] == Items.StoneShovel ||
                            _p2Hand[_activeSlot[1]] == Items.IronMultiTool ||
                            _p2Hand[_activeSlot[1]] == Items.StoneMultiTool))
                        {
                            _playerInteractCounter[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[1] > _playerInteractTime[1])
                            {
                                _playerInteractCounter[1] = 0;
                                physicalMap.caveInClear(physicalMap.Hitboxs[12, i]);
                                break;
                            }
                        }
                    }



                    // < Buy Wheel >
                    if (p2Pad.Triggers.Left >= 0.9f && (p2Pad.Buttons.X == ButtonState.Pressed && oldP2Pad.Buttons.X == ButtonState.Released))
                    {
                        BuyWheel(p2BuyWheel.wheelSlot);
                    }



                    // < Interact Reset >
                    if (p2Pad.Triggers.Right < 0.9f)
                        _playerInteractCounter[1] = 0;



                    _playerInteractTime[1] = HandItemStatCheck(_p2Hand[_activeSlot[1]]);
                    #endregion

                    p2.updateme(blankKB, blankKB, blankMouse, p2Pad, gameTime, physicalMap.Hitboxs, _p2Hand[_activeSlot[1]]);
                    #endregion
                }



                if (p3Connected == true)
                {
                    #region Player 3
                    #region Player 3 Controls
                    // < Item Bar Control >
                    // ----- < Next Hand Item Left >
                    if (p3Pad.Buttons.LeftShoulder == ButtonState.Pressed && oldP3Pad.Buttons.LeftShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[2]--;

                        if (_activeSlot[2] < 0)
                            _activeSlot[2] = 2;
                    }
                    // ----- < Next Hand Item Right >
                    else if (p3Pad.Buttons.RightShoulder == ButtonState.Pressed && oldP3Pad.Buttons.RightShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[2]++;

                        if (_activeSlot[2] > 2)
                            _activeSlot[2] = 0;
                    }



                    // < Item Drop Controls >
                    if (p3Pad.Buttons.B == ButtonState.Pressed && oldP3Pad.Buttons.B == ButtonState.Released)
                    {
                        if (_p3Hand[_activeSlot[2]] != Items.Bare)
                        {
                            ItemDrop(_p3Hand[_activeSlot[2]], p3.SourcePoint, p3.Velocity);
                            _p3Hand[_activeSlot[2]] = Items.Bare;
                        }
                    }



                    // < Item Pickup >
                    for (int i = 0; i < pysicalItem.Count; i++)
                    {
                        if (p3.Hitbox.Intersects(pysicalItem[i].Hitbox) && p3Pad.Buttons.X == ButtonState.Pressed && _p3Hand[_activeSlot[2]] == Items.Bare)
                        {
                            _p3Hand[_activeSlot[2]] = pysicalItem[i].Item;
                            pysicalItem.RemoveAt(i);
                        }
                    }



                    // < Deposit Cart Interact/Sell >
                    if (p3.Hitbox.Intersects(physicalMap.Hitboxs[10, 0]) && p3Pad.Buttons.X == ButtonState.Pressed)
                    {
                        _newMoney = 0;
                        _newMoneyTransparentsy = 0;

                        for (int i = 0; i < 3; i++)
                        {
                            switch (_p3Hand[i])
                            {
                                case Items.Bare:
                                    _p3Hand[i] = Items.Bare;
                                    break;

                                case Items.Coal:
                                    _money += 3;
                                    _newMoney += 3;
                                    _newMoneyTransparentsy = 2;
                                    _p3Hand[i] = Items.Bare;
                                    if (_level == 2)
                                        _level2OreCollected[0]++;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 4)
                                        _level4OreCollected[0]++;
                                    if (_level == 5)
                                        _level5OreCollected[0]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Copper:
                                    _money += 5;
                                    _newMoney += 5;
                                    _newMoneyTransparentsy = 2;
                                    _p3Hand[i] = Items.Bare;
                                    if (_level == 2)
                                        _level2OreCollected[1]++;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 4)
                                        _level4OreCollected[1]++;
                                    if (_level == 5)
                                        _level5OreCollected[1]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Iron:
                                    _money += 7;
                                    _newMoney += 7;
                                    _newMoneyTransparentsy = 2;
                                    _p3Hand[i] = Items.Bare;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 4)
                                        _level4OreCollected[2]++;
                                    if (_level == 5)
                                        _level5OreCollected[2]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Gold:
                                    _money += 10;
                                    _newMoney += 10;
                                    _newMoneyTransparentsy = 2;
                                    _p3Hand[i] = Items.Bare;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 5)
                                        _level5OreCollected[3]++;
                                    _depositCartInst.Play();
                                    break;
                            }
                        }
                    }



                    // < Ore Seam Interaction Controls >
                    for (int i = 0; i < 14; i++)
                    {
                        // ----- < Coal Drop >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[6, i]) &&
                            p3Pad.Triggers.Right > 0.9f &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p3Hand[0] == Items.Bare)
                                    _p3Hand[0] = Items.Coal;
                                if (_p3Hand[1] == Items.Bare)
                                    _p3Hand[1] = Items.Coal;
                                if (_p3Hand[2] == Items.Bare)
                                    _p3Hand[2] = Items.Coal;
                                else
                                    ItemDrop(Items.Coal, new Vector2(physicalMap.Hitboxs[6, i].Center.X, physicalMap.Hitboxs[6, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[2] = 0;
                                break;
                            }
                        }
                        // < ----- Copper Drop >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[7, i]) &&
                            (p3Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p3Hand[0] == Items.Bare)
                                    _p3Hand[0] = Items.Copper;
                                if (_p3Hand[1] == Items.Bare)
                                    _p3Hand[1] = Items.Copper;
                                if (_p3Hand[2] == Items.Bare)
                                    _p3Hand[2] = Items.Copper;
                                else
                                    ItemDrop(Items.Copper, new Vector2(physicalMap.Hitboxs[7, i].Center.X, physicalMap.Hitboxs[7, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[2] = 0;
                                break;
                            }
                        }
                        // ----- < Iron Drop >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[8, i]) &&
                            (p3Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p3Hand[0] == Items.Bare)
                                    _p3Hand[0] = Items.Iron;
                                if (_p3Hand[1] == Items.Bare)
                                    _p3Hand[1] = Items.Iron;
                                if (_p3Hand[2] == Items.Bare)
                                    _p3Hand[2] = Items.Iron;
                                else
                                    ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[8, i].Center.X, physicalMap.Hitboxs[8, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[2] = 0;
                                break;
                            }
                        }
                        // ----- < Gold Drop >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[9, i]) &&
                            (p3Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronPickaxe ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p3Hand[0] == Items.Bare)
                                    _p3Hand[0] = Items.Gold;
                                if (_p3Hand[1] == Items.Bare)
                                    _p3Hand[1] = Items.Gold;
                                if (_p3Hand[2] == Items.Bare)
                                    _p3Hand[2] = Items.Gold;
                                else
                                    ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[9, i].Center.X, physicalMap.Hitboxs[9, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[2] = 0;
                                break;
                            }
                        }
                        // ----- < Shovel Cave-In >
                        if (p3.Hitbox.Intersects(physicalMap.Hitboxs[12, i]) &&
                            (p3Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p3Hand[_activeSlot[2]] == Items.GoldShovel ||
                            _p3Hand[_activeSlot[2]] == Items.IronShovel ||
                            _p3Hand[_activeSlot[2]] == Items.StoneShovel ||
                            _p3Hand[_activeSlot[2]] == Items.IronMultiTool ||
                            _p3Hand[_activeSlot[2]] == Items.StoneMultiTool))
                        {
                            _playerInteractCounter[2] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                _playerInteractCounter[2] = 0;
                                physicalMap.caveInClear(physicalMap.Hitboxs[12, i]);
                                break;
                            }
                        }
                    }



                    // < Buy Wheel >
                    if (p3Pad.Triggers.Left >= 0.9f && (p3Pad.Buttons.X == ButtonState.Pressed && oldP3Pad.Buttons.X == ButtonState.Released))
                    {
                        BuyWheel(p3BuyWheel.wheelSlot);
                    }



                    // < Interact Reset >
                    if (p3Pad.Triggers.Right < 0.9f)
                        _playerInteractCounter[2] = 0;



                    _playerInteractTime[2] = HandItemStatCheck(_p3Hand[_activeSlot[2]]);
                    #endregion

                    p3.updateme(blankKB, blankKB, blankMouse, p3Pad, gameTime, physicalMap.Hitboxs, _p3Hand[_activeSlot[2]]);
                    #endregion
                }



                if (p4Connected == true)
                {
                    #region Player 4
                    #region Player 4 Controls
                    // < Item Bar Control >
                    // ----- < Next Hand Item Left >
                    if (p4Pad.Buttons.LeftShoulder == ButtonState.Pressed && oldP4Pad.Buttons.LeftShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[3]--;

                        if (_activeSlot[3] < 0)
                            _activeSlot[3] = 2;
                    }
                    // ----- < Next Hand Item Right >
                    else if (p4Pad.Buttons.RightShoulder == ButtonState.Pressed && oldP4Pad.Buttons.RightShoulder != ButtonState.Pressed)
                    {
                        _activeSlot[3]++;

                        if (_activeSlot[3] > 2)
                            _activeSlot[3] = 0;
                    }



                    // < Item Drop Controls >
                    if (p4Pad.Buttons.B == ButtonState.Pressed && oldP4Pad.Buttons.B == ButtonState.Released)
                    {
                        if (_p4Hand[_activeSlot[3]] != Items.Bare)
                        {
                            ItemDrop(_p4Hand[_activeSlot[3]], p4.SourcePoint, p4.Velocity);
                            _p4Hand[_activeSlot[3]] = Items.Bare;
                        }
                    }



                    // < Item Pickup >
                    for (int i = 0; i < pysicalItem.Count; i++)
                    {
                        if (p4.Hitbox.Intersects(pysicalItem[i].Hitbox) && p4Pad.Buttons.X == ButtonState.Pressed && _p4Hand[_activeSlot[3]] == Items.Bare)
                        {
                            _p4Hand[_activeSlot[3]] = pysicalItem[i].Item;
                            pysicalItem.RemoveAt(i);
                        }
                    }



                    // < Deposit Cart Interact/Sell >
                    if (p4.Hitbox.Intersects(physicalMap.Hitboxs[10, 0]) && p4Pad.Buttons.X == ButtonState.Pressed)
                    {
                        _newMoney = 0;
                        _newMoneyTransparentsy = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            switch (_p4Hand[i])
                            {
                                case Items.Bare:
                                    _p4Hand[i] = Items.Bare;
                                    break;

                                case Items.Coal:
                                    _money += 3;
                                    _newMoney += 3;
                                    _newMoneyTransparentsy = 2;
                                    _p4Hand[i] = Items.Bare;
                                    if (_level == 2)
                                        _level2OreCollected[0]++;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 4)
                                        _level4OreCollected[0]++;
                                    if (_level == 5)
                                        _level5OreCollected[0]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Copper:
                                    _money += 5;
                                    _newMoney += 5;
                                    _newMoneyTransparentsy = 2;
                                    _p4Hand[i] = Items.Bare;
                                    if (_level == 2)
                                        _level2OreCollected[1]++;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 4)
                                        _level4OreCollected[1]++;
                                    if (_level == 5)
                                        _level5OreCollected[1]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Iron:
                                    _money += 7;
                                    _newMoney += 7;
                                    _newMoneyTransparentsy = 2;
                                    _p4Hand[i] = Items.Bare;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 4)
                                        _level4OreCollected[2]++;
                                    if (_level == 5)
                                        _level5OreCollected[2]++;
                                    _depositCartInst.Play();
                                    break;

                                case Items.Gold:
                                    _money += 10;
                                    _newMoney += 10;
                                    _newMoneyTransparentsy = 2;
                                    _p4Hand[i] = Items.Bare;
                                    if (_level == 3)
                                        _level3OreCollected++;
                                    if (_level == 5)
                                        _level5OreCollected[3]++;
                                    _depositCartInst.Play();
                                    break;
                            }
                        }
                    }



                    // < Ore Seam Interaction Controls >
                    for (int i = 0; i < 14; i++)
                    {
                        // ----- < Coal Drop >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[6, i]) &&
                            p4Pad.Triggers.Right > 0.9f &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p4Hand[0] == Items.Bare)
                                    _p4Hand[0] = Items.Coal;
                                if (_p4Hand[1] == Items.Bare)
                                    _p4Hand[1] = Items.Coal;
                                if (_p4Hand[2] == Items.Bare)
                                    _p4Hand[2] = Items.Coal;
                                else
                                    ItemDrop(Items.Coal, new Vector2(physicalMap.Hitboxs[6, i].Center.X, physicalMap.Hitboxs[6, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[3] = 0;
                                break;
                            }
                        }
                        // < ----- Copper Drop >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[7, i]) &&
                            (p4Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p4Hand[0] == Items.Bare)
                                    _p4Hand[0] = Items.Copper;
                                if (_p4Hand[1] == Items.Bare)
                                    _p4Hand[1] = Items.Copper;
                                if (_p4Hand[2] == Items.Bare)
                                    _p4Hand[2] = Items.Copper;
                                else
                                    ItemDrop(Items.Copper, new Vector2(physicalMap.Hitboxs[7, i].Center.X, physicalMap.Hitboxs[7, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[3] = 0;
                                break;
                            }
                        }
                        // ----- < Iron Drop >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[8, i]) &&
                            (p4Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p4Hand[0] == Items.Bare)
                                    _p4Hand[0] = Items.Iron;
                                if (_p4Hand[1] == Items.Bare)
                                    _p4Hand[1] = Items.Iron;
                                if (_p4Hand[2] == Items.Bare)
                                    _p4Hand[2] = Items.Iron;
                                else
                                    ItemDrop(Items.Iron, new Vector2(physicalMap.Hitboxs[8, i].Center.X, physicalMap.Hitboxs[8, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[3] = 0;
                                break;
                            }
                        }
                        // ----- < Gold Drop >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[9, i]) &&
                            (p4Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronPickaxe ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StonePickaxe))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                if (_p4Hand[0] == Items.Bare)
                                    _p4Hand[0] = Items.Gold;
                                if (_p4Hand[1] == Items.Bare)
                                    _p4Hand[1] = Items.Gold;
                                if (_p4Hand[2] == Items.Bare)
                                    _p4Hand[2] = Items.Gold;
                                else
                                    ItemDrop(Items.Gold, new Vector2(physicalMap.Hitboxs[9, i].Center.X, physicalMap.Hitboxs[9, i].Center.Y), Vector2.Zero);

                                _playerInteractCounter[3] = 0;
                                break;
                            }
                        }
                        // ----- < Shovel Cave-In >
                        if (p4.Hitbox.Intersects(physicalMap.Hitboxs[12, i]) &&
                            (p4Pad.Triggers.Right > 0.9f || curr_mouse.LeftButton == ButtonState.Pressed) &&
                            (_p4Hand[_activeSlot[3]] == Items.GoldShovel ||
                            _p4Hand[_activeSlot[3]] == Items.IronShovel ||
                            _p4Hand[_activeSlot[3]] == Items.StoneShovel ||
                            _p4Hand[_activeSlot[3]] == Items.IronMultiTool ||
                            _p4Hand[_activeSlot[3]] == Items.StoneMultiTool))
                        {
                            _playerInteractCounter[3] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            if (_playerInteractCounter[2] > _playerInteractTime[2])
                            {
                                _playerInteractCounter[3] = 0;
                                physicalMap.caveInClear(physicalMap.Hitboxs[12, i]);
                                break;
                            }
                        }
                    }



                    // < Buy Wheel >
                    if (p4Pad.Triggers.Left >= 0.9f && (p4Pad.Buttons.X == ButtonState.Pressed && oldP4Pad.Buttons.X == ButtonState.Released))
                    {
                        BuyWheel(p4BuyWheel.wheelSlot);                      
                    }



                    // < Interact Reset >
                    if (p4Pad.Triggers.Right < 0.9f)
                        _playerInteractCounter[3] = 0;



                    _playerInteractTime[3] = HandItemStatCheck(_p4Hand[_activeSlot[3]]);
                    #endregion

                    p4.updateme(blankKB, blankKB, blankMouse, p4Pad, gameTime, physicalMap.Hitboxs, _p4Hand[_activeSlot[3]]);
                    #endregion
                }



                for (int i = 0; i < pysicalItem.Count; i++)
                {
                    pysicalItem[i].updateme(physicalMap.Hitboxs);
                }



                #region Player 1 Hud
                p1Pointer.updateme(gameTime, p1.Hitbox, _p1Hand, kb, p1Pad, curr_mouse, physicalMap.Hitboxs, p1.ActionState, _keyboardLastUsed, _gamePadLastUsed, _activeSlot[0]);
                for (int i = 0; i < pysicalItem.Count; i++)
                { p1Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p1.Hitbox, pysicalItem[i].Item, _p1Hand, _keyboardLastUsed, _gamePadLastUsed); }
                p1InteractBar.updateme(p1.Hitbox, _playerInteractTime[0], _playerInteractCounter[0]);
                p1BuyWheel.updatemeP1(p1.Hitbox, p1Pad, curr_mouse, _keyboardLastUsed, _gamePadLastUsed);
                #endregion



                if (p2Connected == true)
                {
                    #region Player 2 Hud
                    p2Pointer.updateme(gameTime, p2.Hitbox, _p2Hand, blankKB, p2Pad, blankMouse, physicalMap.Hitboxs, p2.ActionState, false, true, _activeSlot[1]);
                    for (int i = 0; i < pysicalItem.Count; i++)
                    { p2Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p2.Hitbox, pysicalItem[i].Item, _p2Hand, false, true); }
                    p2InteractBar.updateme(p2.Hitbox, _playerInteractTime[1], _playerInteractCounter[1]);
                    p2BuyWheel.updatemeGENERIC(p2.Hitbox, p2Pad);
                    #endregion
                }



                if (p3Connected == true)
                {
                    #region Player 3 Hud
                    p3Pointer.updateme(gameTime, p3.Hitbox, _p3Hand, blankKB, p3Pad, blankMouse, physicalMap.Hitboxs, p3.ActionState, false, true, _activeSlot[2]);
                    for (int i = 0; i < pysicalItem.Count; i++)
                    { p3Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p3.Hitbox, pysicalItem[i].Item, _p3Hand, false, true); }
                    p3InteractBar.updateme(p3.Hitbox, _playerInteractTime[2], _playerInteractCounter[2]);
                    p3BuyWheel.updatemeGENERIC(p3.Hitbox, p3Pad);
                    #endregion
                }



                if (p4Connected == true)
                {
                    #region Player 4 Hud
                    p4Pointer.updateme(gameTime, p4.Hitbox, _p4Hand, blankKB, p4Pad, blankMouse, physicalMap.Hitboxs, p4.ActionState, false, true, _activeSlot[3]);
                    for (int i = 0; i < pysicalItem.Count; i++)
                    { p4Pointer.physicalItemInteractPromptFinder(pysicalItem[i].Hitbox, p4.Hitbox, pysicalItem[i].Item, _p4Hand, false, true); }
                    p4InteractBar.updateme(p4.Hitbox, _playerInteractTime[3], _playerInteractCounter[3]);
                    p4BuyWheel.updatemeGENERIC(p4.Hitbox, p4Pad);
                    #endregion
                }
            }

            #region LevelWrap
            if (_currentTime <= -2 && _transitioning == false)
            {
                Transition(gameState.LevelSelect, 0);
                int p = _numberOfPlayers;



                // < Level 2 Star Check >
                if (_level == 2)
                {
                    // ----- < 1 Star Check >
                    if (_level2OreCollected[0] >= (_level2Goal1[0] * p) && _level2OreCollected[1] >= (_level2Goal1[1] * p))
                    {
                        _levelsComplete[1] = true;
                        _levelStars[1] = 1;

                        // ----- < 2 Star Check >
                        if (_level2OreCollected[0] >= (_level2Goal2[0] * p) && _level2OreCollected[1] >= (_level2Goal2[1] * p))
                            _levelStars[1] = 2;

                        // ----- < 3 Star Check >
                        if (_level2OreCollected[0] >= (_level2Goal3[0] * p) && _level2OreCollected[1] >= (_level2Goal3[1] * p))
                            _levelStars[1] = 3;
                    }
                }



                // < Level 3 Star Check >
                if (_level == 3)
                {
                    // ----- < 1 Star Check >
                    if (_level3OreCollected >= (_level3Goal1 * p))
                    {
                        _levelsComplete[2] = true;
                        _levelStars[2] = 1;

                        // ----- < 2 Star Check >
                        if (_level3OreCollected >= (_level3Goal2 * p))
                            _levelStars[2] = 2;

                        // ----- < 3 Star Check >
                        if (_level3OreCollected >= (_level3Goal3 * p))
                            _levelStars[2] = 3;
                    }
                }



                // < Level 4 Star Check >
                if (_level == 4)
                {
                    // ----- < 1 Star Check >
                    if (_level4OreCollected[0] >= (_level4Goal1[0] * p) && 
                        _level4OreCollected[1] >= (_level4Goal1[1] * p) && 
                        _level4OreCollected[2] >= (_level4Goal1[2] * p) && 
                        _money >= (_level4Goal1[3] * p))
                    {
                        _levelStars[3] = 1;
                        _levelsComplete[3] = true;

                        // ----- < 2 Star Check >
                        if (_level4OreCollected[0] >= (_level4Goal2[0] * p) && 
                            _level4OreCollected[1] >= (_level4Goal2[1] * p) && 
                            _level4OreCollected[2] >= (_level4Goal2[2] * p) && 
                            _money >= (_level4Goal2[3] * p))
                        {
                            _levelStars[3] = 2;

                            // ----- < 3 Star Check >
                            if (_level4OreCollected[0] >= (_level4Goal3[0] * p) && 
                                _level4OreCollected[1] >= (_level4Goal3[1] * p) && 
                                _level4OreCollected[2] >= (_level4Goal3[2] * p) && 
                                _money >= (_level4Goal3[3] * p))
                            {
                                _levelStars[3] = 2;
                            }
                        }
                    }
                }



                // < Level 5 Star Check >
                if (_level == 5)
                {
                    // ----- < 1 Star Check >
                    if (_level5OreCollected[0] >= (_level5Goal1[0] * p) &&
                        _level5OreCollected[1] >= (_level5Goal1[1] * p) &&
                        _level5OreCollected[2] >= (_level5Goal1[2] * p) &&
                        _level5OreCollected[3] >= (_level5Goal1[3] * p) &&
                        _money >= (_level5Goal1[4] * p))
                    {
                        _levelStars[4] = 1;
                        _levelsComplete[4] = true;

                        // ----- < 2 Star Check >
                        if (_level5OreCollected[0] >= (_level5Goal2[0] * p) &&
                            _level5OreCollected[1] >= (_level5Goal2[1] * p) &&
                            _level5OreCollected[2] >= (_level5Goal2[2] * p) &&
                            _level5OreCollected[3] >= (_level5Goal2[3] * p) &&
                            _money >= (_level5Goal2[4] * p))
                        {
                            _levelStars[4] = 2;

                            // ----- < 3 Star Check >
                            if (_level5OreCollected[0] >= (_level5Goal3[0] * p) &&
                                _level5OreCollected[1] >= (_level5Goal3[1] * p) &&
                                _level5OreCollected[2] >= (_level5Goal3[2] * p) &&
                                _level5OreCollected[3] >= (_level5Goal3[3] * p) &&
                                _money >= (_level5Goal3[4] * p))
                            {
                                _levelStars[4] = 3;
                            }
                        }
                    }
                }
            }
            #endregion



            cursorHand.updateme(curr_mouse, _p1Hand[_activeSlot[0]]);
        }





        void Transition(gameState target, int level)
        {
            _transitioning = true;
            wipeTransition = new WipeTransition(
                Content.Load<Texture2D>("Screens/BrickTransition"),
                60,
                new Rectangle(0, 0, 1344, 756),
                _screen);
            _targetScreen = target;
            _targetLevel = level;
        }





        int HandItemStatCheck(Items checkedItem)
        {
            switch(checkedItem)
            {
                case Items.StoneMultiTool:
                    return 10;

                case Items.StonePickaxe:
                    return 7;

                case Items.StoneShovel:
                    return 7;

                case Items.IronMultiTool:
                    return 7;

                case Items.IronPickaxe:
                    return 5;

                case Items.IronShovel:
                    return 5;

                case Items.GoldPickaxe:
                    return 3;

                case Items.GoldShovel:
                    return 3;

                default:
                    return 0;    
            }
        }





        void levelSwitch(int targetLevel)
        {
            MediaPlayer.Stop();

            pysicalItem.Clear();
            lanterns.Clear();

            _activeSlot = new int[4] { 0, 0, 0, 0 };
            _oldActiveSlot = new int[4] { 0, 0, 0, 0 };



            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    _p1Hand[j] = Items.Bare;
                    _p2Hand[j] = Items.Bare;
                    _p3Hand[j] = Items.Bare;
                    _p4Hand[j] = Items.Bare;
                }
            }



            #region Level Switchboard
            _level = targetLevel;
            Vector2 mapSize = new Vector2(0, 0);
            switch (_level)
            {
                case 0:
                    _map = new int[14, 14]
                    {
                        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 9, 1, 8, 1,34, 1, 8,17,18, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7, 7, 7,15, 7, 7, 7, 7},
                        {7, 7, 7, 7,29,17, 8, 34,1,14, 1,18, 7, 7},
                        {7, 7, 7, 7, 7,32,34,17, 8,11, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7,33,34,13, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7,32, 1, 8,24, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                    };
                    mapSize = new Vector2(14, 14);

                    for (int i = 0; i < _levelSelectLights.Length; i++)
                    {
                        lanterns.Add( new Lantern(
                            new Rectangle(
                                (int)(_baseMapCell.Width * _levelSelectLights[i].X),
                                (int)(_baseMapCell.Height * _levelSelectLights[i].Y),
                                _baseMapCell.Width,
                                _baseMapCell.Height),
                            Content.Load<Texture2D>("PysicalItems/Lantern"),
                            Content.Load<Texture2D>("PysicalItems/LampLight")));
                    }

                    _money = 0;
                    break;



                case 1:
                    _map = new int[9, 13]
                    {
                        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7,10, 7, 7, 7, 7,27, 1,16, 7, 7, 7},
                        {7, 7, 9, 1,34, 1,17,20,34,14,18, 7, 7},
                        {7, 7, 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7,29, 1,34,14, 1, 4, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                    };
                    mapSize = new Vector2(9, 13);

                    _money = 7 * _numberOfPlayers;
                    _currentTime = _tutorialTime;

                    tutorialObject = new TutorialObject(
                        new Rectangle(10, 578, 421, 452),
                        Content.Load<Texture2D>("Tutorial/Miner Mike Scaled"),
                        Content.Load<SpriteFont>("Fonts/MainFont"));

                    for (int i = 0; i < _tutorialLights.Length; i++)
                    {
                        lanterns.Add(new Lantern(
                            new Rectangle(
                                (int)(_baseMapCell.Width * _tutorialLights[i].X),
                                (int)(_baseMapCell.Height * _tutorialLights[i].Y),
                                _baseMapCell.Width,
                                _baseMapCell.Height),
                            Content.Load<Texture2D>("PysicalItems/Lantern"),
                            Content.Load<Texture2D>("PysicalItems/LampLight")));
                    }
                    break;



                case 2:
                    _map = new int[8, 11]
                    {
                        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7,10, 7,33,18, 7, 7, 7, 7, 7},
                        { 7, 7,10, 7,32,23, 1,16, 7, 7, 7},
                        { 7, 7, 9,34,23,19, 7,15, 7, 7, 7},
                        { 7, 7, 7,29,20,34, 1,14, 4, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                    };
                    mapSize = new Vector2(8, 11);

                    _money = 12 * _numberOfPlayers;
                    _currentTime = _level2Time;
                    _level2OreCollected = new int[2];

                    for (int i = 0; i < _level2Lights.Length; i++)
                    {
                        lanterns.Add(new Lantern(
                            new Rectangle(
                                (int)(_baseMapCell.Width * _level2Lights[i].X),
                                (int)(_baseMapCell.Height * _level2Lights[i].Y),
                                _baseMapCell.Width,
                                _baseMapCell.Height),
                            Content.Load<Texture2D>("PysicalItems/Lantern"),
                            Content.Load<Texture2D>("PysicalItems/LampLight")));
                    }
                    break;



                case 3:
                    _map = new int[15, 17]
                    {
                        {7, 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7,10, 7,33, 1, 4, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7,10, 7,15, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7,10, 7,32,23, 1, 1, 1,17, 1, 4, 7, 7, 7, 7},
                        {7, 7, 7,10, 7, 7,26,22, 7, 7,15, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 9, 1,34,17,20,16,33,14,17,34, 1, 4, 7, 7},
                        {7, 7, 7, 7, 7, 7,15, 7,32,13, 7,15, 7, 7, 7, 7, 7},
                        {7, 7,25,34,23, 1,14, 4, 7, 7,33,14,34,22, 7, 7, 7},
                        {7, 7, 7, 7,26,22, 7, 7, 7, 7,15, 7,27,19, 7, 7, 7},
                        {7, 7, 7,25, 1,20, 1,17, 1,34,14, 1,19, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7,25,14,34, 1, 4, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                    };
                    mapSize = new Vector2(15, 17);

                    _level3OreCollected = 0;
                    _currentTime = _level3Time;
                    _money = 0;

                    for (int j = 0; j < _level3NumberOfOres.Length; j++)
                    {
                        for (int i = 0; i < _level3NumberOfOres[j]; i++)
                        {
                            int rand = RNG.Next(0, _level3Spawns.Length);
                            int rand2 = RNG.Next(-256, 256);
                            if (j == 0)
                            {
                                ItemDrop(
                                    Items.Coal,
                                    new Vector2(
                                        ((_baseMapCell.Width * _level3Spawns[rand].X) + 256) + rand2,
                                        (_baseMapCell.Height * _level3Spawns[rand].Y) + 256),
                                    new Vector2(0, 0));
                            }
                            if (j == 1)
                            {
                                ItemDrop(
                                    Items.Copper,
                                    new Vector2(
                                        ((_baseMapCell.Width * _level3Spawns[rand].X) + 256) + rand2,
                                        (_baseMapCell.Height * _level3Spawns[rand].Y) + 256),
                                    new Vector2(0, 0));
                            }
                            if (j == 2)
                            {
                                ItemDrop(
                                    Items.Iron,
                                    new Vector2(
                                        ((_baseMapCell.Width * _level3Spawns[rand].X) + 256) + rand2,
                                        (_baseMapCell.Height * _level3Spawns[rand].Y) + 256),
                                    new Vector2(0, 0));
                            }
                            if (j == 3)
                            {
                                ItemDrop(
                                    Items.Gold,
                                    new Vector2(
                                        ((_baseMapCell.Width * _level3Spawns[rand].X) + 256) + rand2,
                                        (_baseMapCell.Height * _level3Spawns[rand].Y) + 256),
                                    new Vector2(0, 0));
                            }
                        }
                    }

                    for (int i = 0; i < _level3Lights.Length; i++)
                    {
                        lanterns.Add(new Lantern(
                            new Rectangle(
                                (int)(_baseMapCell.Width * _level3Lights[i].X),
                                (int)(_baseMapCell.Height * _level3Lights[i].Y),
                                _baseMapCell.Width,
                                _baseMapCell.Height),
                            Content.Load<Texture2D>("PysicalItems/Lantern"),
                            Content.Load<Texture2D>("PysicalItems/LampLight")));
                    }
                    break;



                case 4:
                    _map = new int[12, 15]
                    {
                        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7, 9,34,16, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7, 7, 7,15, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7, 7, 7,32,16,31,23,34, 1,34,17, 4, 7, 7},
                        { 7, 7, 7, 7, 7,15, 7,21, 7, 7,33,14,16, 7, 7},
                        { 7, 7, 7, 7, 7,32, 1,20,17, 1,14, 4,15, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7, 7,15, 7, 7,33,13, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7,28,14,34,34,14,12, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                    };
                    mapSize = new Vector2(12, 15);
                    _level4OreCollected = new int[3];
                    _money = 15 * _numberOfPlayers;
                    _currentTime = _level4Time;

                    for (int i = 0; i < _level4Lights.Length; i++)
                    {
                        lanterns.Add(new Lantern(
                            new Rectangle(
                                (int)(_baseMapCell.Width * _level4Lights[i].X),
                                (int)(_baseMapCell.Height * _level4Lights[i].Y),
                                _baseMapCell.Width,
                                _baseMapCell.Height),
                            Content.Load<Texture2D>("PysicalItems/Lantern"),
                            Content.Load<Texture2D>("PysicalItems/LampLight")));
                    }
                    break;



                case 5:
                    _map = new int[14, 15]
                    {
                        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7,10, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7, 9,34,16, 7,33,34,23,12, 7, 7, 7, 7, 7},
                        { 7, 7, 7, 7,15, 7,15, 7,21, 7, 7, 7, 7, 7, 7},
                        { 7, 7,33,34,14, 1,14, 1,20,18, 7, 7, 7, 7, 7},
                        { 7, 7,15, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7,32, 1,17, 1,17, 4,27, 1,16, 7, 7, 7, 7},
                        { 7, 7, 7, 7,15, 7,32, 1,19, 7,15, 7, 7, 7, 7},
                        { 7, 7,31,34,14, 1,34,22, 7,33,13, 7, 7, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7,21, 7,15, 7, 7, 7, 7, 7},
                        { 7, 7, 7, 7, 7, 7,30,20,34,13, 7, 7, 7, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                        { 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
                    };
                    mapSize = new Vector2(14, 15);

                    _currentTime = _level5Time;
                    _level5OreCollected = new int[4];
                    _money = 20 * _numberOfPlayers;

                    for (int i = 0; i < _level5Lights.Length; i++)
                    {
                        lanterns.Add(new Lantern(
                            new Rectangle(
                                (int)(_baseMapCell.Width * _level5Lights[i].X),
                                (int)(_baseMapCell.Height * _level5Lights[i].Y),
                                _baseMapCell.Width,
                                _baseMapCell.Height),
                            Content.Load<Texture2D>("PysicalItems/Lantern"),
                            Content.Load<Texture2D>("PysicalItems/LampLight")));
                    }
                    break;
            }
            #endregion



            physicalMap = new PhysicalMap(
               Content.Load<Texture2D>("Placeholder/MineTileSheet"),
               Content.Load<Texture2D>("Debug/pixel"),
               Content.Load<Texture2D>("PysicalItems/LampLight"),
               _caveInInst,
               _clearCaveInInst,
               _rockFallInst,
               _baseMapCell,
               _map,
               mapSize,
               _currentTime);



            #region Level Flash Cards
            levelFlashCard[0] = new FlashCard(
                new Rectangle(0, 0, 128, 128),
                Content.Load<Texture2D>("HUD/Star"),
                Content.Load<Texture2D>("HUD/HollowStar"),
                physicalMap.Hitboxs[5,0],
                "1# - Getting Started",
                _levelStars[0],
                Content.Load<SpriteFont>("Fonts/BiggestFont"),
                Content.Load<SpriteFont>("Fonts/MainFont"));

            levelFlashCard[1] = new FlashCard(
                new Rectangle(0, 0, 128, 128),
                Content.Load<Texture2D>("HUD/Star"),
                Content.Load<Texture2D>("HUD/HollowStar"),
                physicalMap.Hitboxs[5, 1],
                "2# - Side Step",
                _levelStars[1],
                Content.Load<SpriteFont>("Fonts/BiggestFont"),
                Content.Load<SpriteFont>("Fonts/MainFont"));

            levelFlashCard[2] = new FlashCard(
                new Rectangle(0, 0, 128, 128),
                Content.Load<Texture2D>("HUD/Star"),
                Content.Load<Texture2D>("HUD/HollowStar"),
                physicalMap.Hitboxs[5, 2],
                "3# - Lost & Found",
                _levelStars[2],
                Content.Load<SpriteFont>("Fonts/BiggestFont"),
                Content.Load<SpriteFont>("Fonts/MainFont"));

            levelFlashCard[3] = new FlashCard(
                new Rectangle(0, 0, 128, 128),
                Content.Load<Texture2D>("HUD/Star"),
                Content.Load<Texture2D>("HUD/HollowStar"),
                physicalMap.Hitboxs[5, 3],
                "4# - Iron Fever",
                _levelStars[3],
                Content.Load<SpriteFont>("Fonts/BiggestFont"),
                Content.Load<SpriteFont>("Fonts/MainFont"));

            levelFlashCard[4] = new FlashCard(
                new Rectangle(0, 0, 128, 128),
                Content.Load<Texture2D>("HUD/Star"),
                Content.Load<Texture2D>("HUD/HollowStar"),
                physicalMap.Hitboxs[5, 4],
                "5# - Rock Bottom",
                _levelStars[4],
                Content.Load<SpriteFont>("Fonts/BiggestFont"),
                Content.Load<SpriteFont>("Fonts/MainFont"));
            #endregion



            Vector2 charLoadPos;
            if (_level == 0)
                charLoadPos = _lastLevelEntered;
            else
                charLoadPos = physicalMap.StartPosition;



            _p1Hand = new Items[3];
            _p2Hand = new Items[3];
            _p3Hand = new Items[3];
            _p4Hand = new Items[3];

            _activeSlot = new int[4] { 0, 0, 0, 0 };
            _oldActiveSlot = new int[4] { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    _p1Hand[j] = Items.Bare;
                    _p2Hand[j] = Items.Bare;
                    _p3Hand[j] = Items.Bare;
                    _p4Hand[j] = Items.Bare;
                }
            }



            #region Player 1
            p1 = new PlayerCharacters(
               Content.Load<Texture2D>("Dwarfs/1"),
               Content.Load<Texture2D>("Dwarfs/playerBaseLight"),
               Content.Load<Texture2D>("Dwarfs/HeadLamp2"),
               _pickaxeHit,
               _shoveling,
               _walking, 
               4,
               _baseCharacterCell,
               new Rectangle(
                   (int)charLoadPos.X - 128,
                   (int)charLoadPos.Y - 128,
                   _baseCharacterCell.Width,
                   _baseCharacterCell.Height),
               _p1Hand[_activeSlot[0]]);

            p1Pointer = new PlayerPointer(
                new Rectangle(
                    0,
                    0,
                    128,
                    128),
                Content.Load<Texture2D>("HUD/Item Sheet"),
                Content.Load<Texture2D>("HUD/Pointer"),
                Content.Load<SpriteFont>("Fonts/MainFont"),
                new Color(172, 50, 50, 255),
                1);

            p1InteractBar = new InteractionBar(
                new Rectangle(0, 0, 256, 40),
                Content.Load<Texture2D>("HUD/Bar"),
                Content.Load<Texture2D>("Debug/pixel"),
                new Color(172, 50, 50, 255));

            p1BuyWheel = new BuyWheel(
                new Rectangle(0, 0, 512, 512),
                Content.Load<Texture2D>("HUD/WheelSheet"),
                Content.Load<Texture2D>("HUD/ItemWheel"),
                new Color(172, 50, 50, 255),
                Content.Load<SpriteFont>("Fonts/MainFont"));

            cursorHand = new CursorHand(
                new Rectangle(0, 0, 128, 128),
                Content.Load<Texture2D>("HUD/HandSheet"));
            #endregion



            #region Player 2
            int r = RNG.Next(-168, 168);

            p2 = new PlayerCharacters(
               Content.Load<Texture2D>("Dwarfs/2"),
               Content.Load<Texture2D>("Dwarfs/playerBaseLight"),
               Content.Load<Texture2D>("Dwarfs/HeadLamp2"),
               _pickaxeHit,
               _shoveling,
               _walking,
               4,
               _baseCharacterCell,
               new Rectangle(
                    ((int)charLoadPos.X - 128) + r,
                    ((int)charLoadPos.Y - 128),
                   _baseCharacterCell.Width,
                   _baseCharacterCell.Height),
               _p1Hand[_activeSlot[1]]);

            p2Pointer = new PlayerPointer(
                new Rectangle(
                    0,
                    0,
                    128,
                    128),
                Content.Load<Texture2D>("HUD/Item Sheet"),
                Content.Load<Texture2D>("HUD/Pointer"),
                Content.Load<SpriteFont>("Fonts/MainFont"),
                new Color(78, 100, 182, 255),
                2);

            p2InteractBar = new InteractionBar(
                new Rectangle(0, 0, 256, 40),
                Content.Load<Texture2D>("HUD/Bar"),
                Content.Load<Texture2D>("Debug/pixel"),
                new Color(78, 100, 182, 255));

            p2BuyWheel = new BuyWheel(
                new Rectangle(0, 0, 512, 512),
                Content.Load<Texture2D>("HUD/WheelSheet"),
                Content.Load<Texture2D>("HUD/ItemWheel"),
                new Color(78, 100, 182, 255),
                Content.Load<SpriteFont>("Fonts/MainFont"));
            #endregion



            #region Player 3
            int e = RNG.Next(-168, 168);

            p3 = new PlayerCharacters(
               Content.Load<Texture2D>("Dwarfs/3"),
               Content.Load<Texture2D>("Dwarfs/playerBaseLight"),
               Content.Load<Texture2D>("Dwarfs/HeadLamp2"), 
               _pickaxeHit,
               _shoveling,
               _walking,
               4,
               _baseCharacterCell,
               new Rectangle(
                    ((int)charLoadPos.X - 128) + e,
                    ((int)charLoadPos.Y - 128),
                   _baseCharacterCell.Width,
                   _baseCharacterCell.Height),
               _p1Hand[_activeSlot[2]]);

            p3Pointer = new PlayerPointer(
                new Rectangle(
                    0,
                    0,
                    128,
                    128),
                Content.Load<Texture2D>("HUD/Item Sheet"),
                Content.Load<Texture2D>("HUD/Pointer"),
                Content.Load<SpriteFont>("Fonts/MainFont"),
                new Color(108, 63, 125, 255),
                3);

            p3InteractBar = new InteractionBar(
                new Rectangle(0, 0, 256, 40),
                Content.Load<Texture2D>("HUD/Bar"),
                Content.Load<Texture2D>("Debug/pixel"),
                new Color(108, 63, 125, 255));

            p3BuyWheel = new BuyWheel(
                new Rectangle(0, 0, 512, 512),
                Content.Load<Texture2D>("HUD/WheelSheet"),
                Content.Load<Texture2D>("HUD/ItemWheel"),
                new Color(108, 63, 125, 255),
                Content.Load<SpriteFont>("Fonts/MainFont"));
            #endregion



            #region Player 4
            int f = RNG.Next(-168, 168);

            p4 = new PlayerCharacters(
               Content.Load<Texture2D>("Dwarfs/4"),
               Content.Load<Texture2D>("Dwarfs/playerBaseLight"),
               Content.Load<Texture2D>("Dwarfs/HeadLamp2"), 
               _pickaxeHit,
               _shoveling,
               _walking,
               4,
               _baseCharacterCell,
               new Rectangle(
                    ((int)charLoadPos.X - 128) + f,
                    ((int)charLoadPos.Y - 128),
                   _baseCharacterCell.Width,
                   _baseCharacterCell.Height),
               _p1Hand[_activeSlot[3]]);

            p4Pointer = new PlayerPointer(
                new Rectangle(
                    0,
                    0,
                    128,
                    128),
                Content.Load<Texture2D>("HUD/Item Sheet"),
                Content.Load<Texture2D>("HUD/Pointer"),
                Content.Load<SpriteFont>("Fonts/MainFont"),
                new Color(229, 221, 57, 255),
                4);

            p4InteractBar = new InteractionBar(
                new Rectangle(0, 0, 256, 40),
                Content.Load<Texture2D>("HUD/Bar"),
                Content.Load<Texture2D>("Debug/pixel"),
                new Color(229, 221, 57, 255));

            p4BuyWheel = new BuyWheel(
                new Rectangle(0, 0, 512, 512),
                Content.Load<Texture2D>("HUD/WheelSheet"),
                Content.Load<Texture2D>("HUD/ItemWheel"),
                new Color(229, 221, 57, 255),
                Content.Load<SpriteFont>("Fonts/MainFont"));
            #endregion
        }





        void ItemDrop(Items item, Vector2 position, Vector2 velocity)
        {
            pysicalItem.Add(
                new PysicalItem(
                    Content.Load<Texture2D>("PysicalItems/Item Sheet"),
                    new Rectangle((int)position.X - 64, (int)position.Y - 64, 128, 128),
                    velocity,
                    item));
        }





        void BuyWheel(int SelectedItem)
        {
            int rng = RNG.Next(-100, 100);
            Vector2 posModifer = new Vector2(rng, -_baseMapCell.Height * 2);

            if (SelectedItem == 1 && _money >= 7)
            {
                ItemDrop(Items.StonePickaxe, physicalMap.StartPosition + posModifer, Vector2.Zero);
                _money -= 5;
            }
            if (SelectedItem == 2 && _money >= 5)
            {
                ItemDrop(Items.StoneMultiTool, physicalMap.StartPosition + posModifer, Vector2.Zero);
                _money -= 7;
            }
            if (SelectedItem == 3 && _money >= 7)
            {
                ItemDrop(Items.StoneShovel, physicalMap.StartPosition + posModifer, Vector2.Zero);
                _money -= 5;
            }
            if (SelectedItem == 4 && _money >= 17)
            {
                ItemDrop(Items.IronPickaxe, physicalMap.StartPosition + posModifer, Vector2.Zero);
                _money -= 15;
            }
            if (SelectedItem == 5 && _money >= 15)
            {
                ItemDrop(Items.IronMultiTool, physicalMap.StartPosition + posModifer, Vector2.Zero);
                _money -= 17;
            }
            if (SelectedItem == 6 && _money >= 17)
            {
                ItemDrop(Items.IronShovel, physicalMap.StartPosition + posModifer, Vector2.Zero);
                _money -= 15;
            }
            if (SelectedItem == 7 && _money >= 27)
            {
                ItemDrop(Items.GoldPickaxe, physicalMap.StartPosition + posModifer, Vector2.Zero);
                _money -= 25;
            }
            if (SelectedItem == 8 && _money >= 25)
            {
                ItemDrop(Items.GoldShovel, physicalMap.StartPosition + posModifer, Vector2.Zero);
                _money -= 25;
            }
        }





        void ResetGame()
        {
            splashSequence = new SplashSequence(
                _screen,
                Content.Load<Texture2D>("Screens/screenBackground"),
                Content.Load<Texture2D>("Screens/MonoGame2"),
                Content.Load<Texture2D>("Screens/Vega"));

            tutorialObject = new TutorialObject(
                new Rectangle(10, 578, 421, 452),
                Content.Load<Texture2D>("Tutorial/Miner Mike Scaled"),
                Content.Load<SpriteFont>("Fonts/MainFont"));

            titleCard = new titleCard(
                _screen,
                Content.Load<Texture2D>("Screens/screenBackground"),
                Content.Load<Texture2D>("Screens/Title2"),
                Content.Load<SpriteFont>("Fonts/MainFont"));

             _levelStars = new int[5];
            _levelsComplete = new bool[5];
            _playedBefore = false;
            _lastLevelEntered = new Vector2(1280, 2304);
        }





        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_defaltColour);

            switch (_gameState)
            {
                case gameState.Splash:
                    drawSplash();
                    break;

                case gameState.Title:
                    drawTitle();
                    break;

                case gameState.LevelSelect:
                    drawLevelSelect();
                    break;

                case gameState.Tutorial:
                    drawTutorial();
                    break;

                case gameState.Level:
                    drawLevel();
                    break;
            }

            _spriteBatch.Begin();

            if (_transitioning == true)
                wipeTransition.drawme(_spriteBatch);

            if (_keyboardLastUsed == true)
                cursorHand.drawme(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }





        void drawSplash()
        { 
            _spriteBatch.Begin();
            GraphicsDevice.Clear(Color.Black);
            splashSequence.drawme(_spriteBatch);
            _spriteBatch.End() ;
        }





        void drawTitle() 
        {
            _spriteBatch.Begin();
            titleCard.drawme(_spriteBatch, _keyboardLastUsed);
            _spriteBatch.End();
        }





        void drawLevelSelect()
        {
            // Generate Scene
            GraphicsDevice.SetRenderTarget(_preCanvas);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            #region objectsInCam
            // < Draw Level >
            physicalMap.drawme(_spriteBatch);

            for (int i = 0; i < lanterns.Count; i++)
            {
                lanterns[i].drawme(_spriteBatch);
            }

            // < Draw Player Characters >
            p1.drawme(_spriteBatch);
            if (p2Connected == true)
                p2.drawme(_spriteBatch);    
            if (p3Connected == true)
                p3.drawme(_spriteBatch);
            if (p4Connected == true)
                p4.drawme(_spriteBatch);

            // < Draw Phsical Items >
            for (int i = 0; i < pysicalItem.Count; i++) { pysicalItem[i].drawme(_spriteBatch); }

            


            #endregion
            _spriteBatch.End();



            GraphicsDevice.SetRenderTarget(_lightMask);



            // < Generate Mask >
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            for (int i = 0; i < lanterns.Count; i++) { lanterns[i].drawMask(_spriteBatch); }
            p1.drawMask(_spriteBatch);
            if (p2Connected)
                p2.drawMask(_spriteBatch);
            if (p3Connected)
                p3.drawMask(_spriteBatch);
            if (p4Connected)
                p4.drawMask(_spriteBatch);
            physicalMap.drawMask(_spriteBatch);
            _spriteBatch.End();



            GraphicsDevice.SetRenderTarget(null);



            // < Draw Scene with Mask>
            _lightShader.Parameters["MaskTex"].SetValue(_lightMask);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, _lightShader);
            _spriteBatch.Draw(_preCanvas, Vector2.One, Color.White);
            _spriteBatch.End();



            // < Cam Foreground>
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            for (int i = 0; i < levelFlashCard.Length; i++) { levelFlashCard[i].drawme(_spriteBatch, _keyboardLastUsed); }
            
            // < Draw Player Character Pointers >
            p1Pointer.drawmeLevelSelect(_spriteBatch);
            if (p2Connected == true) p2Pointer.drawmeLevelSelect(_spriteBatch);
            if (p3Connected == true) p3Pointer.drawmeLevelSelect(_spriteBatch);
            if (p4Connected == true) p4Pointer.drawmeLevelSelect(_spriteBatch);

            if (p2Connected || p3Connected || p4Connected) 
                p1Crown.drawme(_spriteBatch);
            
            _spriteBatch.End();



            _spriteBatch.Begin();
            #region HUD Text
            // < Player Add Prompt >
            _spriteBatch.DrawString(_mainFont, "Press X on a GamePad to add players", new Vector2(20, 20), Color.Black);
            _spriteBatch.DrawString(_mainFont, "Press X on a GamePad to add players", new Vector2(24, 24), Color.White);

            // < Player Remove Prompt >
            _spriteBatch.DrawString(_mainFont, "Press B to Remove players", new Vector2(20, 100), Color.Black);
            _spriteBatch.DrawString(_mainFont, "Press B to Remove players", new Vector2(24, 104), Color.White);

            if (_isGamePaused == true)
            {
                // < Screen Shader >
                _spriteBatch.Draw(_overlay, _screen, Color.Black * 0.5f);

                Vector2 pausedLength = _pauseFont.MeasureString("Paused!");
                _spriteBatch.DrawString(_pauseFont, "Paused!", new Vector2(_screen.Center.X - (pausedLength.X / 2), _screen.Center.Y - (pausedLength.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_pauseFont, "Paused!", new Vector2(_screen.Center.X - (pausedLength.X / 2) + 5, _screen.Center.Y - (pausedLength.Y / 2) + 5), Color.White);

                if (_keyboardLastUsed == true)
                {
                    Vector2 exitLength = _mainFont.MeasureString("Press E to EXIT to TITLE");
                    _spriteBatch.DrawString(_mainFont, "Press E to EXIT to TITLE", new Vector2(_screen.Center.X - (exitLength.X / 2), _screen.Center.Y + 200 - (exitLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press E to EXIT to TITLE", new Vector2(_screen.Center.X - (exitLength.X / 2) + 5, _screen.Center.Y + 200 - (exitLength.Y / 2) + 5), Color.White);

                    Vector2 controlsLength = _mainFont.MeasureString("Press F for CONTROL SCHEME");
                    _spriteBatch.DrawString(_mainFont, "Press F for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2), _screen.Center.Y + 300 - (controlsLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press F for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2) + 5, _screen.Center.Y + 300 - (controlsLength.Y / 2) + 5), Color.White);

                    if (_controlScreenUp)
                    {
                        _spriteBatch.Draw(_controlsTexture, _screen, Color.White);

                        Vector2 controlsReturn = _mainFont.MeasureString("Press F to RETURN");
                        _spriteBatch.DrawString(_mainFont, "Press F to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2), _screen.Center.Y + 450 - (controlsLength.Y / 2)), Color.Black);
                        _spriteBatch.DrawString(_mainFont, "Press F to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2) + 5, _screen.Center.Y + 450 - (controlsLength.Y / 2) + 5), Color.White);
                    }
                }

                if (_gamePadLastUsed == true)
                {
                    Vector2 exitLength = _mainFont.MeasureString("Press A to EXIT to TITLE");
                    _spriteBatch.DrawString(_mainFont, "Press A to EXIT to TITLE", new Vector2(_screen.Center.X - (exitLength.X / 2), _screen.Center.Y + 200 - (exitLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press A to EXIT to TITLE", new Vector2(_screen.Center.X - (exitLength.X / 2) + 5, _screen.Center.Y + 200 - (exitLength.Y / 2) + 5), Color.White);

                    Vector2 controlsLength = _mainFont.MeasureString("Press Y for CONTROL SCHEME");
                    _spriteBatch.DrawString(_mainFont, "Press Y for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2), _screen.Center.Y + 300 - (controlsLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press Y for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2) + 5, _screen.Center.Y + 300 - (controlsLength.Y / 2) + 5), Color.White);

                    if (_controlScreenUp)
                    {
                        _spriteBatch.Draw(_controlsTexture, _screen, Color.White);

                        Vector2 controlsReturn = _mainFont.MeasureString("Press Y to RETURN");
                        _spriteBatch.DrawString(_mainFont, "Press Y to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2), _screen.Center.Y + 450 - (controlsLength.Y / 2)), Color.Black);
                        _spriteBatch.DrawString(_mainFont, "Press Y to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2) + 5, _screen.Center.Y + 450 - (controlsLength.Y / 2) + 5), Color.White);
                    }
                }
            }
            #endregion
            _spriteBatch.End();
        }





        void drawTutorial()
        {
            // < Generate Scene >
            GraphicsDevice.SetRenderTarget(_preCanvas);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            #region ObjectInCam

            // < Draw Level >
            physicalMap.drawme(_spriteBatch);

            for (int i = 0; i < lanterns.Count; i++)
            {
                lanterns[i].drawme(_spriteBatch);
            }

            // < Draw Player Characters >
            p1.drawme(_spriteBatch);
            if (p2Connected == true)
                p2.drawme(_spriteBatch);    
            if (p3Connected == true)
                p3.drawme(_spriteBatch);
            if (p4Connected == true)
                p4.drawme(_spriteBatch); 

            // < Draw Phsical Items >
            for (int i = 0; i < pysicalItem.Count; i++) { pysicalItem[i].drawme(_spriteBatch); }

           

          

            #endregion
            _spriteBatch.End();



            GraphicsDevice.SetRenderTarget(_lightMask);



            // < Generate Mask >
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            for (int i = 0; i < lanterns.Count; i++) { lanterns[i].drawMask(_spriteBatch); }
            p1.drawMask(_spriteBatch);
            if (p2Connected)
                p2.drawMask(_spriteBatch);
            if (p3Connected)
                p3.drawMask(_spriteBatch);
            if (p4Connected)
                p4.drawMask(_spriteBatch);
            _spriteBatch.End();



            GraphicsDevice.SetRenderTarget(null);



            // < Draw Scene with Mask>
            _lightShader.Parameters["MaskTex"].SetValue(_lightMask);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, _lightShader);
            _spriteBatch.Draw(_preCanvas, Vector2.One, Color.White);
            _spriteBatch.End();



            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            #region ObjectOutOfMask
            // < Draw Level Flash Cards >
            for (int i = 0; i < levelFlashCard.Length; i++) { levelFlashCard[i].drawme(_spriteBatch, _keyboardLastUsed); }
            
            // < Draw Player Character Pointers >
            p1Pointer.drawme(_spriteBatch, _activeSlot[0]);
            if (p2Connected == true)
                p2Pointer.drawme(_spriteBatch, _activeSlot[1]);
            if (p3Connected == true)
                p3Pointer.drawme(_spriteBatch, _activeSlot[2]);
            if (p4Connected == true)
                p4Pointer.drawme(_spriteBatch, _activeSlot[3]);

            // < Player Interaction Bars >
            // ----- < Player 1 >
            if ((curr_mouse.LeftButton == ButtonState.Pressed || p1Pad.Triggers.Right > 0.9f) &&
                (p1Pad.Buttons.Y == ButtonState.Released && kb.IsKeyUp(Keys.Tab)) &&
                _p1Hand[_activeSlot[0]] != Items.Bare &&
                _p1Hand[_activeSlot[0]] != Items.Coal &&
                _p1Hand[_activeSlot[0]] != Items.Copper &&
                _p1Hand[_activeSlot[0]] != Items.Iron &&
                _p1Hand[_activeSlot[0]] != Items.Gold)
                p1InteractBar.drawme(_spriteBatch);
            // ----- < Player 2 >
            if (p2Pad.Triggers.Right > 0.9f &&
                p2Pad.Buttons.Y == ButtonState.Released &&
                _p2Hand[_activeSlot[1]] != Items.Bare &&
                _p2Hand[_activeSlot[1]] != Items.Coal &&
                _p2Hand[_activeSlot[1]] != Items.Copper &&
                _p2Hand[_activeSlot[1]] != Items.Iron &&
                _p2Hand[_activeSlot[1]] != Items.Gold)
                p2InteractBar.drawme(_spriteBatch);
            // ----- < Player 3 >
            if (p3Pad.Triggers.Right > 0.9f &&
                p3Pad.Buttons.Y == ButtonState.Released &&
                _p3Hand[_activeSlot[2]] != Items.Bare &&
                _p3Hand[_activeSlot[2]] != Items.Coal &&
                _p3Hand[_activeSlot[2]] != Items.Copper &&
                _p3Hand[_activeSlot[2]] != Items.Iron && 
                _p3Hand[_activeSlot[2]] != Items.Gold)
                p3InteractBar.drawme(_spriteBatch);
            // ----- < Player 4 >
            if (p4Pad.Triggers.Right > 0.9f &&
                p4Pad.Buttons.Y == ButtonState.Released &&
                _p4Hand[_activeSlot[3]] != Items.Bare &&
                _p4Hand[_activeSlot[3]] != Items.Coal &&
                _p4Hand[_activeSlot[3]] != Items.Copper &&
                _p4Hand[_activeSlot[3]] != Items.Iron &&
                _p4Hand[_activeSlot[3]] != Items.Gold)
                p4InteractBar.drawme(_spriteBatch);



            // < Player Buy Wheels >
            // ----- < Player 1 >
            if ((p1Pad.Triggers.Left >= 0.9 || kb.IsKeyDown(Keys.C)) && (p1Pad.Buttons.Y != ButtonState.Pressed && kb.IsKeyUp(Keys.Tab)))
                p1BuyWheel.drawme(_spriteBatch);
            // ----- < Player 2 >
            if (p2Pad.Triggers.Left >= 0.9 && p2Pad.Buttons.Y != ButtonState.Pressed)
                p2BuyWheel.drawme(_spriteBatch);
            // ----- < Player 3 >
            if (p3Pad.Triggers.Left >= 0.9 && p3Pad.Buttons.Y != ButtonState.Pressed)
                p3BuyWheel.drawme(_spriteBatch);
            // ----- < Player 4 >
            if (p4Pad.Triggers.Left >= 0.9 && p4Pad.Buttons.Y != ButtonState.Pressed)
                p4BuyWheel.drawme(_spriteBatch);
            #endregion
            _spriteBatch.End();



            _spriteBatch.Begin();
            #region HUD Text
            // < Tutorial Draw >
            if (tutorialObject.finishedTutorial == false)
                tutorialObject.drawme(_spriteBatch);


            // < Game Timer >
            // ----- < Format Timer >
            int secondsInTimer = (int)_currentTime;
            int minuitesInTimer = 0;
            while (secondsInTimer > 59)
            {
                if (secondsInTimer - 60 > -1)
                {
                    minuitesInTimer++;
                    secondsInTimer -= 60;
                }
            }
            // ----- < Assemble Timer >
            string drawnTimer = "";
            if (secondsInTimer > 9)
                drawnTimer = minuitesInTimer + ":" + secondsInTimer;
            if (secondsInTimer <= 9)
                drawnTimer = minuitesInTimer + ":0" + secondsInTimer;
            if (secondsInTimer < 0)
                drawnTimer = minuitesInTimer + ":00";

            // ----- < Timer Draw >
            if (tutorialObject.finishedTutorial == true)
            {
                Vector2 timerSize = _biggerMainFont.MeasureString(drawnTimer);
                _spriteBatch.DrawString(_biggerMainFont, drawnTimer, new Vector2(_screen.Center.X - (timerSize.X / 2), _screen.Top + 20), Color.Black);
                _spriteBatch.DrawString(_biggerMainFont, drawnTimer, new Vector2(_screen.Center.X - (timerSize.X / 2) + 5, _screen.Top + 25), Color.White);
            }



            // < Money Draw >
            Vector2 moneyLength = _biggerMainFont.MeasureString("$" + _money);
            _spriteBatch.DrawString(_biggerMainFont, "$" + _money, new Vector2((_screen.Width - 30) - moneyLength.X, 30), Color.Black);
            _spriteBatch.DrawString(_biggerMainFont, "$" + _money, new Vector2((_screen.Width - 35) - moneyLength.X, 35), Color.White); 



            // < Task Draw >
            if (tutorialObject.finishedTutorial == true)
            {
                int currentTarget = (_tutorialGoal1 * _numberOfPlayers);
                int starGoal = 1;

                if (_money >= _tutorialGoal1 * _numberOfPlayers)
                {
                    currentTarget = (_tutorialGoal2 * _numberOfPlayers);
                    starGoal = 2;
                }
                if (_money >= _tutorialGoal2 * _numberOfPlayers)
                {
                    currentTarget = (_tutorialGoal3 * _numberOfPlayers);
                    starGoal = 3;
                }

                _spriteBatch.DrawString(_mainFont, starGoal + " STAR\n-------\n|> have $" + currentTarget + " in the BANK", new Vector2(_screen.Left + 30, 300), Color.Black);
                _spriteBatch.DrawString(_mainFont, starGoal + " STAR\n-------\n|> have $" + currentTarget + " in the BANK", new Vector2(_screen.Left + 33, 303), Color.White);
            }
            


            if (_isGamePaused == true)
            {
                // < Screen Dimmer >
                _spriteBatch.Draw(_overlay, _screen, Color.Black * 0.5f);

                Vector2 pausedLength = _pauseFont.MeasureString("Paused!");
                _spriteBatch.DrawString(_pauseFont, "Paused!", new Vector2(_screen.Center.X - (pausedLength.X / 2), _screen.Center.Y - (pausedLength.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_pauseFont, "Paused!", new Vector2(_screen.Center.X - (pausedLength.X / 2) + 5, _screen.Center.Y - (pausedLength.Y / 2) + 5), Color.White);

                if (_keyboardLastUsed == true)
                {
                    Vector2 exitLength = _mainFont.MeasureString("Press E to EXIT to HOME");
                    _spriteBatch.DrawString(_mainFont, "Press E to EXIT to HOME", new Vector2(_screen.Center.X - (exitLength.X / 2), _screen.Center.Y + 200 - (exitLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press E to EXIT to HOME", new Vector2(_screen.Center.X - (exitLength.X / 2) + 5, _screen.Center.Y + 200 - (exitLength.Y / 2) + 5), Color.White);

                    Vector2 resetLength = _mainFont.MeasureString("Press R to RESTART");
                    _spriteBatch.DrawString(_mainFont, "Press R to RESTART", new Vector2(_screen.Center.X - (resetLength.X / 2), _screen.Center.Y + 300 - (resetLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press R to RESTART", new Vector2(_screen.Center.X - (resetLength.X / 2) + 5, _screen.Center.Y + 300 - (resetLength.Y / 2) + 5), Color.White);

                    Vector2 controlsLength = _mainFont.MeasureString("Press F for CONTROL SCHEME");
                    _spriteBatch.DrawString(_mainFont, "Press F for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2), _screen.Center.Y + 400 - (controlsLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press F for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2) + 5, _screen.Center.Y + 400 - (controlsLength.Y / 2) + 5), Color.White);

                    if (_controlScreenUp)
                    {
                        _spriteBatch.Draw(_controlsTexture, _screen, Color.White);

                        Vector2 controlsReturn = _mainFont.MeasureString("Press F to RETURN");
                        _spriteBatch.DrawString(_mainFont, "Press F to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2), _screen.Center.Y + 450 - (controlsLength.Y / 2)), Color.Black);
                        _spriteBatch.DrawString(_mainFont, "Press F to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2) + 5, _screen.Center.Y + 450 - (controlsLength.Y / 2) + 5), Color.White);
                    }
                }
                if (_gamePadLastUsed == true)
                {
                    Vector2 exitLength = _mainFont.MeasureString("Press A to EXIT to HOME");
                    _spriteBatch.DrawString(_mainFont, "Press A to EXIT to HOME", new Vector2(_screen.Center.X - (exitLength.X / 2), _screen.Center.Y + 200 - (exitLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press A to EXIT to HOME", new Vector2(_screen.Center.X - (exitLength.X / 2) + 5, _screen.Center.Y + 200 - (exitLength.Y / 2) + 5), Color.White);

                    Vector2 resetLength = _mainFont.MeasureString("Press B to RESTART");
                    _spriteBatch.DrawString(_mainFont, "Press B to RESTART", new Vector2(_screen.Center.X - (resetLength.X / 2), _screen.Center.Y + 300 - (resetLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press B to RESTART", new Vector2(_screen.Center.X - (resetLength.X / 2) + 5, _screen.Center.Y + 300 - (resetLength.Y / 2) + 5), Color.White);
                    
                    Vector2 controlsLength = _mainFont.MeasureString("Press Y for CONTROL SCHEME");
                    _spriteBatch.DrawString(_mainFont, "Press Y for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2), _screen.Center.Y + 400 - (controlsLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press Y for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2) + 5, _screen.Center.Y + 400 - (controlsLength.Y / 2) + 5), Color.White);

                    if (_controlScreenUp)
                    {
                        _spriteBatch.Draw(_controlsTexture, _screen, Color.White);

                        Vector2 controlsReturn = _mainFont.MeasureString("Press Y to RETURN");
                        _spriteBatch.DrawString(_mainFont, "Press Y to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2), _screen.Center.Y + 450 - (controlsLength.Y / 2)), Color.Black);
                        _spriteBatch.DrawString(_mainFont, "Press Y to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2) + 5, _screen.Center.Y + 450 - (controlsLength.Y / 2) + 5), Color.White);
                    }
                }
            }
            #endregion
            _spriteBatch.End();
        }





        void drawLevel()
        {
            // < Generate Scene >
            GraphicsDevice.SetRenderTarget(_preCanvas);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            #region ObjectInCam
            // < Draw Level >
            physicalMap.drawme(_spriteBatch);

            for (int i = 0; i < lanterns.Count; i++)
            {
                lanterns[i].drawme(_spriteBatch);
            }

            // < Draw Player Characters >
            p1.drawme(_spriteBatch);
            if (p2Connected == true)
                p2.drawme(_spriteBatch);
            if (p3Connected == true)
                p3.drawme(_spriteBatch);
            if (p4Connected == true)
                p4.drawme(_spriteBatch);

            // < Draw Phsical Items >
            for (int i = 0; i < pysicalItem.Count; i++) { pysicalItem[i].drawme(_spriteBatch); }
            #endregion
            _spriteBatch.End();



            GraphicsDevice.SetRenderTarget(_lightMask);



            // < Generate Mask >
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            for (int i = 0; i < lanterns.Count; i++) { lanterns[i].drawMask(_spriteBatch); }
            p1.drawMask(_spriteBatch);
            if (p2Connected)
             p2.drawMask(_spriteBatch);
            if (p3Connected)
                p3.drawMask(_spriteBatch);
            if (p4Connected)
                p4.drawMask(_spriteBatch);
            _spriteBatch.End();



            GraphicsDevice.SetRenderTarget(null);



            // < Draw Scene with Mask>
            _lightShader.Parameters["MaskTex"].SetValue(_lightMask);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, _lightShader);
            _spriteBatch.Draw(_preCanvas, Vector2.One, Color.White);
            _spriteBatch.End();



            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.getCam());
            #region ObjectOutOfMask
            // < Draw Level Flash Cards >
            for (int i = 0; i < levelFlashCard.Length; i++) { levelFlashCard[i].drawme(_spriteBatch, _keyboardLastUsed); }

            // < Draw Player Character Pointers >
            p1Pointer.drawme(_spriteBatch, _activeSlot[0]);
            if (p2Connected == true)
                p2Pointer.drawme(_spriteBatch, _activeSlot[1]);
            if (p3Connected == true)
                p3Pointer.drawme(_spriteBatch, _activeSlot[2]);
            if (p4Connected == true)
                p4Pointer.drawme(_spriteBatch, _activeSlot[3]);



            // < Player Interaction Bars >
            // ----- < Player 1 >
            if ((curr_mouse.LeftButton == ButtonState.Pressed || p1Pad.Triggers.Right > 0.9f) &&
                (p1Pad.Buttons.Y == ButtonState.Released && kb.IsKeyUp(Keys.Tab)) &&
                _p1Hand[_activeSlot[0]] != Items.Bare &&
                _p1Hand[_activeSlot[0]] != Items.Coal &&
                _p1Hand[_activeSlot[0]] != Items.Copper &&
                _p1Hand[_activeSlot[0]] != Items.Iron &&
                _p1Hand[_activeSlot[0]] != Items.Gold)
                p1InteractBar.drawme(_spriteBatch);
            // ----- < Player 2 >
            if (p2Pad.Triggers.Right > 0.9f &&
                p2Pad.Buttons.Y == ButtonState.Released &&
                _p2Hand[_activeSlot[1]] != Items.Bare &&
                _p2Hand[_activeSlot[1]] != Items.Coal &&
                _p2Hand[_activeSlot[1]] != Items.Copper &&
                _p2Hand[_activeSlot[1]] != Items.Iron &&
                _p2Hand[_activeSlot[1]] != Items.Gold)
                p2InteractBar.drawme(_spriteBatch);
            // ----- < Player 3 >
            if (p3Pad.Triggers.Right > 0.9f &&
                p3Pad.Buttons.Y == ButtonState.Released &&
                _p3Hand[_activeSlot[2]] != Items.Bare &&
                _p3Hand[_activeSlot[2]] != Items.Coal &&
                _p3Hand[_activeSlot[2]] != Items.Copper &&
                _p3Hand[_activeSlot[2]] != Items.Iron &&
                _p3Hand[_activeSlot[2]] != Items.Gold)
                p3InteractBar.drawme(_spriteBatch);
            // ----- < Player 4 >
            if (p4Pad.Triggers.Right > 0.9f &&
                p4Pad.Buttons.Y == ButtonState.Released &&
                _p4Hand[_activeSlot[3]] != Items.Bare &&
                _p4Hand[_activeSlot[3]] != Items.Coal &&
                _p4Hand[_activeSlot[3]] != Items.Copper &&
                _p4Hand[_activeSlot[3]] != Items.Iron &&
                _p4Hand[_activeSlot[3]] != Items.Gold)
                p4InteractBar.drawme(_spriteBatch);



            // < Player Buy Wheels >
            // ----- < Player 1 >
            if ((p1Pad.Triggers.Left >= 0.9 || kb.IsKeyDown(Keys.C)) && (p1Pad.Buttons.Y != ButtonState.Pressed && kb.IsKeyUp(Keys.Tab)))
                p1BuyWheel.drawme(_spriteBatch);
            // ----- < Player 2 >
            if (p2Pad.Triggers.Left >= 0.9 && p2Pad.Buttons.Y != ButtonState.Pressed && p2Connected == true)
                p2BuyWheel.drawme(_spriteBatch);
            // ----- < Player 3 >
            if (p3Pad.Triggers.Left >= 0.9 && p3Pad.Buttons.Y != ButtonState.Pressed && p3Connected == true)
                p3BuyWheel.drawme(_spriteBatch);
            // ----- < Player 4 >
            if (p4Pad.Triggers.Left >= 0.9 && p4Pad.Buttons.Y != ButtonState.Pressed && p4Connected == true)
                p4BuyWheel.drawme(_spriteBatch);
            #endregion
            _spriteBatch.End();



            _spriteBatch.Begin();
            #region HUD Text

            // < Game Timer >
            // ----- < Format Timer >
            int secondsInTimer = (int)_currentTime;
            int minuitesInTimer = 0;
            while (secondsInTimer > 59)
            {
                if (secondsInTimer - 60 > -1)
                {
                    minuitesInTimer++;
                    secondsInTimer -= 60;
                }
            }
            // ----- < Assemble Timer >
            string drawnTimer = "";
            if (secondsInTimer > 9)
                drawnTimer = minuitesInTimer + ":" + secondsInTimer;
            if (secondsInTimer <= 9)
                drawnTimer = minuitesInTimer + ":0" + secondsInTimer;
            if (secondsInTimer < 0)
                drawnTimer = minuitesInTimer + ":00";

            // ----- < Timer Draw >
            Vector2 timerSize = _biggerMainFont.MeasureString(drawnTimer);
            _spriteBatch.DrawString(_biggerMainFont, drawnTimer, new Vector2(_screen.Center.X - (timerSize.X / 2), _screen.Top + 20), Color.Black);
            _spriteBatch.DrawString(_biggerMainFont, drawnTimer, new Vector2(_screen.Center.X - (timerSize.X / 2) + 5, _screen.Top + 25), Color.White);
            


            // < Money Draw >
            Vector2 moneyLength = _biggerMainFont.MeasureString("$" + _money);
            _spriteBatch.DrawString(_biggerMainFont, "$" + _money, new Vector2((_screen.Width - 30) - moneyLength.X, 30), Color.Black);
            _spriteBatch.DrawString(_biggerMainFont, "$" + _money, new Vector2((_screen.Width - 35) - moneyLength.X, 35), Color.White);



            // < Dynamic Task Draw >
            if (_level == 2)
            {
                int p = _numberOfPlayers;
                int[] currentTarget = new int[2];
                currentTarget[0] = (_level2Goal1[0] * p);
                currentTarget[1] = (_level2Goal1[1] * p);
                int starGoal = 1;



                if (_level2OreCollected[0] >= (_level2Goal1[0] * p) && _level2OreCollected[1] >= (_level2Goal1[1]))
                {
                    currentTarget[0] = (_level2Goal2[0] * p);
                    currentTarget[1] = (_level2Goal2[1] * p);
                    starGoal = 2;
                }
                if (_level2OreCollected[0] >= (_level2Goal2[0] * p) && _level2OreCollected[1] >= (_level2Goal2[1]))
                {
                    currentTarget[0] = (_level2Goal3[0] * p);
                    currentTarget[1] = (_level2Goal3[1] * p);
                    starGoal = 3;
                }



                _spriteBatch.DrawString(_mainFont, starGoal + " STAR\n-------\n|> Extract " + _level2OreCollected[0] + "/" + currentTarget[0] + " coal\n|> Extract " + _level2OreCollected[1] + "/" + currentTarget[1] + " copper", new Vector2(_screen.Left + 30, 300), Color.Black);
                _spriteBatch.DrawString(_mainFont, starGoal + " STAR\n-------\n|> Extract " + _level2OreCollected[0] + "/" + currentTarget[0] + " coal\n|> Extract " + _level2OreCollected[1] + "/" + currentTarget[1] + " copper", new Vector2(_screen.Left + 33, 303), Color.White);
            }
            if (_level == 3)
            {
                int currentTarget = (_level3Goal1 * _numberOfPlayers);
                int starGoal = 1;



                if (_level3OreCollected >= _level3Goal1 * _numberOfPlayers)
                {
                    currentTarget = (_level3Goal2 * _numberOfPlayers);
                    starGoal = 2;
                }
                if (_level3OreCollected >= _level3Goal2 * _numberOfPlayers)
                {
                    currentTarget = (_level3Goal3 * _numberOfPlayers);
                    starGoal = 3;
                }



                _spriteBatch.DrawString(_mainFont, starGoal + " STAR\n-------\n|> Find " + _level3OreCollected + "/" + currentTarget + " ore chunks", new Vector2(_screen.Left + 30, 300), Color.Black);
                _spriteBatch.DrawString(_mainFont, starGoal + " STAR\n-------\n|> Find " + _level3OreCollected + "/" + currentTarget + " ore chunks", new Vector2(_screen.Left + 33, 303), Color.White);
            }
            if (_level == 4)
            {
                int p = _numberOfPlayers;
                int[] currentTarget = new int[4];
                currentTarget[0] = (_level4Goal1[0] * p);
                currentTarget[1] = (_level4Goal1[1] * p);
                currentTarget[2] = (_level4Goal1[2] * p);
                currentTarget[3] = (_level4Goal1[3] * p);
                int starGoal = 1;



                if (_level4OreCollected[0] >= (_level4Goal1[0] * p) && _level4OreCollected[1] >= (_level4Goal1[1] * p) && _level4OreCollected[2] >= (_level4Goal1[2] * p) && _money >= (_level4Goal1[3] * p))
                {
                    currentTarget[0] = (_level4Goal2[0] * p);
                    currentTarget[1] = (_level4Goal2[1] * p);
                    currentTarget[2] = (_level4Goal2[2] * p);
                    currentTarget[3] = (_level4Goal2[3] * p);
                    starGoal = 2;
                }
                if (_level4OreCollected[0] >= (_level4Goal2[0] * p) && _level4OreCollected[1] >= (_level4Goal2[1] * p) && _level4OreCollected[2] >= (_level4Goal2[2] * p) && _money >= (_level4Goal2[3] * p))
                {
                    currentTarget[0] = (_level4Goal3[0] * p);
                    currentTarget[1] = (_level4Goal3[1] * p);
                    currentTarget[2] = (_level4Goal3[2] * p);
                    currentTarget[3] = (_level4Goal3[3] * p);
                    starGoal = 3;
                }



                _spriteBatch.DrawString(_mainFont, 
                    starGoal + " STAR" +
                    "\n-------\n" +
                    "|> Extract " + _level4OreCollected[0] + "/" + currentTarget[0] + " coal\n" +
                    "|> Extract " + _level4OreCollected[1] + "/" + currentTarget[1] + " copper\n" +
                    "|> Extract " + _level4OreCollected[2] + "/" + currentTarget[2] + " iron\n" +
                    "|> Have " + _money + "/" + currentTarget[3] + " money in the bank",
                    new Vector2(_screen.Left + 30, 300), Color.Black);

                _spriteBatch.DrawString(_mainFont, starGoal + " STAR" +
                    "\n-------\n" +
                    "|> Extract " + _level4OreCollected[0] + "/" + currentTarget[0] + " coal\n" +
                    "|> Extract " + _level4OreCollected[1] + "/" + currentTarget[1] + " copper\n" +
                    "|> Extract " + _level4OreCollected[2] + "/" + currentTarget[2] + " iron\n" +
                    "|> Have " + _money + "/" + currentTarget[3] + " money in the bank", 
                    new Vector2(_screen.Left + 33, 303), Color.White);
            }
            if (_level == 5)
            {
                int p = _numberOfPlayers;
                int[] currentTarget = new int[5];
                currentTarget[0] = (_level5Goal1[0] * p);
                currentTarget[1] = (_level5Goal1[1] * p);
                currentTarget[2] = (_level5Goal1[2] * p);
                currentTarget[3] = (_level5Goal1[3] * p);
                currentTarget[4] = (_level5Goal1[4] * p);
                int starGoal = 1;

                if (_level5OreCollected[0] >= (_level5Goal1[0] * p) &&
                    _level5OreCollected[1] >= (_level5Goal1[1] * p) &&
                    _level5OreCollected[2] >= (_level5Goal1[2] * p) &&
                    _level5OreCollected[3] >= (_level5Goal1[3] * p) &&
                    _money >= (_level5Goal1[4] * p))
                {
                    currentTarget[0] = (_level5Goal2[0] * p);
                    currentTarget[1] = (_level5Goal2[1] * p);
                    currentTarget[2] = (_level5Goal2[2] * p);
                    currentTarget[3] = (_level5Goal2[3] * p);
                    currentTarget[4] = (_level5Goal2[4] * p);
                    starGoal = 2;
                }
                if (_level5OreCollected[0] >= (_level5Goal2[0] * p) &&
                    _level5OreCollected[1] >= (_level5Goal2[1] * p) &&
                    _level5OreCollected[2] >= (_level5Goal2[2] * p) &&
                    _level5OreCollected[3] >= (_level5Goal2[3] * p) &&
                    _money >= (_level5Goal2[4] * p))
                {
                    currentTarget[0] = (_level5Goal3[0] * p);
                    currentTarget[1] = (_level5Goal3[1] * p);
                    currentTarget[2] = (_level5Goal3[2] * p);
                    currentTarget[3] = (_level5Goal3[3] * p);
                    currentTarget[4] = (_level5Goal3[4] * p);
                    starGoal = 3;
                }



                _spriteBatch.DrawString(_mainFont,
                    starGoal + " STAR" +
                    "\n-------\n" +
                    "|> Extract " + _level5OreCollected[0] + "/" + currentTarget[0] + " coal\n" +
                    "|> Extract " + _level5OreCollected[1] + "/" + currentTarget[1] + " copper\n" +
                    "|> Extract " + _level5OreCollected[2] + "/" + currentTarget[2] + " iron\n" +
                    "|> Extract " + _level5OreCollected[3] + "/" + currentTarget[3] + " gold\n" +
                    "|> Have " + _money + "/" + currentTarget[4] + " money in the bank",
                    new Vector2(_screen.Left + 30, 300), Color.Black);

                _spriteBatch.DrawString(_mainFont, starGoal + " STAR" +
                    "\n-------\n" +
                    "|> Extract " + _level5OreCollected[0] + "/" + currentTarget[0] + " coal\n" +
                    "|> Extract " + _level5OreCollected[1] + "/" + currentTarget[1] + " copper\n" +
                    "|> Extract " + _level5OreCollected[2] + "/" + currentTarget[2] + " iron\n" +
                    "|> Extract " + _level5OreCollected[3] + "/" + currentTarget[3] + " gold\n" +
                    "|> Have " + _money + "/" + currentTarget[4] + " money in the bank",
                    new Vector2(_screen.Left + 33, 303), Color.White);
            }


            if (_isGamePaused == true)
            {
                // < Screen Dimmer > 
                _spriteBatch.Draw(_overlay, _screen, Color.Black * 0.5f);

                Vector2 pausedLength = _pauseFont.MeasureString("Paused!");
                _spriteBatch.DrawString(_pauseFont, "Paused!", new Vector2(_screen.Center.X - (pausedLength.X / 2), _screen.Center.Y - (pausedLength.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_pauseFont, "Paused!", new Vector2(_screen.Center.X - (pausedLength.X / 2) + 5, _screen.Center.Y - (pausedLength.Y / 2) + 5), Color.White);

                if (_keyboardLastUsed == true)
                {
                    Vector2 exitLength = _mainFont.MeasureString("Press E to EXIT to HOME");
                    _spriteBatch.DrawString(_mainFont, "Press E to EXIT to HOME", new Vector2(_screen.Center.X - (exitLength.X / 2), _screen.Center.Y + 200 - (exitLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press E to EXIT to HOME", new Vector2(_screen.Center.X - (exitLength.X / 2) + 5, _screen.Center.Y + 200 - (exitLength.Y / 2) + 5), Color.White);

                    Vector2 resetLength = _mainFont.MeasureString("Press R to RESTART");
                    _spriteBatch.DrawString(_mainFont, "Press R to RESTART", new Vector2(_screen.Center.X - (resetLength.X / 2), _screen.Center.Y + 300 - (resetLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press R to RESTART", new Vector2(_screen.Center.X - (resetLength.X / 2) + 5, _screen.Center.Y + 300 - (resetLength.Y / 2) + 5), Color.White);

                    Vector2 controlsLength = _mainFont.MeasureString("Press F for CONTROL SCHEME");
                    _spriteBatch.DrawString(_mainFont, "Press F for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2), _screen.Center.Y + 400 - (controlsLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press F for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2) + 5, _screen.Center.Y + 400 - (controlsLength.Y / 2) + 5), Color.White);

                    if (_controlScreenUp)
                    {
                        _spriteBatch.Draw(_controlsTexture, _screen, Color.White);

                        Vector2 controlsReturn = _mainFont.MeasureString("Press F to RETURN");
                        _spriteBatch.DrawString(_mainFont, "Press F to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2), _screen.Center.Y + 450 - (controlsLength.Y / 2)), Color.Black);
                        _spriteBatch.DrawString(_mainFont, "Press F to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2) + 5, _screen.Center.Y + 450 - (controlsLength.Y / 2) + 5), Color.White);
                    }
                }
                if (_gamePadLastUsed == true)
                {
                    Vector2 exitLength = _mainFont.MeasureString("Press A to EXIT to HOME");
                    _spriteBatch.DrawString(_mainFont, "Press A to EXIT to HOME", new Vector2(_screen.Center.X - (exitLength.X / 2), _screen.Center.Y + 200 - (exitLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press A to EXIT to HOME", new Vector2(_screen.Center.X - (exitLength.X / 2) + 5, _screen.Center.Y + 200 - (exitLength.Y / 2) + 5), Color.White);

                    Vector2 resetLength = _mainFont.MeasureString("Press B to RESTART");
                    _spriteBatch.DrawString(_mainFont, "Press B to RESTART", new Vector2(_screen.Center.X - (resetLength.X / 2), _screen.Center.Y + 300 - (resetLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press B to RESTART", new Vector2(_screen.Center.X - (resetLength.X / 2) + 5, _screen.Center.Y + 300 - (resetLength.Y / 2) + 5), Color.White);

                    Vector2 controlsLength = _mainFont.MeasureString("Press Y for CONTROL SCHEME");
                    _spriteBatch.DrawString(_mainFont, "Press Y for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2), _screen.Center.Y + 400 - (controlsLength.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_mainFont, "Press Y for CONTROL SCHEME", new Vector2(_screen.Center.X - (controlsLength.X / 2) + 5, _screen.Center.Y + 400 - (controlsLength.Y / 2) + 5), Color.White);

                    if (_controlScreenUp)
                    {
                        _spriteBatch.Draw(_controlsTexture, _screen, Color.White);

                        Vector2 controlsReturn = _mainFont.MeasureString("Press Y to RETURN");
                        _spriteBatch.DrawString(_mainFont, "Press Y to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2), _screen.Center.Y + 450 - (controlsLength.Y / 2)), Color.Black);
                        _spriteBatch.DrawString(_mainFont, "Press Y to RETURN", new Vector2(_screen.Center.X - (controlsReturn.X / 2) + 5, _screen.Center.Y + 450 - (controlsLength.Y / 2) + 5), Color.White);
                    }
                }
            }
            #endregion
            _spriteBatch.End();
        }
    }



    enum gameState
    {
        Splash,
        Title,
        LevelSelect,
        Tutorial,
        Level
    }



    enum Items
    {
        Bare,
        StoneMultiTool,
        StonePickaxe,
        StoneShovel,
        IronMultiTool,
        IronPickaxe,
        IronShovel,
        GoldPickaxe,
        GoldShovel,
        Coal,
        Copper,
        Iron,
        Gold
    }
}