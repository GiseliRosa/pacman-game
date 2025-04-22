using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Classe principal que representa a janela do jogo PAC-MAN
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variáveis do Jogo

        // Temporizador principal do jogo
        private readonly DispatcherTimer _gameTimer = new DispatcherTimer();

        // Controles de movimento do PAC-MAN
        private bool _goLeft, _goRight, _goDown, _goUp;
        private bool _noLeft, _noRight, _noDown, _noUp;

        // Configurações gerais
        private const int Speed = 7;          // Velocidade do PAC-MAN
        private short _life = 3;              // Vidas restantes
        private const int GhostSpeed = 4;     // Velocidade dos fantasmas
        private int _score;                   // Pontuação atual
        private readonly Random _rand = new Random();
        private int _tickCounter;             // Contador de ticks para eventos temporizados

        // Hitbox do PAC-MAN
        private Rect _pacmanHitBox;

        // Variáveis dos fantasmas
        private bool _scatterMode = true;     // Modo dispersão/perseguição
        private int _modeTimer;               // Contador para alternar modos
        private const int ScatterDuration = 300; // Duração do modo dispersão
        private const int ChaseDuration = 600;   // Duração do modo perseguição

        // Variáveis da cereja
        private Rect _cherryHitBox;
        private const int TileSize = 32;      // Tamanho de cada célula do grid
        private readonly List<Point> _validCherryPositions = new List<Point>(); // Posições válidas

        #endregion

        #region Inicialização

        /// <summary>
        /// Construtor da janela principal - Configura a janela e inicializa o jogo
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            GameSetUp();
        }

        /// <summary>
        /// Configura o estado inicial do jogo
        /// - Define o foco para o canvas
        /// - Configura o timer do jogo
        /// - Carrega os elementos visuais
        /// </summary>
        private void GameSetUp()
        {
            MyCanvas.Focus(); // Permite capturar eventos de teclado
            _gameTimer.Tick += GameLoop; // Associa o método GameLoop ao timer
            _gameTimer.Interval = TimeSpan.FromMilliseconds(30); // ~33 FPS
            _gameTimer.Start(); // Inicia o loop do jogo

            LoadCharacterImages(); // Carrega as imagens dos personagens
            InitializeCherry();
            SpawnCherryInFixedPosition();
        }

        /// <summary>
        /// Carrega as imagens dos personagens
        /// </summary>
        private void LoadCharacterImages()
        {
            // PAC-MAN
            pacman.Fill = CreateImageBrush("pacman.jpg");

            // Fantasmas
            redGuy.Fill = CreateImageBrush("red.jpg");
            orangeGuy.Fill = CreateImageBrush("orange.jpg");
            pinkGuy.Fill = CreateImageBrush("pink.jpg");

            // Vidas
            lifeOne.Fill = CreateImageBrush("pacman.jpg");
            lifeTwo.Fill = CreateImageBrush("pacman.jpg");
            lifeThree.Fill = CreateImageBrush("pacman.jpg");
        }

        /// <summary>
        /// Cria um pincel de imagem a partir de um arquivo
        /// </summary>
        private ImageBrush CreateImageBrush(string imageName)
        {
            return new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri($"pack://application:,,,/images/{imageName}"))
            };
        }

        #endregion

        #region Lógica Principal do Jogo

        /// <summary>
        /// Loop principal do jogo, executado a cada frame
        /// </summary>
        private void GameLoop(object sender, EventArgs e)
        {
            UpdateGameState();
            HandlePacmanMovement();
            HandleCherryLogic();
            CheckCollisions();
            CheckWinCondition();
        }

        /// <summary>
        /// Atualiza o estado geral do jogo
        /// </summary>
        private void UpdateGameState()
        {
            _tickCounter++;
            _modeTimer++;

            // Alterna entre modos dos fantasmas
            if (_scatterMode && _modeTimer > ScatterDuration)
            {
                _scatterMode = false;
                _modeTimer = 0;
            }
            else if (!_scatterMode && _modeTimer > ChaseDuration)
            {
                _scatterMode = true;
                _modeTimer = 0;
            }

            txtScore.Content = $"Score: {_score}";
        }

        /// <summary>
        /// Controla o movimento do PAC-MAN
        /// </summary>
        private void HandlePacmanMovement()
        {
            if (_goRight) Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + Speed);
            if (_goLeft) Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - Speed);
            if (_goUp) Canvas.SetTop(pacman, Canvas.GetTop(pacman) - Speed);
            if (_goDown) Canvas.SetTop(pacman, Canvas.GetTop(pacman) + Speed);

            HandleScreenWrapping();
            _pacmanHitBox = new Rect(Canvas.GetLeft(pacman), Canvas.GetTop(pacman), pacman.Width, pacman.Height);
        }

        /// <summary>
        /// Implementa o efeito de "túnel" nas bordas da tela
        /// </summary>
        private void HandleScreenWrapping()
        {
            if (_goDown && Canvas.GetTop(pacman) + 80 > Application.Current.MainWindow.Height)
                Canvas.SetTop(pacman, 0);
            if (_goUp && Canvas.GetTop(pacman) < 1)
                Canvas.SetTop(pacman, Application.Current.MainWindow.Height);
            if (_goLeft && Canvas.GetLeft(pacman) - 10 < 1)
                Canvas.SetLeft(pacman, Application.Current.MainWindow.Width);
            if (_goRight && Canvas.GetLeft(pacman) + 70 > Application.Current.MainWindow.Width)
                Canvas.SetLeft(pacman, 0);
        }

        #endregion

        #region Lógica da Cereja

        /// <summary>
        /// Controla o aparecimento e coleta da cereja
        /// 250 Ciclos (aproximadamente 7,5 seg) = Frames = Execuções do GameLoop

        /*Quando o PAC-MAN encosta na cereja, três coisas acontecem simultaneamente: 
        a cereja desaparece da tela,
        o jogador ganha 15 pontos extras (um bônus considerável em relação às moedas comuns)
        e o contador de tempo é reiniciado, preparando o sistema para fazer a cereja reaparecer após novo intervalo.*/
        /// </summary>
        /// <summary>
        /// Controla o aparecimento e coleta da cereja
        /// </summary>
        private void HandleCherryLogic()
        {
            // Lógica de aparecimento baseada em tempo
            if (cherry.Visibility == Visibility.Hidden && _tickCounter >= 250)
            {
                SpawnCherryInFixedPosition();
                _tickCounter = 0; // é zerado para reiniciar a contagem.
            }

            // Lógica de coleta
            if (cherry.Visibility == Visibility.Visible &&
                _pacmanHitBox.IntersectsWith(new Rect(Canvas.GetLeft(cherry), Canvas.GetTop(cherry),
                                            cherry.Width, cherry.Height)))
            {
                cherry.Visibility = Visibility.Hidden;
                _score += 15;
                _tickCounter = 0;
            }
        }
        /// <summary>
        /// Configura a cereja e suas posições válidas
        /// </summary>

        /* Objetivo: Garante que a cereja sempre surja em locais acessíveis, 
          longe de paredes e espalhados pelo mapa para equilibrar a jogabilidade.*/
        private void InitializeCherry()
        {
            cherry.Fill = CreateImageBrush("cereja.jpg");
            cherry.Visibility = Visibility.Hidden;

            // Posições ajustadas com margem de segurança (X, Y)
            _validCherryPositions.Clear();

            // ÁREAS SEGURAS
            // Região Superior (Y entre 100-140)
            _validCherryPositions.Add(new Point(187, 277));  // Esquerda
            _validCherryPositions.Add(new Point(600, 120));  // Direita

            // Região Central (Y entre 200-320)
            _validCherryPositions.Add(new Point(350, 250));  // Centro-esquerda
            _validCherryPositions.Add(new Point(450, 210));  // Centro-direita

            // Região Inferior (Y entre 400-520)
            _validCherryPositions.Add(new Point(105, 487));  // Esquerda
            _validCherryPositions.Add(new Point(768, 477));
        }

        /// <summary>
        /// Faz a cereja aparecer em uma posição fixa
        /// </summary>
        private void SpawnCherryInFixedPosition()
        {
            if (_validCherryPositions.Count == 0) return;

            // Seleciona uma posição aleatória da lista de posições válidas
            var randomPosition = _validCherryPositions[_rand.Next(_validCherryPositions.Count)];

            Canvas.SetLeft(cherry, randomPosition.X);
            Canvas.SetTop(cherry, randomPosition.Y);
            cherry.Visibility = Visibility.Visible;
        }

        #endregion

        #region Colisões

        /// <summary>
        /// Verifica todas as colisões do jogo
        /// </summary>
        private void CheckCollisions()
        {
            foreach (var element in MyCanvas.Children.OfType<Rectangle>())
            {
                Rect hitBox = new Rect(Canvas.GetLeft(element), Canvas.GetTop(element), element.Width, element.Height);

                switch (element.Tag)
                {
                    case "wall":
                        HandleWallCollision(element, hitBox);
                        break;
                    case "coin":
                        HandleCoinCollision(element, hitBox);
                        break;
                    case "ghost":
                        HandleGhostCollision(element, hitBox);
                        break;
                }
            }
        }

        /// <summary>
        /// Trata colisões com paredes
        /// </summary>
        private void HandleWallCollision(Rectangle wall, Rect hitBox)
        {
            if (_goLeft && _pacmanHitBox.IntersectsWith(hitBox))
            {
                Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + 10);
                _noLeft = true;
                _goLeft = false;
            }
            if (_goRight && _pacmanHitBox.IntersectsWith(hitBox))
            {
                Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - 10);
                _noRight = true;
                _goRight = false;
            }
            if (_goDown && _pacmanHitBox.IntersectsWith(hitBox))
            {
                Canvas.SetTop(pacman, Canvas.GetTop(pacman) - 10);
                _noDown = true;
                _goDown = false;
            }
            if (_goUp && _pacmanHitBox.IntersectsWith(hitBox))
            {
                Canvas.SetTop(pacman, Canvas.GetTop(pacman) + 10);
                _noUp = true;
                _goUp = false;
            }
        }

        /// <summary>
        /// Trata colisões com moedas
        /// </summary>
        private void HandleCoinCollision(Rectangle coin, Rect hitBox)
        {
            if (_pacmanHitBox.IntersectsWith(hitBox) && coin.Visibility == Visibility.Visible)
            {
                coin.Visibility = Visibility.Hidden;
                _score++;
            }
        }

        /// <summary>
        /// Trata colisões com fantasmas
        /// </summary>
        private void HandleGhostCollision(Rectangle ghost, Rect hitBox)
        {
            if (_pacmanHitBox.IntersectsWith(hitBox))
            {
                DeathLogic();
            }

            // Movimentação dos fantasmas
            if (ghost.Name == "redGuy") MoveBlinky(ghost);
            else if (ghost.Name == "pinkGuy") MovePinky(ghost);
            else if (ghost.Name == "orangeGuy") MoveInky(ghost);
        }

        #endregion

        #region Movimentação dos Fantasmas
        /*Blinky persegue diretamente.
        Pinky tenta antecipar seu movimento.
        Inky age como um caçador em equipe.
        O scatter mode dá respiros estratégicos.*/
        private void MoveBlinky(Rectangle ghost)
        {
            double ghostX = Canvas.GetLeft(ghost);
            double ghostY = Canvas.GetTop(ghost);
            double targetX, targetY;

            if (_scatterMode)
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
            //Onde eu estou agora?" (ghostX/Y)
            //Para onde vou olhar depois?"(targetX / Y)

            double ghostX = Canvas.GetLeft(ghost);
            double ghostY = Canvas.GetTop(ghost);
            double targetX, targetY;

            if (_scatterMode)
            {
                targetX = 0;
                targetY = 0;
            }
            else
            {
                targetX = Canvas.GetLeft(pacman);
                targetY = Canvas.GetTop(pacman);

                if (_goRight) targetX += 4 * 32; // 128 pixels à frente do Pac-Man, sendo 4 = Número de blocos e 32 o Tamanho em pixels de cada bloco
                if (_goLeft) targetX -= 4 * 32;
                if (_goUp) targetY -= 4 * 32;
                if (_goDown) targetY += 4 * 32;
            }

            MoveGhostTowardsTarget(ghost, ghostX, ghostY, targetX, targetY); // faz a chamada do método passando os parâmetros
        }

        private void MoveInky(Rectangle ghost)
        {
            double ghostX = Canvas.GetLeft(ghost);
            double ghostY = Canvas.GetTop(ghost);
            double targetX, targetY;

            if (_scatterMode)
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
            var possibleDirections = new List<Point>
            {
                new Point(ghostX + GhostSpeed, ghostY),
                new Point(ghostX - GhostSpeed, ghostY),
                new Point(ghostX, ghostY + GhostSpeed),
                new Point(ghostX, ghostY - GhostSpeed)
            };

            Point bestDirection = possibleDirections[0];
            double minDistance = double.MaxValue;

            foreach (var direction in possibleDirections)
            {
                var distance = Math.Pow(targetX - direction.X, 2) + Math.Pow(targetY - direction.Y, 2);
                if (distance < minDistance && CanMoveTo(ghost, direction.X, direction.Y))
                {
                    minDistance = distance;
                    bestDirection = direction;
                }
            }

            Canvas.SetLeft(ghost, bestDirection.X);
            Canvas.SetTop(ghost, bestDirection.Y);
        }

        /*O método CanMoveTo é um verificador de colisão essencial para o movimento dos fantasmas. 
         Ele determina se um fantasma pode se mover para uma determinada posição (x, y) sem colidir com paredes ou sair dos limites do mapa. */
        private bool CanMoveTo(Rectangle ghost, double x, double y) // 
        {
            Rect newPos = new Rect(x, y, ghost.Width, ghost.Height);

            foreach (var wall in MyCanvas.Children.OfType<Rectangle>())
            {
                if ((string)wall.Tag == "wall")
                {
                    Rect wallRect = new Rect(Canvas.GetLeft(wall), Canvas.GetTop(wall), wall.Width, wall.Height);
                    if (newPos.IntersectsWith(wallRect) || WillCollide(newPos))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool WillCollide(Rect newPos)
        {
            return newPos.X + 95 >= MyCanvas.ActualWidth ||
                   newPos.Y + 95 >= MyCanvas.ActualHeight;
        }

        #endregion

        #region Controles e Lógica de Fim de Jogo

        /// <summary>
        /// Manipula os eventos de teclado
        /// </summary>
        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left when !_noLeft:
                    ResetMovement();
                    _goLeft = true;
                    pacman.RenderTransform = new RotateTransform(-180, pacman.Width / 2, pacman.Height / 2);
                    break;

                case Key.Right when !_noRight:
                    ResetMovement();
                    _goRight = true;
                    pacman.RenderTransform = new RotateTransform(0, pacman.Width / 2, pacman.Height / 2);
                    break;

                case Key.Up when !_noUp:
                    ResetMovement();
                    _goUp = true;
                    pacman.RenderTransform = new RotateTransform(-90, pacman.Width / 2, pacman.Height / 2);
                    break;

                case Key.Down when !_noDown:
                    ResetMovement();
                    _goDown = true;
                    pacman.RenderTransform = new RotateTransform(90, pacman.Width / 2, pacman.Height / 2);
                    break;
            }
        }

        /// <summary>
        /// Reseta todos os estados de movimento
        /*Essencialmente, esse método coloca o PAC-MAN em um estado neutro, como se estivesse parado e pronto para receber novos comandos do jogador.
        Ele é crucial em situações como quando o personagem morre e precisa ser reposicionado, quando o jogo é reiniciado, 
        ou em qualquer momento que exija uma parada completa e imediata do movimento.*/
        /// </summary>
        private void ResetMovement()
        {
            _goRight = _goDown = _goUp = _goLeft = false;
            _noLeft = _noRight = _noDown = _noUp = false;
        }

        /// <summary>
        /// Lógica de quando o PAC-MAN é pego por um fantasma
        /// </summary>
        private void DeathLogic()
        {
            _life--;

            switch (_life)
            {
                case 2:
                    lifeOne.Visibility = Visibility.Hidden; //esconde a imagem que representa uma vida do jogador, mas mantem o o espaço que ela ocupava.
                    break;
                case 1:
                    lifeTwo.Visibility = Visibility.Hidden;
                    break;
                case 0:
                    lifeThree.Visibility = Visibility.Hidden;
                    break;
            }

            if (_life < 0)
            {
                GameOver("O Fantasma te tocou, recomece! Aff!");
            }
            else
            {
                ResetPacmanPosition();
            }
        }

        /// <summary>
        /// Reseta a posição do PAC-MAN
/* O método ResetPacmanPosition() tem uma função clara: reposicionar o PAC-MAN em seu local inicial no mapa quando necessário. Ele faz isso de forma direta, definindo as coordenadas fixas (X=417, Y=452) que representam a posição de spawn segura do personagem,
 * normalmente no centro ou em uma área protegida do labirinto. Além disso, ele chama ResetMovement() para garantir que o PAC-MAN pare imediatamente 
qualquer movimento que estivesse fazendo antes do reset, liberando também possíveis bloqueios de direção causados por colisões.
Esse método é acionado principalmente em duas situações:
- Quando o PAC-MAN é pego por um fantasma (perdendo uma vida)
- Ao reiniciar o jogo ou fase*/
/// </summary>
private void ResetPacmanPosition()
{
    Canvas.SetLeft(pacman, 417);
    Canvas.SetTop(pacman, 452);
    ResetMovement();
}

/// <summary>
/// Verifica se o jogador venceu
/// </summary>
private void CheckWinCondition()
{
    if (_score >= 85)
    {
        GameOver("Você ganhou, atingiu a pontuação!");
    }
}

/// <summary>
/// Exibe a tela de fim de jogo
/// </summary>
private void GameOver(string message)
{
    _gameTimer.Stop();
    MessageBox.Show(message, "Pacman da Giseli (Adaptado)");

    // Reinicia o jogo
    if (System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName) != null)
    {
        Application.Current.Shutdown();
    }
}

#endregion
}
}