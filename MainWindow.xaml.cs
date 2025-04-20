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
    {
        // Variáveis de controle do jogo
        DispatcherTimer gameTimer = new DispatcherTimer();
        bool goLeft, goRight, goDown, goUp;
        bool noLeft, noRight, noDown, noUp;
        int speed = 7;
        short life = 3;
        Rect pacmanHitBox;
        int ghostSpeed = 4;
        int score = 0;
        private Random rand = new Random();
        int tickCounter;

        // Variáveis para controle dos modos dos fantasmas
        bool scatterMode = true;
        int modeTimer = 0;
        const int scatterDuration = 300;
        const int chaseDuration = 600;

        // Variáveis para controle das cerejas
        private Rect cherryHitBox;
        private int cherryCol, cherryRow;
        private const int tileSize = 32; // Tamanho de cada célula do grid

        public MainWindow()
        {
            InitializeComponent();
            GameSetUp();
        }

        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left && noLeft == false)
            {
                goRight = goUp = goDown = false;
                noRight = noUp = noDown = false;
                goLeft = true;
                pacman.RenderTransform = new RotateTransform(-180, pacman.Width / 2, pacman.Height / 2);
            }

            if (e.Key == Key.Right && noRight == false)
            {
                noLeft = noUp = noDown = false;
                goLeft = goUp = goDown = false;
                goRight = true;
                pacman.RenderTransform = new RotateTransform(0, pacman.Width / 2, pacman.Height / 2);
            }

            if (e.Key == Key.Up && noUp == false)
            {
                noRight = noDown = noLeft = false;
                goRight = goDown = goLeft = false;
                goUp = true;
                pacman.RenderTransform = new RotateTransform(-90, pacman.Width / 2, pacman.Height / 2);
            }

            if (e.Key == Key.Down && noDown == false)
            {
                noUp = noLeft = noRight = false;
                goUp = goLeft = goRight = false;
                goDown = true;
                pacman.RenderTransform = new RotateTransform(90, pacman.Width / 2, pacman.Height / 2);
            }
        }

        private void GameSetUp()
        {
            MyCanvas.Focus();
            gameTimer.Tick += GameLoop!;
            gameTimer.Interval = TimeSpan.FromMilliseconds(30);
            gameTimer.Start();

            // Configuração das imagens
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

            // Configuração das vidas
            ImageBrush lifeOneImage = new ImageBrush();
            lifeOneImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/pacman.jpg"));
            lifeOne.Fill = lifeOneImage;

            ImageBrush lifeTwoImage = new ImageBrush();
            lifeTwoImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/pacman.jpg"));
            lifeTwo.Fill = lifeTwoImage;

            ImageBrush lifeThreeImage = new ImageBrush();
            lifeThreeImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/pacman.jpg"));
            lifeThree.Fill = lifeThreeImage;

            // Configuração da cereja
            ImageBrush cherryImage = new ImageBrush();
            cherryImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/cereja.jpg"));
            cherry.Fill = cherryImage;
            cherry.Visibility = Visibility.Hidden;

            // Posição inicial aleatória para a cereja
            cherryCol = rand.Next(0, (int)(Application.Current.MainWindow.Width / tileSize));
            cherryRow = rand.Next(0, (int)(Application.Current.MainWindow.Height / tileSize));
        }

        private void GameLoop(object sender, EventArgs e)
        {
            tickCounter++;
            modeTimer++;

            // Alternar entre modos de dispersão e perseguição
            if (scatterMode && modeTimer > scatterDuration)
            {
                scatterMode = false;
                modeTimer = 0;
            }
            else if (!scatterMode && modeTimer > chaseDuration)
            {
                scatterMode = true;
                modeTimer = 0;
            }

            txtScore.Content = "Score: " + score;

            // Movimento do Pac-Man
            if (goRight) Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + speed);
            if (goLeft) Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - speed);
            if (goUp) Canvas.SetTop(pacman, Canvas.GetTop(pacman) - speed);
            if (goDown) Canvas.SetTop(pacman, Canvas.GetTop(pacman) + speed);

            // Limites da tela
            if (goDown && Canvas.GetTop(pacman) + 80 > Application.Current.MainWindow.Height)
                Canvas.SetTop(pacman, 0);
            if (goUp && Canvas.GetTop(pacman) < 1)
                Canvas.SetTop(pacman, Application.Current.MainWindow.Height);
            if (goLeft && Canvas.GetLeft(pacman) - 10 < 1)
                Canvas.SetLeft(pacman, Application.Current.MainWindow.Width);
            if (goRight && Canvas.GetLeft(pacman) + 70 > Application.Current.MainWindow.Width)
                Canvas.SetLeft(pacman, 0);

            pacmanHitBox = new Rect(Canvas.GetLeft(pacman), Canvas.GetTop(pacman), pacman.Width, pacman.Height);

            // Verificação de colisões
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                Rect hitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                // Colisão com paredes
                if ((string)x.Tag == "wall")
                {
                    if (goLeft && pacmanHitBox.IntersectsWith(hitBox))
                    {
                        Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + 10);
                        noLeft = true;
                        goLeft = false;
                    }
                    if (goRight && pacmanHitBox.IntersectsWith(hitBox))
                    {
                        Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - 10);
                        noRight = true;
                        goRight = false;
                    }
                    if (goDown && pacmanHitBox.IntersectsWith(hitBox))
                    {
                        Canvas.SetTop(pacman, Canvas.GetTop(pacman) - 10);
                        noDown = true;
                        goDown = false;
                    }
                    if (goUp && pacmanHitBox.IntersectsWith(hitBox))
                    {
                        Canvas.SetTop(pacman, Canvas.GetTop(pacman) + 10);
                        noUp = true;
                        goUp = false;
                    }
                }

                // Colisão com moedas
                if ((string)x.Tag == "coin" && pacmanHitBox.IntersectsWith(hitBox) && x.Visibility == Visibility.Visible)
                {
                    x.Visibility = Visibility.Hidden;
                    score++;
                }

                // Colisão com fantasmas
                if ((string)x.Tag == "ghost")
                {
                    if (pacmanHitBox.IntersectsWith(hitBox))
                    {
                        DeathLogic();
                    }

                    // Movimento dos fantasmas
                    if (x.Name == "redGuy") MoveBlinky((Rectangle)x);
                    else if (x.Name == "pinkGuy") MovePinky((Rectangle)x);
                    else if (x.Name == "orangeGuy") MoveInky((Rectangle)x);
                }

                // Lógica da cereja
                if (x.Name == "cherry")
                {
                    cherryHitBox = hitBox;

                    // Aparece após 250 ticks
                    if (tickCounter == 250 && x.Visibility != Visibility.Visible)
                    {
                        // TODO: checar se a cereja não tá sendo gerada dentro da parede
                        cherryCol = rand.Next(0, (int)(Application.Current.MainWindow.Width / tileSize));
                        cherryRow = rand.Next(0, (int)(Application.Current.MainWindow.Height / tileSize));

                        Canvas.SetLeft(x, cherryCol * tileSize);
                        Canvas.SetTop(x, cherryRow * tileSize);
                        x.Visibility = Visibility.Visible;
                    }

                    // Colisão com a cereja
                    if (pacmanHitBox.IntersectsWith(cherryHitBox) && x.Visibility == Visibility.Visible)
                    {
                        x.Visibility = Visibility.Hidden;
                        score += 15;
                        tickCounter = 0; // Reseta para reaparecer depois
                    }
                }
            }

            // Vitória ao coletar todas as moedas
            if (score == 85)
            {
                GameOver("Você ganhou, atingiu a pontuação!");
            }
        }

        // Métodos de movimento dos fantasmas (mantidos originais)
        private void MoveBlinky(Rectangle ghost)
        {
            double ghostX = Canvas.GetLeft(ghost);
            double ghostY = Canvas.GetTop(ghost);
            double targetX, targetY;

            if (scatterMode)
            {
                targetX = Application.Current.MainWindow.Width;
                targetY = 0;
            }
            else
            {
                targetX = Canvas.GetLeft(pacman);
                targetY = Canvas.GetTop(pacman);
            }

            MoveGhostTowardsTarget(ghost, ghostX, ghostY, targetX, targetY);
        }

        private void MovePinky(Rectangle ghost)
        {
            double ghostX = Canvas.GetLeft(ghost);
            double ghostY = Canvas.GetTop(ghost);
            double targetX, targetY;

            if (scatterMode)
            {
                targetX = 0;
                targetY = 0;
            }
            else
            {
                targetX = Canvas.GetLeft(pacman);
                targetY = Canvas.GetTop(pacman);

                if (goRight) targetX += 4 * 32;
                if (goLeft) targetX -= 4 * 32;
                if (goUp) targetY -= 4 * 32;
                if (goDown) targetY += 4 * 32;
            }

            MoveGhostTowardsTarget(ghost, ghostX, ghostY, targetX, targetY);
        }

        private void MoveInky(Rectangle ghost)
        {
            double ghostX = Canvas.GetLeft(ghost);
            double ghostY = Canvas.GetTop(ghost);
            double targetX, targetY;

            if (scatterMode)
            {
                targetX = Application.Current.MainWindow.Width;
                targetY = Application.Current.MainWindow.Height;
            }
            else
            {
                double pacmanX = Canvas.GetLeft(pacman);
                double pacmanY = Canvas.GetTop(pacman);
                double blinkyX = Canvas.GetLeft(redGuy);
                double blinkyY = Canvas.GetTop(redGuy);

                targetX = pacmanX + (pacmanX - blinkyX);
                targetY = pacmanY + (pacmanY - blinkyY);
            }

            MoveGhostTowardsTarget(ghost, ghostX, ghostY, targetX, targetY);
        }

        private void MoveGhostTowardsTarget(Rectangle ghost, double ghostX, double ghostY, double targetX, double targetY)
        {
            List<Point> possibleDirections = new List<Point>();
            possibleDirections.Add(new Point(ghostX + ghostSpeed, ghostY));
            possibleDirections.Add(new Point(ghostX - ghostSpeed, ghostY));
            possibleDirections.Add(new Point(ghostX, ghostY + ghostSpeed));
            possibleDirections.Add(new Point(ghostX, ghostY - ghostSpeed));

            Point bestDirection = possibleDirections[0];
            double minDistance = double.MaxValue;

            foreach (var direction in possibleDirections)
            {
                double distance = Math.Pow(targetX - direction.X, 2) + Math.Pow(targetY - direction.Y, 2);
                if (distance < minDistance && CanMoveTo(ghost, direction.X, direction.Y))
                {
                    minDistance = distance;
                    bestDirection = direction;
                }
            }

            Canvas.SetLeft(ghost, bestDirection.X);
            Canvas.SetTop(ghost, bestDirection.Y);
        }

        private bool CanMoveTo(Rectangle ghost, double x, double y)
        {
            Rect newPos = new Rect(x, y, ghost.Width, ghost.Height);
            Rect screenWidth = new Rect(0, 0, MyCanvas.ActualWidth, MyCanvas.ActualHeight);
            foreach (var wall in MyCanvas.Children.OfType<Rectangle>())
            {
                if ((string)wall.Tag == "wall")
                {
                    Rect wallRect = new Rect(Canvas.GetLeft(wall), Canvas.GetTop(wall), wall.Width, wall.Height);
                    if (newPos.IntersectsWith(wallRect) || willCollide(newPos))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool willCollide(Rect newPos)
        {
            double screenHeig = MyCanvas.ActualHeight;
            double screenWidth = MyCanvas.ActualWidth;
            //Testa se a próxima posição do fantasma acrescida de 95 pixels, será maior
            //que a borda da tela, se for, vai colidir.
            if (newPos.X + 95 >= screenWidth || 
                newPos.Y + 95 >= screenHeig)
                return true;
            return false;
        }

        private void DeathLogic()
        {
            if (life == 3)
            {
                lifeOne.Visibility = Visibility.Hidden;
                life--;
                ResetPacmanPosition();
            }
            else if (life == 2)
            {
                lifeTwo.Visibility = Visibility.Hidden;
                life--;
                ResetPacmanPosition();
            }
            else if (life == 1)
            {
                lifeThree.Visibility = Visibility.Hidden;
                life--;
                ResetPacmanPosition();
            }
            else
            {
                GameOver("O Fantasma te tocou, recomece! Aff!");
            }
        }

        private void ResetPacmanPosition()
        {
            Canvas.SetLeft(pacman, 417);
            Canvas.SetTop(pacman, 452);
            goRight = goDown = goUp = goLeft = false;
        }

        private void GameOver(string message)
        {
            gameTimer.Stop();
            MessageBox.Show(message, "Pacman da Giseli (Adaptado)");
            if (System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName) != null)
                Application.Current.Shutdown();
        }
    }
}