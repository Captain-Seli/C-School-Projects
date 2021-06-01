using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

// observable collections
using System.Collections.ObjectModel;

// debug output
using System.Diagnostics;

// timer, sleep
using System.Threading;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

// hi res timer
using HighPrecisionTimer;
using System.Timers;

// Rectangle
// Must update References manually
using System.Drawing;

// INotifyPropertyChanged
using System.ComponentModel;

namespace BouncingBall
{
    public partial class Model : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }
       // Timer Vars
       DispatcherTimer t;
       DateTime start;
       public void timerDisplay()
        {
            // Timer Display Code
            t = new DispatcherTimer(new TimeSpan(0, 0, 0), DispatcherPriority.Background,
            t_Tick, Dispatcher.CurrentDispatcher);
            t.IsEnabled = true;
            start = DateTime.Now;
        }
          
       // Function to display Timer
       private void t_Tick(object sender, EventArgs e)
         {
            // MoveBall = !MoveBall;
            // Have to subtract the current time from the start time to get accurate stopwatch time
             TimeElapsed = Convert.ToString(DateTime.Now - start);
         }

        private static UInt32 _numBalls = 1;
        private UInt32[] _buttonPresses = new UInt32[_numBalls];
        Random _randomNumber = new Random();

        // Begin Brick
        System.Windows.Media.Brush FillColorRed;
        System.Windows.Media.Brush FillColorBlack;
        public ObservableCollection<Brick> BrickCollection;
        int _numBricks = 48; // This changes the behavior of the bricks being broken
        int _numBrickRows = 4;
        int _numBrickColumns = 12;
        Rectangle[] _brickRectangles = new Rectangle[1];
        double _brickHeight = 50;
        double _brickWidth = 100;
        // End Brick
    
        private MultimediaTimer _ballHiResTimer;
        private MultimediaTimer _paddleHiResTimer;
        private double _ballXMove = 1;
        private double _ballYMove = 1;
        System.Drawing.Rectangle _ballRectangle;
        System.Drawing.Rectangle _paddleRectangle;
        bool _movepaddleLeft = false;
        bool _movepaddleRight = false;
        private bool _moveBall = false;
        public bool MoveBall
        {
            get { return _moveBall; }
            set { _moveBall = value; }
        }

        private double _windowHeight = 100;
        public double WindowHeight
        {
            get { return _windowHeight; }
            set { _windowHeight = value; }
        }

        private double _windowWidth = 100;
        public double WindowWidth
        {
            get { return _windowWidth; }
            set { _windowWidth = value; }
        }

        // Score Variable
        private int _score;
        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                OnPropertyChanged("Score");
            }
           
        }

        // Time variable to display stopwatch
        private string _timeElapsed ="0:00";
        public string TimeElapsed
        {
            get { return _timeElapsed; }
            set 
            {   _timeElapsed = value;
                OnPropertyChanged("TimeElapsed");
            }
        }

        /// <summary>
        /// Model constructor
        /// </summary>
        /// <returns></returns>
        public Model()
        {

            SolidColorBrush mySolidColorBrushRed = new SolidColorBrush();
            SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush();

            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.

            mySolidColorBrushRed.Color = System.Windows.Media.Color.FromRgb(255, 0, 0);
            FillColorRed = mySolidColorBrushRed;
            // Change the brick black to "disappear" when hit
            mySolidColorBrushBlack.Color = System.Windows.Media.Color.FromRgb(0, 0, 0);
            FillColorBlack = mySolidColorBrushBlack;
        }

        public void InitModel()
        {
            // create our multi-media timers
            _ballHiResTimer = new MultimediaTimer() { Interval = 4 };
            _ballHiResTimer.Elapsed += BallMMTimerCallback;
            _ballHiResTimer.Start();


            _paddleHiResTimer = new MultimediaTimer() { Interval = 3 };
            _paddleHiResTimer.Elapsed += PaddleMMTimerCallback;
            _paddleHiResTimer.Start();

            BrickCollection = new ObservableCollection<Brick>();
            // Creating brick collection and adding all the necessary bricks to each column and row
            for (int i = 0; i < _numBrickRows; i++)
            {
                for (int k = 0; k < _numBrickColumns; k++)
                {
                    BrickCollection.Add(new Brick()
                    {
                        BrickFill = FillColorRed,
                        BrickHeight = _brickHeight,
                        BrickWidth = _brickWidth,
                        BrickVisible = System.Windows.Visibility.Visible,
                        BrickName = (i* _numBrickColumns+k).ToString(),
                    });
                    BrickCollection[i * _numBrickColumns + k].BrickCanvasLeft = k * _brickWidth;
                    BrickCollection[i * _numBrickColumns + k].BrickCanvasTop = i * _brickHeight;
                }
            }
            UpdateRects();
        }

        // Reset the color of the bricks
        void ToggleBrickColorBack(String name)
        {
            int index = int.Parse(name);

            if (BrickCollection[index].BrickFill == FillColorBlack)
            {
                BrickCollection[index].BrickFill = FillColorRed;
                return;
            }
        }

        // Reset the entire game
        public void resetGame()
        {
            // Reset the Score
            Score = 0;
            Console.WriteLine("Reset Score: " + Score);
            for (int brick = 0; brick < _numBricks; brick++)
            {
                ToggleBrickColorBack(BrickCollection[brick].BrickName);
            }
        }
        public void CleanUp()
        {
            _ballHiResTimer.Stop();
            _paddleHiResTimer.Stop();
        }

        public void SetStartPosition()
        {
            
            ballHeight = 50;
            ballWidth = 50;
            paddleWidth = 120;
            paddleHeight = 10;

            // Set position of ball at start
            ballCanvasLeft = _windowWidth/2 - ballWidth/2;
            ballCanvasTop = _windowHeight/2;
           
            _moveBall = false;

            paddleCanvasLeft = _windowWidth / 2 - paddleWidth / 2;
            paddleCanvasTop = _windowHeight - paddleHeight;
            _paddleRectangle = new System.Drawing.Rectangle((int)paddleCanvasLeft, (int)paddleCanvasTop, (int)paddleWidth, (int)paddleHeight);

            UpdateRects();
        }

        private void UpdateRects()
        {
            _ballRectangle = new System.Drawing.Rectangle((int)ballCanvasLeft, (int)ballCanvasTop, (int)ballWidth, (int)ballHeight);
            for (int brick = 0; brick < _numBricks; brick++)
            {
                Console.WriteLine("Creating Brick #" + brick);
                BrickCollection[brick].BrickRectangle = new System.Drawing.Rectangle((int)BrickCollection[brick].BrickCanvasLeft,
                    (int)BrickCollection[brick].BrickCanvasTop, (int)BrickCollection[brick].BrickWidth, (int)BrickCollection[brick].BrickHeight);
            }
        
        }

        public void MoveLeft(bool move)
        {
            _movepaddleLeft = move;
        }

        public void MoveRight(bool move)
        {
            _movepaddleRight = move;
        }

        private void BallMMTimerCallback(object o, System.EventArgs e)
        {

            if (!_moveBall)
                return;

            ballCanvasLeft += _ballXMove;
            ballCanvasTop += _ballYMove;

            // check to see if ball has it the left or right side of the drawing element
            if ((ballCanvasLeft + ballWidth >= _windowWidth) ||
                (ballCanvasLeft <= 0))
                _ballXMove = -_ballXMove;


            // check to see if ball has it the top of the drawing element
            if ( ballCanvasTop <= 0) 
                _ballYMove = -_ballYMove;

            if (ballCanvasTop + ballWidth >= _windowHeight)
            {
                // we hit bottom. stop moving the ball
                _moveBall = false;

            }

            // see if we hit the paddle
            _ballRectangle = new System.Drawing.Rectangle((int)ballCanvasLeft, (int)ballCanvasTop, (int)ballWidth, (int)ballHeight);
            if (_ballRectangle.IntersectsWith(_paddleRectangle))
            {
                // hit paddle. reverse direction in Y direction
                _ballYMove = -_ballYMove;

                // move the ball away from the paddle so we don't intersect next time around and
                // get stick in a loop where the ball is bouncing repeatedly on the paddle
                ballCanvasTop += 2*_ballYMove;

                // add move the ball in X some small random value so that ball is not traveling in the same 
                // pattern
                ballCanvasLeft += _randomNumber.Next(5);
            }

            // Change color of bricks when they are hit
            void ToggleBrickColor(String name)
            {
                int index = int.Parse(name);

                if (BrickCollection[index].BrickFill == FillColorRed)
                {
                    BrickCollection[index].BrickFill = FillColorBlack;
                    return;
                }
            }

            // Loop to ensure the ball hits every brick in the collection
            for (int brick = 0; brick < _numBricks; brick++)
            {
                // If the brick isn't red, pass it over
                if (BrickCollection[brick].BrickFill != FillColorRed) continue;
                IntersectSide collisionSide = IntersectsAt(BrickCollection[brick].BrickRectangle, _ballRectangle);
                switch (collisionSide)
                {
                    case IntersectSide.NONE:
                        break;

                    case IntersectSide.TOP:
                        _ballYMove = -_ballYMove;
                        // Increment score when hit
                        Score = _score + 1;
                        // change color when hit
                        ToggleBrickColor(BrickCollection[brick].BrickName);
                        Console.WriteLine("Score: " + Score);
                        break;

                    case IntersectSide.BOTTOM:
                        _ballYMove = -_ballYMove;
                        Score = _score + 1;
                        ToggleBrickColor(BrickCollection[brick].BrickName);
                        Console.WriteLine("Score: " + Score);
                        break;

                    case IntersectSide.LEFT:
                        _ballXMove = -_ballXMove;
                        Score = _score + 1;
                        ToggleBrickColor(BrickCollection[brick].BrickName);
                        Console.WriteLine("Score: " + Score);
                        break;

                    case IntersectSide.RIGHT:
                        _ballXMove = -_ballXMove;
                        Score = _score + 1;
                        ToggleBrickColor(BrickCollection[brick].BrickName);
                        Console.WriteLine("Score: " + Score);
                        break;
                }
            }
            
        }

        enum IntersectSide { NONE, LEFT, RIGHT, TOP, BOTTOM };
        private IntersectSide IntersectsAt(Rectangle brick, Rectangle ball)
        {
            if (brick.IntersectsWith(ball) == false)
                return IntersectSide.NONE;

            Rectangle r = Rectangle.Intersect(brick, ball);

            // did we hit the top of the brick
            if (ball.Top + ball.Height - 1 == r.Top &&
                r.Height == 1)
                return IntersectSide.TOP;

            if (ball.Top == r.Top &&
                r.Height == 1)
                return IntersectSide.BOTTOM;

            if (ball.Left == r.Left &&
                r.Width == 1)
                return IntersectSide.RIGHT;

            if (ball.Left + ball.Width - 1 == r.Left &&
                r.Width == 1)
                return IntersectSide.LEFT;

            return IntersectSide.NONE;
        }

        private void PaddleMMTimerCallback(object o, System.EventArgs e)
        {
            if (_movepaddleLeft && paddleCanvasLeft > 0)
                paddleCanvasLeft -= 2;
            else if (_movepaddleRight && paddleCanvasLeft < _windowWidth - paddleWidth)
                paddleCanvasLeft += 2;
            
            _paddleRectangle = new System.Drawing.Rectangle((int)paddleCanvasLeft, (int)paddleCanvasTop, (int)paddleWidth, (int)paddleHeight);
        }



    }
}
