using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Forms;

namespace WpfTetris
{
    public partial class MainWindow : Window
    {
        private const int fieldWidth = 10;
        private const int fieldHeight = 20;

        private OutlinedText gameOver;
        private LinearGradientBrush gameOverFill = new LinearGradientBrush(Color.FromRgb(255, 128, 64), Color.FromRgb(255, 64, 0), 90);
        private LinearGradientBrush gameOverStroke = new LinearGradientBrush(Color.FromRgb(32, 0, 0), Color.FromRgb(64, 0, 0), 90);

        private Image[][] field;
        private TetrisPiece currentPiece;
        private Image[][] currentPieceImages;
        private int currentPieceX;
        private int currentPieceY;
        private TetrisPiece nextPiece;
        private bool downKeyPressed = false;
        private TimeSpan fallSpeed;   // time taken for a piece to drop 1 block

        private double blockSize;
        private DispatcherTimer timer;

        public MainWindow()
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            InitializeComponent();

            field = Ut.NewArray<Image>(fieldWidth, fieldHeight);
            generateNextPiece();
            fallSpeed = TimeSpan.FromSeconds(1);
            timer = new DispatcherTimer { Interval = fallSpeed, IsEnabled = true };
            timer.Tick += tick;
        }

        private void generatePieceImages()
        {
            currentPieceImages = currentPiece.CreateImages();
            foreach (var col in currentPieceImages)
                foreach (var img in col)
                    if (img != null)
                        mainCanvas.Children.Add(img);
        }

        private void generateNextPiece()
        {
            if (nextPiece == null)
                nextPiece = TetrisPiece.GetRandomPiece();

            currentPiece = nextPiece;
            currentPieceX = fieldWidth / 2 - currentPiece.Width / 2;
            currentPieceY = -currentPiece.Height;
            generatePieceImages();
            nextPiece = TetrisPiece.GetRandomPiece();
        }

        private void tick(object _ = null, EventArgs __ = null)
        {
            // Is there enough free space to lower the current piece by one block?
            var canLower = true;
            for (int x = 0; x < currentPiece.Width; x++)
                for (int y = 0; y < currentPiece.Height; y++)
                    if (currentPiece.Shape[x][y] && !IsEmptySpace(currentPieceX + x, currentPieceY + y + 1))
                        canLower = false;

            if (canLower)
            {
                // There is enough empty space and we can just lower the piece.
                currentPieceY++;
            }
            else
            {
                // We cannot lower the current piece. There are a lot of things we need to do now:

                // 1. Lock the piece in place by adding it to the field array.
                for (int x = 0; x < currentPiece.Width; x++)
                    for (int y = 0; y < currentPiece.Height; y++)
                        if (currentPieceImages[x][y] != null)
                            field[currentPieceX + x][currentPieceY + y] = currentPieceImages[x][y];

                // 2. Check if we’ve created a full row.
                var fullRows = new List<int>();
                for (int y = 0; y < fieldHeight; y++)
                {
                    var fullRow = true;
                    for (int x = 0; fullRow && x < fieldWidth; x++)
                        if (field[x][y] == null)
                            fullRow = false;
                    if (fullRow)
                        fullRows.Add(y);
                }

                // 3. If there are any full rows, we need to eliminate them and move everything down.
                for (int i = 0; i < fullRows.Count; i++)
                {
                    for (int x = 0; x < fieldWidth; x++)
                        mainCanvas.Children.Remove(field[x][fullRows[i]]);
                    for (int y = fullRows[i]; y >= 0; y--)
                        for (int x = 0; x < fieldWidth; x++)
                            field[x][y] = y == 0 ? null : field[x][y - 1];
                }

                // 4. Create the next piece.
                generateNextPiece();
            }

            updateImages();
        }

        private void updateImages()
        {
            // The current piece
            for (int x = 0; x < currentPiece.Width; x++)
                for (int y = 0; y < currentPiece.Height; y++)
                    if (currentPieceImages[x][y] != null)
                        moveAndResize(currentPieceImages[x][y], Canvas.GetLeft(playingField) + (currentPieceX + x) * blockSize, Canvas.GetTop(playingField) + (currentPieceY + y) * blockSize, blockSize, blockSize);

            // All the existing images that make up the playing field
            for (int x = 0; x < fieldWidth; x++)
                for (int y = 0; y < fieldHeight; y++)
                    if (field[x][y] != null)
                        moveAndResize(field[x][y], Canvas.GetLeft(playingField) + x * blockSize, Canvas.GetTop(playingField) + y * blockSize, blockSize, blockSize);
        }

        private bool IsEmptySpace(int x, int y)
        {
            // There are walls on the left and right and a floor at the bottom
            if (x < 0 || x >= fieldWidth || y >= fieldHeight)
                return false;

            // There is empty space on the top
            if (y < 0)
                return true;

            return field[x][y] == null;
        }

