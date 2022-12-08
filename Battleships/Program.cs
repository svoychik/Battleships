var board = new Board(10, 10);

board.SetShip(new Point(5, 5), Positioning.Horizontal, 1);
board.SetShip(new Point(0, 1), Positioning.Horizontal, 1); //not valid
board.SetShip(new Point(5, 4), Positioning.Vertical, 4); // not valid because it's already busy
board.SetShip(new Point(9, 3), Positioning.Horizontal, 3);

board.Shoot(new Point(8, 3));
board.Shoot(new Point(9, 3));
board.Shoot(new Point(7, 3));

board.Shoot(new Point(0, 1));

public record Point(int X, int Y)
{
    public int X { get; set; } = X;
    public int Y { get; set; } = Y;
}

public enum ShootingResult
{
    Hit,
    Sunk,
    Miss
}

public enum Positioning
{
    Vertical,
    Horizontal
}

public class Board
{
    //Key - any point that belongs to the ship. But a few points may be referencing the same Ship object
    private readonly Dictionary<Point, Ship> _occupiedPoint = new();

    public Board(int height, int width)
    {
        Height = height;
        Width = width;
    }

    private int Width { get; set; }
    private int Height { get; set; }

    public bool SetShip(Point topRight, Positioning positioning, int size)
    {
        if (PointOutOfTheBorder(topRight))
        {
            Console.WriteLine("Ship out of the game border");
            return false;
        }

        var possiblePointOfShip = CalculatePointsForTheShip(topRight, positioning, size).ToList();
        if (possiblePointOfShip.Any(p => FindShip(p) != null))
            return false;
        var newShip = new Ship(size);
        possiblePointOfShip.ForEach(x =>
        {
            _occupiedPoint.Add(x, newShip);
            Console.WriteLine($"The ship point was placed into the border {x}");
        });
        return true;
    }

    public ShootingResult Shoot(Point p)
    {
        if (PointOutOfTheBorder(p))
        {
            Console.WriteLine("Out of the border shoot " + p);
            return ShootingResult.Miss;
        }

        var shipInThePoint = FindShip(p);
        if (shipInThePoint == null)
        {
            Console.WriteLine("No ships were found for the " + p);
            return ShootingResult.Miss;
        }

        shipInThePoint.Hit();
        var result = shipInThePoint.IsSunk
            ? ShootingResult.Sunk
            : ShootingResult.Hit;
        Console.WriteLine($"The ship received a damaged. It's {result}");
        return result;
    }

    Ship? FindShip(Point p) => _occupiedPoint.TryGetValue(p, out var point)
        ? point
        : null;

    private IEnumerable<Point> CalculatePointsForTheShip(Point topRight, Positioning positioning, int size)
    {
        var currentPoint = topRight;
        yield return currentPoint;
        for (int i = 0; i < size - 1; i++)
        {
            currentPoint = positioning switch
            {
                Positioning.Horizontal => currentPoint with { X = currentPoint.X - 1 },
                Positioning.Vertical => currentPoint with { Y = currentPoint.Y - 1 },
                _ => throw new ArgumentOutOfRangeException(nameof(positioning), positioning, null)
            };
            yield return currentPoint;
        }
    }

    private bool PointOutOfTheBorder(Point p) =>
        p.X <= Width && p.X >= 1
                     && p.Y <= Height && p.Y >= 1;
}


public class Ship
{
    private Guid Id { get; set; }
    private int Size { get; set; }
    private int Health { get; set; }

    public bool IsSunk => Health <= 0;

    //If I decide to store a ship on a dish, might be useful to have relations Ship - Point 
    public Ship(int size)
    {
        Id = Guid.NewGuid();
        Size = size;
        Health = size;
    }

    public int Hit()
    {
        Health--;
        return Health;
    }
}