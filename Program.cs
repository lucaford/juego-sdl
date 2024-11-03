using System;
using System.Diagnostics.Eventing.Reader;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using Tao.Sdl;

namespace MyGame
{
    public struct Square
    {
        public int x1, y1, x2, y2;

        public Square(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
    }

    public struct Player
    {
        public float posX, posY;
        public int lives;
        public float velocity;
        public int coins;
        public string direction;

        public Player(float posX, float posY, int lives, float velocity, string direction)
        {
            this.posX = posX;
            this.posY = posY;
            this.lives = lives;
            this.velocity = velocity;
            this.coins = 0;
            this.direction = direction;
        }
    }

    class Program
    {
        static Image backgroundImage;
        static Image playerLeftImage;
        static Image playerRightImage;
        static Image enemyRightImage;
        static Image enemyLeftImage;
        static Image coinImage;
        static Image bitcoinHudImage;
        static Image gameMenuImage;
        static Image loserMenuImage;
        static Image winnerMenuImage;
        static Image heartIcon;
        static Image playerRoom;

        static SoundPlayer menuBackgroundMusic;
        static SoundPlayer gameplayBackgroundMusic;
        static SoundPlayer loseBackgroundMusic;
        static SoundPlayer winBackgroundMusic;

        static SoundPlayer coinSound;
        static SoundPlayer hurtSound;

        static Random randomObj;
        static Font font;
        static Square garden;
        static Square house;
        static Square houseDoor;
        static Square playerInsideHouse;
        static Player player;

        static int currentTime;
        static int windowWidth;
        static int windowHeight;
        static int lastDamageTime;
        static int cooldownTime;
        static float[] enemyPosX;
        static float[] enemyPosY;
        static float[] enemyLeftPosX;
        static float[] enemyLeftPosY;
        static float amountOfEnemies;
        static float amountOfLeftEnemies;
        static float coinPosY;
        static float coinPosX;
        static float enemyVelocity;
        static string gameStep;
        static bool isMusicPlaying;
        static int mouseX, mouseY;

        static void InitGameObjects()
        {
            backgroundImage = Engine.LoadImage("assets/fondo.png");
            playerLeftImage = Engine.LoadImage("assets/character-left.png");
            playerRightImage = Engine.LoadImage("assets/character-right.png");
            enemyRightImage = Engine.LoadImage("assets/enemy-right.png");
            enemyLeftImage = Engine.LoadImage("assets/enemy-left.png");
            coinImage = Engine.LoadImage("assets/bitcoin.png");
            bitcoinHudImage = Engine.LoadImage("assets/bitcoin-hud.png");
            gameMenuImage = Engine.LoadImage("assets/game-menu.png");
            loserMenuImage = Engine.LoadImage("assets/loser-menu.png");
            winnerMenuImage = Engine.LoadImage("assets/winner-menu.png");
            heartIcon = Engine.LoadImage("assets/heart-red.png");
            playerRoom = Engine.LoadImage("assets/player-room.png");

            menuBackgroundMusic = new SoundPlayer("assets/music/menu-bg-music.wav");
            gameplayBackgroundMusic = new SoundPlayer("assets/music/gameplay-bg-music.wav");
            loseBackgroundMusic = new SoundPlayer("assets/music/lose-bg-music.wav");
            winBackgroundMusic = new SoundPlayer("assets/music/win-bg-music.wav");

            coinSound = new SoundPlayer("assets/sounds/coin.wav");
            hurtSound = new SoundPlayer("assets/sounds/hurt.wav");

            randomObj = new Random();

            house = new Square(586, 67, 888, 286);
            houseDoor = new Square(428, 419, 509, 464);
            playerInsideHouse = new Square(351, 197, 670, 473);
            garden = new Square(5, 9, 1016, 758);

            player = new Player(600, 316, 3, 10, "LEFT");

            font = Engine.LoadFont("assets/fonts/BebasNeue-Regular.ttf", 28);

            currentTime = 0;
            windowWidth = 960;
            windowHeight = 720;
            lastDamageTime = 0;
            cooldownTime = 1000;

            enemyPosX = new float[5];
            enemyPosY = new float[5];
            enemyLeftPosX = new float[3];
            enemyLeftPosY = new float[3];
            amountOfEnemies = 1;
            amountOfLeftEnemies = 0;
            coinPosY = 400;
            coinPosX = 400;
            enemyVelocity = 2;
            gameStep = "MENU";
            isMusicPlaying = false;
            mouseX = 0;
            mouseY = 0;
        }

        static void Main(string[] args)
        {
            Engine.Initialize();
            InitGameObjects();

            enemyPosY[0] = randomObj.Next(0, 768);
            enemyPosX[0] = -1;

            enemyLeftPosY[0] = randomObj.Next(0, 768);
            enemyLeftPosX[0] = 1024;

            while (true)
            {
                CheckInputs();
                Update();
                Render();
                Sdl.SDL_Delay(20);
            }
        }