        private void resize(object sender, SizeChangedEventArgs e)
        {
            // Resize/reposition the background image
            if (mainCanvas.ActualWidth > backgroundImage.Source.Width * mainCanvas.ActualHeight / backgroundImage.Source.Height)
            {
                double newActualWidth = mainCanvas.ActualWidth;
                double newActualHeight = newActualWidth * backgroundImage.Source.Height / backgroundImage.Source.Width;
                Canvas.SetLeft(backgroundImage, 0);
                Canvas.SetTop(backgroundImage, (mainCanvas.ActualHeight - newActualHeight) / 2);
                backgroundImage.Width = newActualWidth;
                backgroundImage.Height = newActualHeight;
            }
            else
            {
                double newActualHeight = mainCanvas.ActualHeight;
                double newActualWidth = newActualHeight * backgroundImage.Source.Width / backgroundImage.Source.Height;
                Canvas.SetLeft(backgroundImage, (mainCanvas.ActualWidth - newActualWidth) / 2);
                Canvas.SetTop(backgroundImage, 0);
                backgroundImage.Width = newActualWidth;
                backgroundImage.Height = newActualHeight;
            }

            // Determine the physical size of the playing field
            double blockSizeX = mainCanvas.ActualWidth / (fieldWidth + 2);
            double blockSizeY = mainCanvas.ActualHeight / (fieldHeight + 2);
            double gameWidth, gameHeight, leftMargin, topMargin;

            if (blockSizeX > blockSizeY)
            {
                blockSize = blockSizeY;
                gameWidth = blockSize * (fieldWidth + 2);
                gameHeight = mainCanvas.ActualHeight;
                leftMargin = (mainCanvas.ActualWidth - gameWidth) / 2;
                topMargin = 0;
            }
            else
            {
                blockSize = blockSizeX;
                gameWidth = mainCanvas.ActualWidth;
                gameHeight = blockSize * (fieldHeight + 2);
                topMargin = (mainCanvas.ActualHeight - gameHeight) / 2;
                leftMargin = 0;
            }

            moveAndResize(playingField, leftMargin + blockSize, topMargin + blockSize, blockSize * fieldWidth, blockSize * fieldHeight);
            updateImages();
        }

        private void moveAndResize(FrameworkElement elem, double x, double y, double width, double height)
        {
            elem.Width = width;
            elem.Height = height;
            Canvas.SetLeft(elem, x);
            Canvas.SetTop(elem, y);
        }

        private void moveAfter(UIElement moveWhat, UIElement after)
        {
            var ind = mainCanvas.Children.IndexOf(moveWhat);
            mainCanvas.Children.RemoveAt(ind);
            if (after == null)
                mainCanvas.Children.Add(moveWhat);
            else
                mainCanvas.Children.Insert(mainCanvas.Children.IndexOf(after) + 1, moveWhat);
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            bool canMove = true;
            switch (e.Key)
            {
                case Key.Down:
                    if (!downKeyPressed)
                    {
                        downKeyPressed = true;
                        timer.Interval = TimeSpan.FromMilliseconds(fallSpeed.TotalMilliseconds / 10);
                        tick();
                    }
                    break;

                case Key.Left:
                case Key.Right:
                    var dx = e.Key == Key.Left ? -1 : 1;
                    for (int x = 0; x < currentPiece.Width; x++)
                        for (int y = 0; y < currentPiece.Height; y++)
                            if (currentPiece.Shape[x][y] && !IsEmptySpace(currentPieceX + x + dx, currentPieceY + y))
                                canMove = false;
                    if (canMove)
                    {
                        currentPieceX += dx;
                        updateImages();
                    }
                    break;

                case Key.Up:
                    // Create the rotated piece and then check if there’s enough empty space in its location.
                    var newPiece = currentPiece.RotateClockwise();
                    var newPieceX = currentPieceX + currentPiece.Width / 2 - newPiece.Width / 2;
                    var newPieceY = currentPieceY + currentPiece.Height / 2 - newPiece.Height / 2;
                    for (int x = 0; x < newPiece.Width; x++)
                        for (int y = 0; y < newPiece.Height; y++)
                            if (newPiece.Shape[x][y] && !IsEmptySpace(newPieceX + x, newPieceY + y))
                                canMove = false;

                    if (canMove)
                    {
                        // Remove the images associated with the old piece.
                        foreach (var col in currentPieceImages)
                            foreach (var img in col)
                                if (img != null)
                                    mainCanvas.Children.Remove(img);

                        // Switch to the new piece.
                        currentPiece = newPiece;
                        currentPieceX = newPieceX;
                        currentPieceY = newPieceY;
                        generatePieceImages();
                        updateImages();
                    }
                    break;
            }
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                downKeyPressed = false;
                timer.Interval = fallSpeed;
            }
        }
    }
}
