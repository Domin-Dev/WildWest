using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Map
{
    public class ClientTile
    {
        public int tileID;
        public byte variant;

        public ClientTile(FixedTile tile)
        {
            tileID = tile.tileID;
            variant = tile.variant;
        }
    }
}
