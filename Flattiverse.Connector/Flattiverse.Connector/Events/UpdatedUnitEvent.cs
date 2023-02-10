﻿using Flattiverse.Connector.Units;
using System.Text.Json;

namespace Flattiverse.Connector.Events
{
    /// <summary>
    /// This event informs of the update of a unit in the universeGroup.
    /// </summary>
    [FlattiverseEventIdentifier("unitUpdated")]
    public class UpdatedUnitEvent : UnitEvent
    {
        public readonly Unit Unit;

        internal UpdatedUnitEvent(UniverseGroup group, JsonElement element) : base(element)
        {
            Utils.Traverse(element, out JsonElement unit, "unit");

            Unit = Unit.CreateFromJson(group, unit);
        }

        public override EventKind Kind => EventKind.UnitUpdated;
    }
}