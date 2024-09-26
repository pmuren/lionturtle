using System;
namespace lionturtle;

public record struct Connector(string Color, int Charge)
{
    public bool ConnectsWith(Connector remoteConnector)
    {
        return (
            ValidateChargePair(Charge, remoteConnector.Charge)
            && ValidateColorPair(Color, remoteConnector.Color)
        );
    }

    public bool ValidateChargePair(int chargeA, int chargeB)
    {
        if (chargeA != chargeB) return true;
        if (chargeA == 0 && chargeB == 0) return true;
        return false;
    }

    public bool ValidateColorPair(string colorA, string colorB)
    {
        if (colorA == "red" || colorB == "black")
        {
            if (colorA == "red" && colorB == "black") return true;
            return false;
        }
        if (colorA == "black" || colorB == "red")
        {
            if (colorA == "black" && colorB == "red") return true;
            return false;
        }
        if (colorA == "white" || colorB == "white") return true;
        if (colorA == colorB && (colorA == "blue" || colorA == "green")) return true;
        return false;

    }
}