        static void ResetGame()
        {
            player = new Player(600, 316, 3, 10, "LEFT");
            player.coins = 0;

            coinPosX = 400;
            coinPosY = 400;

            amountOfEnemies = 1;
            amountOfLeftEnemies = 0;
            enemyVelocity = 2;

            for (int i = 0; i < enemyPosX.Length; i++)
            {
                enemyPosX[i] = -1;
                enemyPosY[i] = randomObj.Next(0, 768);
            }

            for (int i = 0; i < enemyLeftPosX.Length; i++)
            {
                enemyLeftPosX[i] = 1024;
                enemyLeftPosY[i] = randomObj.Next(0, 768);
            }

            menuBackgroundMusic.Stop();
            gameplayBackgroundMusic.Stop();
            loseBackgroundMusic.Stop();
            winBackgroundMusic.Stop();

            isMusicPlaying = false;
        }

        static bool IsMouseClickInsideSquare(int x1, int x2, int y1, int y2)
        {
            return mouseX >= x1 && mouseX <= x2 && mouseY >= y1 && mouseY <= y2;
        }

        static void CheckInputs()
        {
            if (Engine.MouseClick(Engine.MOUSE_LEFT, out mouseX, out mouseY))
            {
                Console.WriteLine(mouseX);
                Console.WriteLine(mouseY);
                if (gameStep == "LOSER_MENU")
                {
                    if (IsMouseClickInsideSquare(600, 962, 620, 716))
                    {
                        gameStep = "PLAYER_ROOM";
                        ResetGame();
                    }
                    if (IsMouseClickInsideSquare(141, 504, 620, 716))
                    {
                        gameStep = "MENU";
                    }
                }
            }
            if (Engine.KeyPress(Engine.KEY_LEFT))
            {
                player.posX -= player.velocity;
                player.direction = "LEFT";
            }

            if (Engine.KeyPress(Engine.KEY_RIGHT))
            {
                player.posX += player.velocity;
                player.direction = "RIGHT";
            }

            if (Engine.KeyPress(Engine.KEY_DOWN))
            {
                player.posY += player.velocity;
            }

            if (Engine.KeyPress(Engine.KEY_UP))
            {
                player.posY -= player.velocity;
            }

            if (Engine.KeyPress(Engine.KEY_ESP))
            {
                menuBackgroundMusic.Stop();
                isMusicPlaying = false;
                gameStep = "PLAYER_ROOM";
                ResetGame();
            }
            if (Engine.KeyPress(Engine.KEY_ESC))
            {
                Environment.Exit(0);
            }
        }

        static bool IsCooldownTimeOver()
        {
            return (currentTime - lastDamageTime) >= cooldownTime;
        }

        static void OnLoseLife()
        {
            player.lives--;
            hurtSound.Play();
            if (player.lives < 1)
            {
                gameStep = "LOSER_MENU";
                isMusicPlaying = false;
            }
            lastDamageTime = currentTime;
        }

        static void StopPlayerForEnteringSquare(float x1, float y1, float x2, float y2)
        {
            int playerWidth = 26;
            int playerHeight = 26;

            if (player.posX < x2 && player.posX + playerWidth > x1 && player.posY < y2 && player.posY + playerHeight > y1)
            {
                if (player.posX + playerWidth > x1 && Engine.KeyPress(Engine.KEY_RIGHT))
                {
                    player.posX = x1 - playerWidth;
                }

                if (player.posX < x2 && Engine.KeyPress(Engine.KEY_LEFT))
                {
                    player.posX = x2;
                }

                if (player.posY + playerHeight > y1 && Engine.KeyPress(Engine.KEY_DOWN))
                {
                    player.posY = y1 - playerHeight;
                }

                if (player.posY < y2 && Engine.KeyPress(Engine.KEY_UP))
                {
                    player.posY = y2;
                }
            }
        }
        static void RestrictPlayerMovement(Square square)
        {
            if (player.posX < square.x1)
            {
                player.posX = square.x1;
            }
            else if (player.posX > square.x2)
            {
                player.posX = square.x2;
            }

            if (player.posY < square.y1)
            {
                player.posY = square.y1;
            }
            else if (player.posY > square.y2)
            {
                player.posY = square.y2;
            }
        }

        static bool IsObjectInsideSquare(Square square, float posX, float posY)
        {
            return posX > square.x1 && posX < square.x2 && posY > square.y1 && posY < square.y2;
        }

        static void GenerateCoin()
        {
            bool isCoinInsideHouse;

            do
            {
                coinPosX = randomObj.Next(5, 973);
                coinPosY = randomObj.Next(185, 718);

                isCoinInsideHouse = IsObjectInsideSquare(house, coinPosX, coinPosY);

            } while (isCoinInsideHouse);
        }

