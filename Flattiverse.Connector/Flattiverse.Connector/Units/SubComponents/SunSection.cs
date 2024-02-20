﻿using Flattiverse.Connector.Network;
using Flattiverse.Connector.UnitConfigurations;

namespace Flattiverse.Connector.Units.SubComponents
{
    public class SunSection
    {
        private double innerRadius;
        private double outerRadius;
        private double angelFrom;
        private double angelTo;

        private double energy;
        private double ions;

        private readonly SunConfiguration? Configuration;

        internal SunSection(SunConfiguration configuration)
        {
            Configuration = configuration;

            InnerRadius = 100;
            OuterRadius = 130;

            AngelFrom = 45;
            AngelTo = 135;

            Energy = 4;
            Ions = 0;
        }

        internal SunSection(SunConfiguration? configuration, PacketReader reader)
        {
            Configuration = configuration;

            InnerRadius = reader.Read2U(100);
            OuterRadius = reader.Read2U(100);
            AngelFrom = reader.Read2U(100);
            AngelTo = reader.Read2U(100);

            Energy = reader.Read2S(100);
            Ions = reader.Read2S(100);
        }

        internal void Write(PacketWriter writer)
        {
            writer.Write2U(InnerRadius, 100);
            writer.Write2U(OuterRadius, 100);
            writer.Write2U(AngelFrom, 100);
            writer.Write2U(AngelTo, 100);

            writer.Write2S(Energy, 100);
            writer.Write2S(Ions, 100);
        }

        public void Remove()
        {
            Configuration?.sections.Remove(this);
        }

        /// <summary>
        /// The inner radius (=radius which is nearer to the sun) of the section.
        /// </summary>
        public double InnerRadius
        {
            get => innerRadius;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value) || value < 0.0 || value >= outerRadius)
                    throw new GameException(0x31);

                if (Configuration is null)
                    throw new GameException(0x34);

                innerRadius = value;
            }
        }

        /// <summary>
        /// The outer radius (=radius which is farer away to the sun) of the section.
        /// </summary>
        public double OuterRadius
        {
            get => outerRadius;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value) || value > 1000.0 || innerRadius >= value)
                    throw new GameException(0x31);

                if (Configuration is null)
                    throw new GameException(0x34);

                outerRadius = value;
            }
        }

        /// <summary>
        /// Sets the radius for the inner and the outer radius at once.
        /// </summary>
        /// <param name="inner">The inner radius.</param>
        /// <param name="outer">The outer radius.</param>
        /// <exception cref="GameException">Thrown, if you don't have permission to do this or the values are invalid.</exception>
        public void SetRadii(double inner, double outer)
        {
            if (double.IsInfinity(inner) || double.IsNaN(inner) || inner < 0.0 || inner >= outer)
                throw new GameException(0x31);

            if (double.IsInfinity(outer) || double.IsNaN(outer) || outer > 2000.0)
                throw new GameException(0x31);

            if (Configuration is null)
                throw new GameException(0x34);

            innerRadius = inner;
            outerRadius = outer;
        }

        /// <summary>
        /// Sets the angle for the left (from) and the right (to) side at once.
        /// </summary>
        /// <param name="from">The from (left) radius.</param>
        /// <param name="to">The to (right) radius.</param>
        /// <exception cref="GameException">Thrown, if you don't have permission to do this or the values are invalid.</exception>
        public void SetAngels(double from, double to)
        {
            if (double.IsInfinity(from) || double.IsNaN(from) || from < 0.0 || from >= to)
                throw new GameException(0x31);

            if (double.IsInfinity(to) || double.IsNaN(to) || to > 360.0)
                throw new GameException(0x31);

            if (Configuration is null)
                throw new GameException(0x34);

            angelFrom = from;
            angelTo = to;
        }

        /// <summary>
        /// The left angle, when you look from the middle point of the sun to the section.
        /// </summary>
        public double AngelFrom
        {
            get => angelFrom;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value) || value < 0.0 || value >= angelTo)
                    throw new GameException(0x31);

                if (Configuration is null)
                    throw new GameException(0x34);

                angelFrom = value;
            }
        }

        /// <summary>
        /// The right angle, when you look from the middle point of the sun to the section.
        /// </summary>
        public double AngelTo
        {
            get => angelTo;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value) || value > 360.0 || angelFrom >= value)
                    throw new GameException(0x31);

                if (Configuration is null)
                    throw new GameException(0x34);

                angelTo = value;
            }
        }

        /// <summary>
        /// The energy output in this corona. This value multiplied with EnergyCells results in the energy loaded per Second. 
        /// </summary>
        public double Energy
        {
            get => energy;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value) || energy > 500.0 || energy < -500.0)
                    throw new GameException(0x31);

                if (Configuration is null)
                    throw new GameException(0x34);

                energy = value;
            }
        }

        /// <summary>
        /// The ions output in this corona. This value multiplied with IonCells results in the ions loaded per Second. 
        /// </summary>
        public double Ions
        {
            get => ions;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value) || ions > 50.0 || ions < -50.0)
                    throw new GameException(0x31);

                if (Configuration is null)
                    throw new GameException(0x34);

                ions = value;
            }
        }
    }
}