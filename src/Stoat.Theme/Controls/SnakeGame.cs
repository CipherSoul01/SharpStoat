using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Stoat.Theme.Controls;

public sealed class SnakeGame : Control
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<SnakeGame, IBrush?>(
            nameof(Background));

    public static readonly StyledProperty<IBrush?> CellColorProperty =
        AvaloniaProperty.Register<SnakeGame, IBrush?>(
            nameof(CellColor));

    public static readonly StyledProperty<IBrush?> FoodColorProperty =
        AvaloniaProperty.Register<SnakeGame, IBrush?>(
            nameof(FoodColor));

    public static readonly StyledProperty<IBrush?> SnakeColorProperty =
        AvaloniaProperty.Register<SnakeGame, IBrush?>(
            nameof(SnakeColor));

    public static readonly StyledProperty<IBrush?> SnakeHeadColorProperty =
        AvaloniaProperty.Register<SnakeGame, IBrush?>(
            nameof(SnakeHeadColor));

    public static readonly StyledProperty<IBrush?> OverlayColorProperty =
        AvaloniaProperty.Register<SnakeGame, IBrush?>(
            nameof(OverlayColor));

    public static readonly StyledProperty<IBrush?> TextColorProperty =
        AvaloniaProperty.Register<SnakeGame, IBrush?>(
            nameof(TextColor));

    public static readonly StyledProperty<IBrush?> SecondaryTextColorProperty =
        AvaloniaProperty.Register<SnakeGame, IBrush?>(
            nameof(SecondaryTextColor));

    public static readonly StyledProperty<bool> PlayerControlledProperty =
        AvaloniaProperty.Register<SnakeGame, bool>(
            nameof(PlayerControlled),
            defaultValue: false);

    public static readonly StyledProperty<HorizontalAlignment> HorizontalAlignmentContentProperty = AvaloniaProperty.Register<SnakeGame, HorizontalAlignment>(
        nameof(HorizontalAlignmentContent));

    public static readonly StyledProperty<VerticalAlignment> VerticalAlignmentContentProperty = AvaloniaProperty.Register<SnakeGame, VerticalAlignment>(
        nameof(VerticalAlignmentContent));

    public VerticalAlignment VerticalAlignmentContent
    {
        get => GetValue(VerticalAlignmentContentProperty);
        set => SetValue(VerticalAlignmentContentProperty, value);
    }
    
    public HorizontalAlignment HorizontalAlignmentContent
    {
        get => GetValue(HorizontalAlignmentContentProperty);
        set => SetValue(HorizontalAlignmentContentProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush? CellColor
    {
        get => GetValue(CellColorProperty);
        set => SetValue(CellColorProperty, value);
    }

    public IBrush? FoodColor
    {
        get => GetValue(FoodColorProperty);
        set => SetValue(FoodColorProperty, value);
    }

    public IBrush? SnakeColor
    {
        get => GetValue(SnakeColorProperty);
        set => SetValue(SnakeColorProperty, value);
    }

    public IBrush? SnakeHeadColor
    {
        get => GetValue(SnakeHeadColorProperty);
        set => SetValue(SnakeHeadColorProperty, value);
    }

    public IBrush? OverlayColor
    {
        get => GetValue(OverlayColorProperty);
        set => SetValue(OverlayColorProperty, value);
    }

    public IBrush? TextColor
    {
        get => GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public IBrush? SecondaryTextColor
    {
        get => GetValue(SecondaryTextColorProperty);
        set => SetValue(SecondaryTextColorProperty, value);
    }

    public bool PlayerControlled
    {
        get => GetValue(PlayerControlledProperty);
        set => SetValue(PlayerControlledProperty, value);
    }

    static SnakeGame()
    {
        AffectsRender<SnakeGame>(
            BackgroundProperty,
            CellColorProperty,
            FoodColorProperty,
            SnakeColorProperty,
            SnakeHeadColorProperty,
            OverlayColorProperty,
            TextColorProperty,
            SecondaryTextColorProperty,
            PlayerControlledProperty);
    }

    private const int Columns = 20;
    private const int Rows = 20;

    private readonly List<Point> _snake = [];
    private readonly DispatcherTimer _timer;

    private Point _food;

    private Direction _direction = Direction.Right;
    private Direction _nextDirection = Direction.Right;

    private bool _started;
    private bool _gameOver;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public SnakeGame()
    {
        Focusable = true;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(120)
        };

        _timer.Tick += (_, _) => UpdateGame();

        AttachedToVisualTree += (_, _) =>
        {
            Focus();

            if (!PlayerControlled)
            {
                StartGame();
            }
        };

        DetachedFromVisualTree += (_, _) =>
        {
            _timer.Stop();
        };

        ResetGame();
    }

    private void ResetGame()
    {
        _timer.Stop();

        _snake.Clear();

        _snake.Add(new Point(5, 10));
        _snake.Add(new Point(4, 10));
        _snake.Add(new Point(3, 10));

        _food = new Point(15, 10);

        _direction = Direction.Right;
        _nextDirection = Direction.Right;

        _started = false;
        _gameOver = false;

        InvalidateVisual();
    }

    private void StartGame()
    {
        if (_gameOver)
            return;

        _started = true;
        _timer.Start();

        InvalidateVisual();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!PlayerControlled)
            return;

        if (_gameOver)
        {
            ResetGame();
            StartGame();

            e.Handled = true;
            return;
        }

        if (!_started)
        {
            StartGame();

            e.Handled = true;
            return;
        }

        var direction = e.Key switch
        {
            Key.Up or Key.W => Direction.Up,
            Key.Down or Key.S => Direction.Down,
            Key.Left or Key.A => Direction.Left,
            Key.Right or Key.D => Direction.Right,
            _ => (Direction?)null
        };

        if (direction.HasValue)
        {
            SetDirection(direction.Value);
            e.Handled = true;
        }
    }

    private void SetDirection(Direction direction)
    {
        if (direction == Direction.Up &&
            _direction != Direction.Down)
        {
            _nextDirection = Direction.Up;
        }
        else if (direction == Direction.Down &&
                 _direction != Direction.Up)
        {
            _nextDirection = Direction.Down;
        }
        else if (direction == Direction.Left &&
                 _direction != Direction.Right)
        {
            _nextDirection = Direction.Left;
        }
        else if (direction == Direction.Right &&
                 _direction != Direction.Left)
        {
            _nextDirection = Direction.Right;
        }
    }

    private void UpdateGame()
    {
        if (!PlayerControlled)
        {
            UpdateAutomaticDirection();
        }

        _direction = _nextDirection;

        var head = _snake[0];

        var newHead = _direction switch
        {
            Direction.Up =>
                new Point(head.X, head.Y - 1),

            Direction.Down =>
                new Point(head.X, head.Y + 1),

            Direction.Left =>
                new Point(head.X - 1, head.Y),

            Direction.Right =>
                new Point(head.X + 1, head.Y),

            _ => head
        };

        if (newHead.X < 0 ||
            newHead.X >= Columns ||
            newHead.Y < 0 ||
            newHead.Y >= Rows ||
            _snake.Contains(newHead))
        {
            GameOver();
            return;
        }

        _snake.Insert(0, newHead);

        if (newHead == _food)
        {
            SpawnFood();
        }
        else
        {
            _snake.RemoveAt(_snake.Count - 1);
        }

        InvalidateVisual();
    }

    private void UpdateAutomaticDirection()
    {
        var possibleDirections = new[]
        {
            Direction.Up,
            Direction.Down,
            Direction.Left,
            Direction.Right
        };

        var validDirections = possibleDirections
            .Where(IsSafeDirection)
            .ToList();

        if (validDirections.Count == 0)
            return;

        var distances = validDirections
            .Select(direction => new
            {
                Direction = direction,
                Distance = GetFoodDistance(direction)
            })
            .ToList();

        var minDistance = distances.Min(x => x.Distance);

        var preferredDirection = distances
            .Where(x => x.Distance == minDistance)
            .ElementAt(Random.Shared.Next(
                distances.Count(x => x.Distance == minDistance)))
            .Direction;

        SetDirection(preferredDirection);
    }

    private bool IsSafeDirection(Direction direction)
    {
        var head = _snake[0];

        var next = direction switch
        {
            Direction.Up =>
                new Point(head.X, head.Y - 1),

            Direction.Down =>
                new Point(head.X, head.Y + 1),

            Direction.Left =>
                new Point(head.X - 1, head.Y),

            Direction.Right =>
                new Point(head.X + 1, head.Y),

            _ => head
        };
        

        return IsAllowedByPath(next, direction) && IsSafeIndex(next, direction);
    }

    private bool IsAllowedByPath(Point point, Direction direction)
    {
        return direction switch
        {
            Direction.Left  => point.Y % 2 != 0,
            Direction.Right => point.Y % 2 == 0,

            Direction.Down  => point.X % 2 != 0,
            Direction.Up    => point.X % 2 == 0,

            _ => false
        };
    }

    private bool IsSafeIndex(Point next, Direction direction)
    {
        if (next.X < 0 ||
            next.X >= Columns ||
            next.Y < 0 ||
            next.Y >= Rows)
        {
            return false;
        }

        if (_snake.Contains(next))
            return false;

        if (direction == Direction.Up &&
            _direction == Direction.Down)
        {
            return false;
        }

        if (direction == Direction.Down &&
            _direction == Direction.Up)
        {
            return false;
        }

        if (direction == Direction.Left &&
            _direction == Direction.Right)
        {
            return false;
        }

        if (direction == Direction.Right &&
            _direction == Direction.Left)
        {
            return false;
        }

        return true;
    }

    private int GetFoodDistance(Direction direction)
    {
        var head = _snake[0];

        var next = direction switch
        {
            Direction.Up =>
                new Point(head.X, head.Y - 1),

            Direction.Down =>
                new Point(head.X, head.Y + 1),

            Direction.Left =>
                new Point(head.X - 1, head.Y),

            Direction.Right =>
                new Point(head.X + 1, head.Y),

            _ => head
        };

        var dx = next.X - _food.X;
        var dy = next.Y - _food.Y;
            return (int)Math.Sqrt(
            dx * dx +
            dy * dy);
    }

    private void SpawnFood()
    {
        do
        {
            _food = new Point(
                Random.Shared.Next(Columns),
                Random.Shared.Next(Rows));

        } while (_snake.Contains(_food));
    }

    private void GameOver()
    {
        _timer.Stop();
        _gameOver = true;

        InvalidateVisual();

        if (!PlayerControlled)
        {
            ResetGame();
            StartGame();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds.Size;

        context.FillRectangle(
            Background,
            new Rect(bounds));

        var scale = Math.Min(
            bounds.Width / Columns,
            bounds.Height / Rows);

        if (scale <= 0)
            return;

        var boardWidth = Columns * scale;
        var boardHeight = Rows * scale;

        var offsetX = HorizontalAlignmentContent switch
        {
            HorizontalAlignment.Left => 0,
            HorizontalAlignment.Center => (bounds.Width - boardWidth) / 2,
            HorizontalAlignment.Right => bounds.Width - boardWidth,
            _ => 0
        };
        
        var offsetY = VerticalAlignmentContent switch
        {
            VerticalAlignment.Top => 0,
            VerticalAlignment.Center => (bounds.Height - boardHeight) / 2,
            VerticalAlignment.Bottom => bounds.Height - boardHeight,
            _ => 0
        };

        using (context.PushTransform(
                   Matrix.CreateTranslation(offsetX, offsetY)))
        {
            DrawBoard(context, scale);

            if (_gameOver)
            {
                DrawGameOver(context, scale);
            }
        }
    }

    private void DrawBoard(
        DrawingContext context,
        double cellSize)
    {
        var gap = Math.Max(1, cellSize * 0.18);
        var squareSize = cellSize - gap;

        for (var y = 0; y < Rows; y++)
        {
            for (var x = 0; x < Columns; x++)
            {
                var rect = new Rect(
                    x * cellSize + gap / 2,
                    y * cellSize + gap / 2,
                    squareSize,
                    squareSize);

                context.FillRectangle(
                    CellColor,
                    rect);
            }
        }

        for (var i = 0; i < _snake.Count; i++)
        {
            var segment = _snake[i];

            var rect = new Rect(
                segment.X * cellSize + gap / 2,
                segment.Y * cellSize + gap / 2,
                squareSize,
                squareSize);

            context.FillRectangle(
                i == 0
                    ? SnakeHeadColor
                    : SnakeColor,
                rect);
        }

        var foodRect = new Rect(
            _food.X * cellSize + gap / 2,
            _food.Y * cellSize + gap / 2,
            squareSize,
            squareSize);

        context.FillRectangle(
            FoodColor,
            foodRect);
    }

    private void DrawGameOver(
        DrawingContext context,
        double cellSize)
    {
        var boardWidth = Columns * cellSize;
        var boardHeight = Rows * cellSize;

        context.FillRectangle(
            OverlayColor,
            new Rect(
                0,
                0,
                boardWidth,
                boardHeight));
    }
}