        static void Update()
        {

            currentTime = Sdl.SDL_GetTicks();
            int distanceBetweenPlayerAndCoin = 30;

            switch (gameStep)
            {
                case "MENU":
                    if (!isMusicPlaying)
                    {
                        menuBackgroundMusic.PlayLooping();
                        isMusicPlaying = true;
                    }
                    break;
                case "LOSER_MENU":
                    if (!isMusicPlaying)
                    {
                        loseBackgroundMusic.PlayLooping();
                        isMusicPlaying = true;
                    }
                    break;
                case "WINNER_MENU":
                    if (!isMusicPlaying)
                    {
                        winBackgroundMusic.PlayLooping();
                        isMusicPlaying = true;
                    }
                    break;
                case "PLAYER_ROOM":
                    RestrictPlayerMovement(playerInsideHouse);
                        winBackgroundMusic.Stop();
                        if(IsObjectInsideSquare(houseDoor, player.posX, player.posY))
                    {
                        gameStep = "GAMEPLAY";
                        player.posX = 766;
                            player.posY =322 ;
                    }
                    break;
                case "GAMEPLAY":

                    StopPlayerForEnteringSquare(house.x1, house.y1, house.x2, house.y2);

                    RestrictPlayerMovement(garden);
                    break;
            }

            if (Math.Abs(coinPosX - player.posX) < distanceBetweenPlayerAndCoin && Math.Abs(coinPosY - player.posY) < distanceBetweenPlayerAndCoin)
            {
                player.coins++;
                coinSound.Play();
                if (enemyVelocity < 11)
                {
                    enemyVelocity += 1;
                }

                if (player.coins >= 3 && amountOfEnemies == 1) amountOfEnemies++;
                else if (player.coins >= 6 && amountOfEnemies == 2) amountOfEnemies++;
                else if (player.coins >= 9 && amountOfEnemies == 3) amountOfEnemies++;
                else if (player.coins >= 12 && amountOfEnemies == 4) amountOfEnemies++;

                if (player.coins >= 20 && amountOfLeftEnemies == 0) amountOfLeftEnemies++;
                if (player.coins >= 30 && amountOfLeftEnemies == 1) amountOfLeftEnemies++;
                if (player.coins >= 40 && amountOfLeftEnemies == 2) amountOfLeftEnemies++;

                GenerateCoin();
            }

            for (int i = 0; i < amountOfEnemies; i++)
            {
                if (enemyPosX[i] < 1024)
                {
                    enemyPosX[i] += enemyVelocity;
                }
                else
                {
                    enemyPosX[i] = -1;
                    enemyPosY[i] = randomObj.Next(0, 768);
                }
                Engine.Draw(enemyRightImage, enemyPosX[i], enemyPosY[i]);

                if (Math.Abs(enemyPosX[i] - player.posX) < distanceBetweenPlayerAndCoin && Math.Abs(enemyPosY[i] - player.posY) < distanceBetweenPlayerAndCoin && IsCooldownTimeOver())
                {
                    OnLoseLife();
                }
            }

            for (int i = 0; i < amountOfLeftEnemies; i++)
            {
                if (enemyLeftPosX[i] > 0)
                {
                    enemyLeftPosX[i] -= enemyVelocity;
                }
                else
                {
                    enemyLeftPosX[i] = 1024;
                    enemyLeftPosY[i] = randomObj.Next(0, 768);
                }

                if (Math.Abs(enemyLeftPosX[i] - player.posX) < distanceBetweenPlayerAndCoin && Math.Abs(enemyLeftPosY[i] - player.posY) < distanceBetweenPlayerAndCoin && IsCooldownTimeOver())
                {
                    OnLoseLife();
                }
            }
        }

        static void DrawPlayer()
        {
            if(player.direction == "RIGHT")
            {

                Engine.Draw(playerRightImage, player.posX, player.posY);
            } else
            {

                Engine.Draw(playerLeftImage, player.posX, player.posY);
            }
        }

        static void Render()
        {
            Engine.Clear();
            switch (gameStep)
            {
                case "MENU":
                    Engine.Draw(gameMenuImage, 0, 0);
                    break;
                case "LOSER_MENU":
                    Engine.Draw(loserMenuImage, 0, 0);

                    Font loserCoinsText = Engine.LoadFont("assets/fonts/BebasNeue-Regular.ttf", 46);
                    Engine.DrawText(player.coins.ToString(), 464, 388, 255, 255, 255, loserCoinsText);
                    break;
                case "WINNER_MENU":
                    Engine.Draw(winnerMenuImage, 0, 0);
                    break;
                case "PLAYER_ROOM":
                    Engine.Draw(playerRoom, 0, 0);

                    DrawPlayer();
                    break;
                case "GAMEPLAY":
                    Engine.Draw(backgroundImage, 0, 0);

                    DrawPlayer();
                    Engine.Draw(coinImage, coinPosX, coinPosY);

                    Engine.DrawText(player.coins.ToString(), 110, 145, 255, 255, 255, font);
                    Engine.Draw(bitcoinHudImage, 65, 140);

                    for (int i = player.lives; i > 0; i--)
                    {
                        Engine.Draw(heartIcon, (20 + (i * 45)), 100);
                    }

                    for (int i = 0; i < amountOfEnemies; i++)
                    {
                        Engine.Draw(enemyRightImage, enemyPosX[i], enemyPosY[i]);
                    }

                    for (int i = 0; i < amountOfLeftEnemies; i++)
                    {
                        Engine.Draw(enemyLeftImage, enemyLeftPosX[i], enemyLeftPosY[i]);
                    }
                    break;
            }
            Engine.Show();
        }
    }
}
