using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PAC_Man_Game_WPF_MOO_ICT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    /*gameTimer: Controla o loop do jogo, atualizando os movimentos e verificações de colisão.
      goLeft, goRight, goDown, goUp: Determinam para onde o Pac-Man está se movendo.
      noLeft, noRight, noDown, noUp: Impedem que o Pac-Man atravesse paredes.*/
    {
        DispatcherTimer gameTimer = new DispatcherTimer(); // create a new instance of the dispatcher timer called game timer
        bool goLeft, goRight, goDown, goUp; // 4 boolean created to move player in 4 direction
        bool noLeft, noRight, noDown, noUp; // 4 more boolean created to stop player moving in that direction
        int speed = 7; // Velocidade do Pac-Man.
        short life = 3; // Vida do fia da mãe
        Rect pacmanHitBox; //Área de colisão do Pac-Man.
        int ghostSpeed = 4; // Velocidade dos fantasmas.
        int ghostMoveStep = 160; // Distância máxima que os fantasmas podem se mover antes de mudar de direção.
        int currentGhostStep; // current movement limit for the ghosts
        int score = 0; // score keeping integer
        private Random rand = new Random();
        int tickCounter; // Server para saber quantas vezes a tela atualizou ( noção de tempo )
        private Dictionary<Rectangle, string> ghostDirections = new Dictionary<Rectangle, string>();

        public MainWindow()
        {
            InitializeComponent();
            GameSetUp(); // run the game set up function
        }
        /*Este evento captura o pressionamento de teclas e define a direção do Pac-Man.
        Se Left for pressionado e noLeft for false, o Pac-Man começa a se mover para a esquerda.
        goRight, goUp, goDown são desativados para que ele não se mova em múltiplas direções ao mesmo tempo.
        A imagem do Pac-Man é rotacionada para a esquerda (-180°).
        Isso se repete para as teclas Right, Up e Down, com ajustes na rotação.*/
        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            // this is the key down event
            if (e.Key == Key.Left && noLeft == false)
            {
                // if the left key is down and the boolean noLeft is set to false
                goRight = goUp = goDown = false; // set rest of the direction booleans to false
                noRight = noUp = noDown = false; // set rest of the restriction boolean to false
                goLeft = true; // set go left true
                pacman.RenderTransform = new RotateTransform(-180, pacman.Width / 2, pacman.Height / 2); // rotate the pac man image to face left
            }

            if (e.Key == Key.Right && noRight == false)
            {
                // if the right key pressed and no right boolean is false
                noLeft = noUp = noDown = false; // set rest of the direction boolean to false
                goLeft = goUp = goDown = false; // set rest of the restriction boolean to false
                goRight = true; // set go right to true
                pacman.RenderTransform = new RotateTransform(0, pacman.Width / 2, pacman.Height / 2); // rotate the pac man image to face right
            }

            if (e.Key == Key.Up && noUp == false)
            {
                // if the up key is pressed and no up is set to false
                noRight = noDown = noLeft = false; // set rest of the direction boolean to false
                goRight = goDown = goLeft = false; // set rest of the restriction boolean to 
                goUp = true; // set go up to true
                pacman.RenderTransform = new RotateTransform(-90, pacman.Width / 2, pacman.Height / 2); // rotate the pac man character to face up
            }

            if (e.Key == Key.Down && noDown == false)
            {
                // if the down key is press and the no down boolean is false
                noUp = noLeft = noRight = false; // set rest of the direction boolean to false
                goUp = goLeft = goRight = false; // set rest of the restriction boolean to false
                goDown = true; // set go down to true
                pacman.RenderTransform = new RotateTransform(90, pacman.Width / 2, pacman.Height / 2); // rotate the pac man character to face down
            }
        }
        private void GameSetUp()
        {
            // this function will run when the program loads
            MyCanvas.Focus(); // set my canvas as the main focus for the program
            gameTimer.Tick += GameLoop!; // link the game loop event to the time tick
            gameTimer.Interval = TimeSpan.FromMilliseconds(30); // set time to tick every 20 milliseconds
            gameTimer.Start(); // start the time
            currentGhostStep = ghostMoveStep; // Configura o movimento inicial dos fantasmas

            // below pac man and the ghosts images are being imported from the images folder and then we are assigning the image brush to the rectangles
            ImageBrush pacmanImage = new ImageBrush();
            pacmanImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/pacman.jpg"));
            pacman.Fill = pacmanImage;

            ImageBrush redGhost = new ImageBrush();
            redGhost.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/red.jpg"));
            redGuy.Fill = redGhost;

            ImageBrush orangeGhost = new ImageBrush();
            orangeGhost.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/orange.jpg"));
            orangeGuy.Fill = orangeGhost;

            ImageBrush pinkGhost = new ImageBrush();
            pinkGhost.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/pink.jpg"));
            pinkGuy.Fill = pinkGhost;

            // Isso aqui são as texturas das vidas;
            ImageBrush lifeOneImage = new ImageBrush();
            lifeOneImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/pacman.jpg"));
            lifeOne.Fill = lifeOneImage;

            ImageBrush lifeTwoImage = new ImageBrush();
            lifeTwoImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/pacman.jpg"));
            lifeTwo.Fill = lifeTwoImage;

            ImageBrush lifeThreeImage = new ImageBrush();
            lifeThreeImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/pacman.jpg"));
            lifeThree.Fill = lifeThreeImage;

        }
        private void GameLoop(object sender, EventArgs e)
        {
            tickCounter++;
            // this is the game loop event, this event will control all of the movements, outcome, collision and score for the game
            txtScore.Content = "Score: " + score; // show the scoreo to the txtscore label. 
            // start moving the character in the movement directions

            if (goRight)
            {
                // if go right boolean is true then move pac man to the right direction by adding the speed to the left 
                Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + speed);
            }
            if (goLeft)
            {
                // if go left boolean is then move pac man to the left direction by deducting the speed from the left
                Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - speed);
            }
            if (goUp)
            {
                // if go up boolean is true then deduct the speed integer from the top position of the pac man
                Canvas.SetTop(pacman, Canvas.GetTop(pacman) - speed);
            }
            if (goDown)
            {
                // if go down boolean is true then add speed integer value to the pac man top position
                Canvas.SetTop(pacman, Canvas.GetTop(pacman) + speed);
            }
            // end the movement 

            //Evita que Pac-Man saia da tela.
            if (goDown && Canvas.GetTop(pacman) + 80 > Application.Current.MainWindow.Height)
            {
                // if pac man is moving down the position of pac man is grater than the main window height then stop down movement
                //noDown = true;
                //goDown = false;
                Canvas.SetTop(pacman, 0);
            }
            if (goUp && Canvas.GetTop(pacman) < 1)
            {
                // is pac man is moving and position of pac man is less than 1 then stop up movement
                //noUp = true;
                //goUp = false;
                Canvas.SetTop(pacman, Application.Current.MainWindow.Height);
            }
            if (goLeft && Canvas.GetLeft(pacman) - 10 < 1)
            {
                // if pac man is moving left and pac man position is less than 1 then stop moving left
                //noLeft = true;
                //goLeft = false;
                Canvas.SetLeft(pacman, Application.Current.MainWindow.Height);
            }
            if (goRight && Canvas.GetLeft(pacman) + 70 > Application.Current.MainWindow.Width)
            {
                // if pac man is moving right and pac man position is greater than the main window then stop moving right
                //noRight = true;
                //goRight = false;
                Canvas.SetLeft(pacman, 0);
            }
            pacmanHitBox = new Rect(Canvas.GetLeft(pacman), Canvas.GetTop(pacman), pacman.Width, pacman.Height); // asssign the pac man hit box to the pac man rectangle

            // below is the main game loop that will scan through all of the rectangles available inside of the game
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                // loop through all of the rectangles inside of the game and identify them using the x variable


                Rect hitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height); // create a new rect called hit box for all of the available rectangles inside of the game

                // find the walls, if any of the rectangles inside of the game has the tag wall inside of it
                if ((string)x.Tag == "wall")
                {
                    // check if we are colliding with the wall while moving left if true then stop the pac man movement
                    if (goLeft == true && pacmanHitBox.IntersectsWith(hitBox))
                    {
                        Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + 10);
                        noLeft = true;
                        goLeft = false;
                    }
                    // check if we are colliding with the wall while moving right if true then stop the pac man movement
                    if (goRight == true && pacmanHitBox.IntersectsWith(hitBox))
                    {
                        Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - 10);
                        noRight = true;
                        goRight = false;
                    }
                    // check if we are colliding with the wall while moving down if true then stop the pac man movement
                    if (goDown == true && pacmanHitBox.IntersectsWith(hitBox))
                    {
                        Canvas.SetTop(pacman, Canvas.GetTop(pacman) - 10);
                        noDown = true;
                        goDown = false;
                    }
                    // check if we are colliding with the wall while moving up if true then stop the pac man movement
                    if (goUp == true && pacmanHitBox.IntersectsWith(hitBox))
                    {
                        Canvas.SetTop(pacman, Canvas.GetTop(pacman) + 10);
                        noUp = true;
                        goUp = false;
                    }

                }

                // check if the any of the rectangles has a coin tag inside of them
                if ((string)x.Tag == "coin")
                {
                    // if pac man collides with any of the coin and coin is still visible to the screen
                    if (pacmanHitBox.IntersectsWith(hitBox) && x.Visibility == Visibility.Visible)
                    {
                        // set the coin visiblity to hidden
                        x.Visibility = Visibility.Hidden;
                        // add 1 to the score
                        score++;
                    }

                }

                // if any rectangle has the tag ghost inside of it
                if ((string)x.Tag == "ghost")
                {
                    //MoveRandomGhost((Rectangle)x);
                    // check if pac man collides with the ghost 
                    if (pacmanHitBox.IntersectsWith(hitBox))
                    {
                        // Lógica para renascer o pacman quando tomar dano
                        DeathLogic();
                    }
                    // if there is a rectangle called orange guy in the game
                    if (x.Name.ToString() == "orangeGuy")
                    {
                        // move that rectangle to towards the left of the screen
                        Canvas.SetLeft(x, Canvas.GetLeft(x) - ghostSpeed);
                    }
                    else if (x.Name.ToString() == "redGuy")
                    {
                        // move that rectangle to towards the left of the screen
                        Canvas.SetLeft(x, Canvas.GetLeft(x) + ghostSpeed);
                    }
                    else if (x.Name.ToString() == "pinkGuy")
                    {
                        // move that rectangle to towards the left of the screen
                        Canvas.SetLeft(x, Canvas.GetLeft(x) + ghostSpeed);
                    }

                    // reduce one from the current ghost step integer
                    currentGhostStep--;

                    // if the current ghost step integer goes below 1 
                    if (currentGhostStep < 1)
                    {
                        // reset the current ghost step to the ghost move step value
                        currentGhostStep = ghostMoveStep;
                        // reverse the ghost speed integer
                        ghostSpeed = -ghostSpeed;
                    }
                }
                if (tickCounter == 300) 
                {
                    // Fazer nascer uma cereja!
                    
                }
            }


            // if the player collected 85 coins in the game
            if (score == 85)
            {
                // show game over function with the you win message
                GameOver("Você ganhou, atingiu a pontuação!");
            }


        }

        private void DeathLogic() 
        {
            if (life == 3)
            {
                lifeOne.Visibility = Visibility.Hidden;
                life--;
                Canvas.SetLeft(pacman, 417);
                Canvas.SetTop(pacman, 452);
                goRight = false;
                goDown = false;
                goUp = false;
                goLeft = false;
            }
            else if (life == 2)
            {
                lifeTwo.Visibility = Visibility.Hidden;
                life--;
                Canvas.SetLeft(pacman, 417);
                Canvas.SetTop(pacman, 452);
                goRight = false;
                goDown = false;
                goUp = false;
                goLeft = false;
            }
            else if (life == 1)
            {
                lifeThree.Visibility = Visibility.Hidden;
                life--;
                Canvas.SetLeft(pacman, 417);
                Canvas.SetTop(pacman, 452);
                goRight = false;
                goDown = false;
                goUp = false;
                goLeft = false;
            }
            else
            {
                GameOver("O Fantasma te tocou, recomece! Aff!");
            }
        }
        private void MoveRandomGhost(Rectangle ghost)
        {
            double ghostX = Canvas.GetLeft(ghost);
            double ghostY = Canvas.GetTop(ghost);
            double newGhostX = ghostX;
            double newGhostY = ghostY;

            // Se o fantasma ainda não tem uma direção, escolhemos uma aleatoriamente
            if (!ghostDirections.ContainsKey(ghost))
            {
                ghostDirections[ghost] = GetRandomDirection();
            }

            // Define o movimento com base na direção atual do fantasma
            switch (ghostDirections[ghost])
            {
                case "up":
                    newGhostY -= ghostSpeed;
                    break;
                case "down":
                    newGhostY += ghostSpeed;
                    break;
                case "left":
                    newGhostX -= ghostSpeed;
                    break;
                case "right":
                    newGhostX += ghostSpeed;
                    break;
            }

            // Criamos a hitbox para verificar colisão antes de mover
            Rect newGhostHitBox = new Rect(newGhostX, newGhostY, ghost.Width, ghost.Height);
            foreach (var wall in MyCanvas.Children.OfType<Rectangle>())
            {
                if ((string)wall.Tag == "wall")
                {
                    Rect wallHitBox = new Rect(Canvas.GetLeft(wall), Canvas.GetTop(wall), wall.Width, wall.Height);

                    // Se o fantasma colidir com uma parede, escolhemos outra direção aleatoriamente
                    if (newGhostHitBox.IntersectsWith(wallHitBox))
                    {
                        ghostDirections[ghost] = GetRandomDirection();
                        return;
                    }
                }
            }

            // Se não houver colisão, move o fantasma
            Canvas.SetLeft(ghost, newGhostX);
            Canvas.SetTop(ghost, newGhostY);
        }
        // Método auxiliar para obter uma direção aleatória
        private string GetRandomDirection()
        {
            string[] directions = { "up", "down", "left", "right" };
            return directions[rand.Next(directions.Length)];
        }
        private void GameOver(string message)
        {
            // inside the game over function we passing in a string to show the final message to the game
            gameTimer.Stop(); // stop the game timer
            MessageBox.Show(message, "Pacman da Giseli (Adaptado)"); // show a mesage box with the message that is passed in this function

            // when the player clicks ok on the message box
            // restart the application
            //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            //Application.Current.Shutdown();
            System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName);
            Application.Current.Shutdown();
        }

    }
}