﻿using Flattiverse.Connector.Hierarchy;
using Flattiverse.Connector.Network;
using Flattiverse.Connector.UnitConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flattiverse.Connector.Units
{
    public class Buoy : CelestialBody
    {
        internal Buoy(Cluster cluster, PacketReader reader) : base(cluster, reader)
        {
        }

        /// <summary>
        /// Sets given values in this unit.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task<Buoy> Configure(Action<BuoyConfiguration> config)
        {
            Session session = await Cluster.Galaxy.GetSession();

            Packet packet = new Packet();
            packet.Header.Command = 0x50;
            packet.Header.Id0 = (byte)Cluster.ID;
            packet.Header.Param0 = (byte)Kind;

            using (PacketWriter writer = packet.Write())
                writer.Write(Name);

            Packet configurationPacket = await session.SendWait(packet);
            BuoyConfiguration changes = new BuoyConfiguration(configurationPacket.Read());
            config(changes);

            session = await Cluster.Galaxy.GetSession();

            packet = new Packet();
            packet.Header.Command = 0x52;
            packet.Header.Id0 = (byte)Cluster.ID;
            packet.Header.Param0 = (byte)Kind;

            using (PacketWriter writer = packet.Write())
            {
                writer.Write(Name);
                changes.Write(writer);
            }

            await session.SendWait(packet);

            if (!Cluster.TryGetUnit(changes.Name, out Unit? unit) || unit is not Buoy buoy)
                throw new GameException(0x35);

            return buoy;
        }

        internal override void Update(PacketReader reader)
        {
            base.Update(reader);
        }

        /// <summary>
        /// Removes this unit.
        /// </summary>
        /// <returns></returns>
        public async Task Remove()
        {
            Session session = await Cluster.Galaxy.GetSession();

            Packet packet = new Packet();
            packet.Header.Command = 0x53;
            packet.Header.Id0 = (byte)Cluster.ID;
            packet.Header.Param0 = (byte)Kind;

            using (PacketWriter writer = packet.Write())
                writer.Write(Name);

            await session.SendWait(packet);
        }

        public override UnitKind Kind => UnitKind.Buoy;

        public override string ToString()
        {
            return $"Buoy {Name}";
        }
    }
}
