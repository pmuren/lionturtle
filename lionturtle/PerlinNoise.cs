using System;
namespace lionturtle;

public class PerlinNoise
{
    private int[] permutationTable;

    public PerlinNoise(int seed)
    {
        Random random = new Random(seed);
        permutationTable = new int[512];
        int[] p = new int[256];

        // Fill p with values from 0 to 255
        for (int i = 0; i < 256; i++)
            p[i] = i;

        // Shuffle using the given seed
        for (int i = 0; i < 256; i++)
        {
            int j = random.Next(i, 256);
            int temp = p[i];
            p[i] = p[j];
            p[j] = temp;
        }

        // Duplicate the permutation to avoid overflow
        for (int i = 0; i < 512; i++)
            permutationTable[i] = p[i & 255];
    }

    private double Fade(double t)
    {
        // Fade function as defined by Ken Perlin
        // 6t^5 - 15t^4 + 10t^3
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private double Lerp(double a, double b, double t)
    {
        // Linear interpolate between a and b
        return a + t * (b - a);
    }

    private double Grad(int hash, double x, double y)
    {
        // Calculate gradient vector
        int h = hash & 7;
        double u = h < 4 ? x : y;
        double v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    public double Noise(double x, double y)
    {
        // Calculate noise contributions from each of the four corners
        int xi = (int)Math.Floor(x) & 255;
        int yi = (int)Math.Floor(y) & 255;
        double xf = x - Math.Floor(x);
        double yf = y - Math.Floor(y);
        double u = Fade(xf);
        double v = Fade(yf);
        int aa, ab, ba, bb;
        aa = permutationTable[permutationTable[xi] + yi];
        ab = permutationTable[permutationTable[xi] + yi + 1];
        ba = permutationTable[permutationTable[xi + 1] + yi];
        bb = permutationTable[permutationTable[xi + 1] + yi + 1];

        double x1, x2, y1;
        x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);
        y1 = Lerp(x1, x2, v);

        return (y1 + 1) / 2; // Normalize to range [0, 1]
    }
}


