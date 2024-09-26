using System;
using System.Xml.Linq;

namespace lionturtle
{
    //public record struct Module(Connector[] Connectors)
    //{
    //    public Module GetRotated(int direction) //110000 -> 011000
    //    {
    //        Connector[] rotated = new Connector[6];
    //        for (int i = 0; i < 6; i++)
    //        {
    //            rotated[i] = Connectors[(i + direction + 6) % 6];
    //        }
    //        return new Module(rotated);
    //    }

    //    public bool Supports(Module remoteModule, int direction)
    //    {
    //        if (Connectors[direction].ConnectsWith(remoteModule.Connectors[(direction + 3) % 6])) return true;
    //        else return false;
    //    }
    //}

    public record Module(
        Connector connector0,
        Connector connector1,
        Connector connector2,
        Connector connector3,
        Connector connector4,
        Connector connector5
    )
    {
        public Connector[] GetConnectorArray()
        {
            return new Connector[]
            {
                connector0,
                connector1,
                connector2,
                connector3,
                connector4,
                connector5
            };
        }

        public Module GetRotated(int direction) //110000 -> 011000
        {
            Connector[] connectors = GetConnectorArray();
            Connector[] rotated = new Connector[6];
            for (int i = 0; i < 6; i++)
            {
                rotated[i] = connectors[(i - direction + 6) % 6];
            }
            //connectors = "001100"
            //rotated, dir=1 = "000110"

            return new Module(
                rotated[0],
                rotated[1],
                rotated[2],
                rotated[3],
                rotated[4],
                rotated[5]
            );
        }

        public bool Supports(Module remoteModule, int direction)
        {
            Connector[] homeConnectors = GetConnectorArray();
            Connector[] remoteConnectors = remoteModule.GetConnectorArray();
            if (homeConnectors[direction].ConnectsWith(remoteConnectors[(direction + 3) % 6])) return true;
            else return false;
        }

        public int[] GetRelativeVertexHeights()
        {
            var verts = new int[6];

            var connectorArray = GetConnectorArray();

            for(int direction = 0; direction < 6; direction++)
            {
                Connector prevConnector = connectorArray[direction];
                Connector nextConnector = connectorArray[(direction + 1) % 6];

                if(prevConnector.Charge == 0 && nextConnector.Charge == 0)
                {
                    verts[direction] = 0;
                }
                else if(prevConnector.Charge == -1 || nextConnector.Charge == -1)
                {
                    verts[direction] = 0;
                }
                else if(prevConnector.Charge == 1 || nextConnector.Charge == 1)
                {
                    verts[direction] = 1;
                }
            }

            return verts;
        }
    }
}

//direction 0
//110000
//110000

//direction 1
//110000
//011000

//direction 2
//110000
//001100