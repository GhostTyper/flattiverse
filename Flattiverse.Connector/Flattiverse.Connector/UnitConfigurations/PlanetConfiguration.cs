﻿using Flattiverse.Connector.Network;
using Flattiverse.Connector.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flattiverse.Connector.UnitConfigurations
{
    public class PlanetConfiguration : CelestialBodyConfiguration
    {
        private PlanetConfiguration() : base() { }

        internal PlanetConfiguration(PacketReader reader) : base(reader)
        {
            //TODO
        }

        internal override void Write(PacketWriter writer)
        {
            base.Write(writer);

            //TODO
        }

        public override UnitKind Kind => UnitKind.Planet;

        internal static PlanetConfiguration Default => new PlanetConfiguration();
    }
}
