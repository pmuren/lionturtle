namespace lionturtle;

public static class Constants
{
    public static readonly AxialPosition[] axial_directions = {
        new AxialPosition(+1, 0), new AxialPosition(+1, -1), new AxialPosition(0, -1),
        new AxialPosition(-1, 0), new AxialPosition(-1, +1), new AxialPosition(0, +1),
    };

    public static readonly AxialPosition[] dualDirections = new AxialPosition[]
    {
        new AxialPosition(+2, -1), new AxialPosition(+1, -2),
        new AxialPosition(-1, -1), new AxialPosition(-2, +1),
        new AxialPosition(-1, +2), new AxialPosition(+1, +1)
    };
}

