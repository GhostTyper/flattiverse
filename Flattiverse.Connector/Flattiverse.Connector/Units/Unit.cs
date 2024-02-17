﻿using Flattiverse.Connector.Hierarchy;
using Flattiverse.Connector.UnitConfigurations;

namespace Flattiverse.Connector.Units;

/// <summary>
/// Represents a unit in Flattiverse. Each unit in an Cluster derives from this class. This class has
/// properties and methods which most units have in common. Derived classes overwrite those properties
/// and methods, other properties and methods are added via interfaces.
/// </summary>
public class Unit
{
    /// <summary>
    /// This is the name of the unit. An unit can't change her name after it has been setup.
    /// </summary>
    public readonly string Name;

    internal Unit(Configuration configuration)
    {
        Name = configuration.Name;
    }

    /// <summary>
    /// Specifies if this unit can hide other units behind her. True means you can't see behind this unit in a scan.
    /// </summary>
    public virtual bool IsMasking => true;

    /// <summary>
    /// Specifies, if the unit can collide with another unit.
    /// </summary>
    public virtual bool IsSolid => true;

    /// <summary>
    /// Specifies if the unit can be seen by other units. (When scanning, etc.)
    /// </summary>
    public virtual bool IsVisible => true;

    /// <summary>
    /// Specifies, if the map editor can edit this unit or not.
    /// </summary>
    public virtual bool CanBeEdited => false;
    
    /// <summary>
    /// The speed limit. If this limit is exceeded units will be slowed down by 6% of their overshooting speed. 
    /// </summary>
    public virtual double SpeedLimit => 0.0;

    /// <summary>
    /// The direction this unit is facing to.
    /// </summary>
    public virtual double Direction => 0.0;

    /// <summary>
    /// The movement of the unit.
    /// </summary>
    public virtual Vector Movement => Vector.Null;
    
    /// <summary>
    /// The position of the unit.
    /// </summary>
    public virtual Vector Position => Vector.Null;

    /// <summary>
    /// The gravity this unit has on other units.
    /// </summary>
    public virtual double Gravity => 0.0;

    /// <summary>
    /// The radius of the unit. 
    /// </summary>
    public virtual double Radius => 1.0;

    /// <summary>
    /// This factor will be multiplied with the distance of the unit to match, if you can see it. 0.9 means you
    /// can see the unit 10% worse than with 100%.
    /// </summary>
    public virtual double VisibleRangeMultiplier => 1.0;

    /// <summary>
    /// This specifies of what kind this unit is.
    /// </summary>
    public virtual Mobility Mobility => Mobility.Still;

    /// <summary>
    /// Specifies the current team or null if the unit doesn't belong to a team. 
    /// </summary>
    public virtual Team? Team => null;

    /// <summary>
    /// Specifies the Kind of the unit.
    /// </summary>
    public virtual UnitKind Kind => UnitKind.Sun;
}