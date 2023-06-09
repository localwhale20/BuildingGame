using System.Linq.Expressions;

namespace BuildingGame.GuiElements;

public class Control
{
    public string Name { get; set; }
    public Rectangle Area { get; set; }
    public List<Control> Children { get; set; } = new();
    public event Action? Clicked;
    public event Action? ClientUpdate;
    public bool Active { get; set; }
    public int ZIndex { get; set; }
    public string? Tooltip { get; set; }
    private bool _pressed = false;
    private bool _oldPressed = false;

    public Control(string name)
    {
        Name = name;
        Active = true;
    }

    public virtual void Update()
    {
        _oldPressed = _pressed;
        _pressed = IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);

        if (_oldPressed && !_pressed && IsMouseHovered())
        {
            Clicked?.Invoke();
        }

        foreach (var child in Children)
        {
            child.Active = Active;
            if (child.ZIndex < ZIndex)
                child.ZIndex = ZIndex + 1;
        }

        ClientUpdate?.Invoke();
    }

    public virtual void Draw()
    {

    }

    public bool IsMouseHovered()
    {
        return CheckCollisionPointRec(GetMousePosition(), Area) && Active;
    }

    public void Adapt(Func<Vector2, Vector2> position)
    {
        ClientUpdate += () =>
        {
            var pos = position.Invoke(new Vector2(Program.WIDTH, Program.HEIGHT));
            Area = new Rectangle(pos.X, pos.Y, Area.width, Area.height);
        };
    }

    public void Adapt(Func<Vector2, Rectangle> area)
    {
        ClientUpdate += () =>
        {
            Area = area.Invoke(new Vector2(Program.WIDTH, Program.HEIGHT));
        };
    }
}