using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfTetris
{
    public abstract class TetrisPiece
    {
        public abstract bool[][] Shape { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract BitmapSource BitmapSource { get; }

        public static TetrisPiece[] Pieces = { new IPiece(), new TPiece(), new OPiece(), new LPiece(), new JPiece(), new SPiece(), new ZPiece() };
        public static TetrisPiece GetRandomPiece() { return Pieces[App.Rnd.Next(Pieces.Length)]; }

        public TetrisPiece RotateClockwise()
        {
            return new RotatedPiece
            {
                RotatedShape = Ut.NewArray<bool>(Height, Width, (x, y) => Shape[y][Height - 1 - x]),
                RotatedWidth = Height,
                RotatedHeight = Width,
                Bmp = BitmapSource
            };
        }
        
        private sealed class RotatedPiece : TetrisPiece
        {
            public bool[][] RotatedShape;
            public int RotatedWidth;
            public int RotatedHeight;
            public BitmapSource Bmp;
            public override bool[][] Shape { get { return RotatedShape; } }
            public override int Width { get { return RotatedWidth; } }
            public override int Height { get { return RotatedHeight; } }
            public override BitmapSource BitmapSource { get { return Bmp; } }
        }

        public Image[][] CreateImages()
        {
            return Ut.NewArray(Width, Height, (x, y) => Shape[x][y] ? new Image { Source = BitmapSource } : null);
        }

        private sealed class IPiece : TetrisPiece
        {
            public override bool[][] Shape { get { return new[] { new[] { true, true, true, true } }; } }
            public override int Width { get { return 1; } }
            public override int Height { get { return 4; } }
            public override BitmapSource BitmapSource { get { return new BitmapImage(new Uri("pack://application:,,,/Resources/blockI.png", UriKind.Absolute)); } }
        }
        private sealed class TPiece : TetrisPiece
        {
            public override bool[][] Shape { get { return new[] { new[] { true, false }, new[] { true, true }, new[] { true, false } }; } }
            public override int Width { get { return 3; } }
            public override int Height { get { return 2; } }
            public override BitmapSource BitmapSource { get { return new BitmapImage(new Uri("pack://application:,,,/Resources/blockT.png", UriKind.Absolute)); } }
        }
        private sealed class OPiece : TetrisPiece
        {
            public override bool[][] Shape { get { return new[] { new[] { true, true }, new[] { true, true } }; } }
            public override int Width { get { return 2; } }
            public override int Height { get { return 2; } }
            public override BitmapSource BitmapSource { get { return new BitmapImage(new Uri("pack://application:,,,/Resources/blockO.png", UriKind.Absolute)); } }
        }
        private sealed class LPiece : TetrisPiece
        {
            public override bool[][] Shape { get { return new[] { new[] { true, true, true }, new[] { false, false, true } }; } }
            public override int Width { get { return 2; } }
            public override int Height { get { return 3; } }
            public override BitmapSource BitmapSource { get { return new BitmapImage(new Uri("pack://application:,,,/Resources/blockL.png", UriKind.Absolute)); } }
        }
        private sealed class JPiece : TetrisPiece
        {
            public override bool[][] Shape { get { return new[] { new[] { false, false, true }, new[] { true, true, true } }; } }
            public override int Width { get { return 2; } }
            public override int Height { get { return 3; } }
            public override BitmapSource BitmapSource { get { return new BitmapImage(new Uri("pack://application:,,,/Resources/blockJ.png", UriKind.Absolute)); } }
        }
        private sealed class SPiece : TetrisPiece
        {
            public override bool[][] Shape { get { return new[] { new[] { false, true }, new[] { true, true }, new[] { true, false } }; } }
            public override int Width { get { return 3; } }
            public override int Height { get { return 2; } }
            public override BitmapSource BitmapSource { get { return new BitmapImage(new Uri("pack://application:,,,/Resources/blockS.png", UriKind.Absolute)); } }
        }
        private sealed class ZPiece : TetrisPiece
        {
            public override bool[][] Shape { get { return new[] { new[] { true, false }, new[] { true, true }, new[] { false, true } }; } }
            public override int Width { get { return 3; } }
            public override int Height { get { return 2; } }
            public override BitmapSource BitmapSource { get { return new BitmapImage(new Uri("pack://application:,,,/Resources/blockZ.png", UriKind.Absolute)); } }
        }
    }
}
