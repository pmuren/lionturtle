namespace lionturtle;

public record AxialPosition(int Q, int R)
{
    public static AxialPosition operator +(AxialPosition a, AxialPosition b)
    {
        return new AxialPosition(a.Q + b.Q, a.R + b.R);
    }

    public static AxialPosition operator -(AxialPosition a, AxialPosition b)
    {
        return new AxialPosition(a.Q - b.Q, a.R - b.R);
    }

    public static AxialPosition operator *(AxialPosition a, int scalar)
    {
        return new AxialPosition(a.Q * scalar, a.R * scalar);
    }

    public static AxialPosition operator /(AxialPosition a, int scalar)
    {
        return new AxialPosition(a.Q / scalar, a.R / scalar);
    }
}