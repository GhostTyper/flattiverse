﻿using Flattiverse.Connector.Network;
using System.Xml.Linq;

namespace Flattiverse.Connector.Hierarchy
{
    public class ShipDesignConfig
    {
        private string name;
        public double CostEnergy;
        public double CostIon;
        public double CostIron;
        public double CostTungsten;
        public double CostSilicon;
        public double CostTritium;
        public ushort CostTime;
        public double Hull;
        public double HullRepair;
        public double Shields;
        public double ShieldsLoad;
        public double Size;
        public double Weight;
        public double EnergyMax;
        public double EnergyCells;
        public double EnergyReactor;
        public double EnergyTransfer;
        public double IonMax;
        public double IonCells;
        public double IonReactor;
        public double IonTransfer;
        public double ThrusterForward;
        public double ThrusterBackward;
        public double Nozzle;
        public double Speed;
        public double Turnrate;
        public double Cargo;
        public double Extractor;
        public double WeaponSpeed;
        public ushort WeaponTime;
        public double WeaponLoad;
        public ushort WeaponAmmo;
        public double WeaponAmmoProduction;
        public bool FreeSpawn;
        public double NozzleEnergyConsumption;
        public double ThrusterEnergyConsumption;
        public double HullRepairEnergyConsumption;
        public double HullRepairIronConsumption;
        public double ShieldsIonConsumption;
        public double ExtractorEnergyConsumption;
        public double WeaponEnergyConsumption;
        public double ScannerEnergyConsumption;
        public double ScannerRange;
        public double ScannerWidth;

        /// <summary>
        /// The name of the configured unit.
        /// </summary>
        /// <exception cref="GameException">0x32 may be thrown, if the name violates rules.</exception>
        public string Name
        {
            get => name;
            set
            {
                if (!Utils.CheckName32(value))
                    throw new GameException(0x31);

                name = value;
            }
        }

        private ShipDesignConfig()
        {
            Name = string.Empty;
            CostEnergy = 0;
            CostIon = 0;
            CostIron = 0;
            CostTungsten = 0;
            CostSilicon = 0;
            CostTritium = 0;
            CostTime = 0;
            Hull = 0;
            HullRepair = 0;
            Shields = 0;
            ShieldsLoad = 0;
            Size = 0;
            Weight = 0;
            EnergyMax = 0;
            EnergyCells = 0;
            EnergyReactor = 0;
            EnergyTransfer = 0;
            IonMax = 0;
            IonCells = 0;
            IonReactor = 0;
            IonTransfer = 0;
            ThrusterForward = 0;
            ThrusterBackward = 0;
            Nozzle = 0;
            Speed = 0;
            Turnrate = 0;
            Cargo = 0;
            Extractor = 0;
            WeaponSpeed = 0;
            WeaponTime = 0;
            WeaponLoad = 0;
            WeaponAmmo = 0;
            WeaponAmmoProduction = 0;
            FreeSpawn = true;
            NozzleEnergyConsumption = 0;
            ThrusterEnergyConsumption = 0;
            HullRepairEnergyConsumption = 0;
            HullRepairIronConsumption = 0;
            ShieldsIonConsumption = 0;
            ExtractorEnergyConsumption = 0;
            WeaponEnergyConsumption = 0;
            ScannerEnergyConsumption = 0;
            ScannerRange = 0;
            ScannerWidth = 0;
        }

        internal ShipDesignConfig(ShipDesignConfig ship)
        {
            Name = ship.Name;
            CostEnergy = ship.CostEnergy;
            CostIon = ship.CostIon;
            CostIron = ship.CostIron;
            CostTungsten = ship.CostTungsten;
            CostSilicon = ship.CostSilicon;
            CostTritium = ship.CostTritium;
            CostTime = ship.CostTime;
            Hull = ship.Hull;
            HullRepair = ship.HullRepair;
            Shields = ship.Shields;
            ShieldsLoad = ship.ShieldsLoad;
            Size = ship.Size;
            Weight = ship.Weight;
            EnergyMax = ship.EnergyMax;
            EnergyCells = ship.EnergyCells;
            EnergyReactor = ship.EnergyReactor;
            EnergyTransfer = ship.EnergyTransfer;
            IonMax = ship.IonMax;
            IonCells = ship.IonCells;
            IonReactor = ship.IonReactor;
            IonTransfer = ship.IonTransfer;
            ThrusterForward = ship.ThrusterForward;
            ThrusterBackward = ship.ThrusterBackward;
            Nozzle = ship.Nozzle;
            Speed = ship.Speed;
            Turnrate = ship.Turnrate;
            Cargo = ship.Cargo;
            Extractor = ship.Extractor;
            WeaponSpeed = ship.WeaponSpeed;
            WeaponTime = ship.WeaponTime;
            WeaponLoad = ship.WeaponLoad;
            WeaponAmmo = ship.WeaponAmmo;
            WeaponAmmoProduction = ship.WeaponAmmoProduction;
            FreeSpawn = ship.FreeSpawn;
            NozzleEnergyConsumption = ship.NozzleEnergyConsumption;
            ThrusterEnergyConsumption = ship.ThrusterEnergyConsumption;
            HullRepairEnergyConsumption = ship.HullRepairEnergyConsumption;
            HullRepairIronConsumption = ship.HullRepairIronConsumption;
            ShieldsIonConsumption = ship.ShieldsIonConsumption;
            ExtractorEnergyConsumption = ship.ExtractorEnergyConsumption;
            WeaponEnergyConsumption = ship.WeaponEnergyConsumption;
            ScannerEnergyConsumption = ship.ScannerEnergyConsumption;
            ScannerRange = ship.ScannerRange;
            ScannerWidth = ship.ScannerWidth;
        }

        internal ShipDesignConfig(PacketReader reader)
        {
            Name = reader.ReadString();
            CostEnergy = reader.Read4U(1000);
            CostIon = reader.Read4U(1000);
            CostIron = reader.Read4U(1000);
            CostTungsten = reader.Read4U(1000);
            CostSilicon = reader.Read4U(1000);
            CostTritium = reader.Read4U(1000);
            CostTime = reader.ReadUInt16();
            Hull = reader.Read3U(10000);
            HullRepair = reader.Read3U(10000);
            Shields = reader.Read3U(10000);
            ShieldsLoad = reader.Read3U(10000);
            Size = reader.Read3U(1000);
            Weight = reader.Read2S(10000);
            EnergyMax = reader.Read4U(1000);
            EnergyCells = reader.Read4U(1000);
            EnergyReactor = reader.Read4U(1000);
            EnergyTransfer = reader.Read4U(1000);
            IonMax = reader.Read4U(1000);
            IonCells = reader.Read4U(1000);
            IonReactor = reader.Read4U(1000);
            IonTransfer = reader.Read4U(1000);
            ThrusterForward = reader.Read2U(10000);
            ThrusterBackward = reader.Read2U(10000);
            Nozzle = reader.Read2U(100);
            Speed = reader.Read2U(1000);
            Turnrate = reader.Read2U(100);
            Cargo = reader.Read4U(1000);
            Extractor = reader.Read4U(1000);
            WeaponSpeed = reader.Read2U(1000);
            WeaponTime = reader.ReadUInt16();
            WeaponLoad = reader.Read3U(1000);
            WeaponAmmo = reader.ReadUInt16();
            WeaponAmmoProduction = reader.Read4U(1000);
            FreeSpawn = reader.ReadBoolean();
            
            NozzleEnergyConsumption = reader.Read4U(1000);
            ThrusterEnergyConsumption = reader.Read4U(1000);
            HullRepairEnergyConsumption = reader.Read4U(1000);
            HullRepairIronConsumption = reader.Read4U(1000);
            ShieldsIonConsumption = reader.Read4U(1000);
            ExtractorEnergyConsumption = reader.Read4U(1000);
            WeaponEnergyConsumption = reader.Read4U(1000);
            ScannerEnergyConsumption = reader.Read4U(1000);
            ScannerRange = reader.Read3U(1000);
            ScannerWidth = reader.Read2U(100);
        }

        internal static ShipDesignConfig Default => new ShipDesignConfig();

        internal void Write(PacketWriter writer)
        {
            writer.Write(Name);
            writer.Write4U(CostEnergy, 1000);
            writer.Write4U(CostIon, 1000);
            writer.Write4U(CostIron, 1000);
            writer.Write4U(CostTungsten, 1000);
            writer.Write4U(CostSilicon, 1000);
            writer.Write4U(CostTritium, 1000);
            writer.Write2U(CostTime, 10);
            writer.Write3U(Hull, 10000);
            writer.Write3U(HullRepair, 10000);
            writer.Write3U(Shields, 1000);
            writer.Write3U(ShieldsLoad, 10000);
            writer.Write3U(Size, 1000);
            writer.Write2S(Weight, 10000);
            writer.Write4U(EnergyMax, 1000);
            writer.Write4U(EnergyCells, 1000);
            writer.Write4U(EnergyReactor, 1000);
            writer.Write4U(EnergyTransfer, 1000);
            writer.Write4U(IonMax, 1000);
            writer.Write4U(IonCells, 1000);
            writer.Write4U(IonReactor, 1000);
            writer.Write4U(IonTransfer, 1000);
            writer.Write2U(ThrusterForward, 10000);
            writer.Write2U(ThrusterBackward, 10000);
            writer.Write2U(Nozzle, 100);
            writer.Write2U(Speed, 1000);
            writer.Write2U(Turnrate, 100);
            writer.Write4U(Cargo, 1000);
            writer.Write4U(Extractor, 1000);
            writer.Write2U(WeaponSpeed, 1000);
            writer.Write((ushort)WeaponTime);
            writer.Write3U(WeaponLoad, 1000);
            writer.Write(WeaponAmmo);
            writer.Write4U(WeaponAmmoProduction, 1000);
            writer.Write(FreeSpawn);
            writer.Write4U(NozzleEnergyConsumption, 1000);
            writer.Write4U(ThrusterEnergyConsumption, 1000);
            writer.Write4U(HullRepairEnergyConsumption, 1000);
            writer.Write4U(HullRepairIronConsumption, 1000);
            writer.Write4U(ShieldsIonConsumption, 1000);
            writer.Write4U(ExtractorEnergyConsumption, 1000);
            writer.Write4U(WeaponEnergyConsumption, 1000);
            writer.Write4U(ScannerEnergyConsumption, 1000);
            writer.Write3U(ScannerRange, 1000);
            writer.Write2U(ScannerWidth, 100);
        }
    }
}
