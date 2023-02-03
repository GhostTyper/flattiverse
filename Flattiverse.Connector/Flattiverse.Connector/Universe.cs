﻿using Flattiverse.Connector.Accounts;
using Flattiverse.Connector.Network;
using Flattiverse.Connector.Units;
using System.Text.Json;

namespace Flattiverse.Connector
{
    /// <summary>
    /// Represents an universe within your universe group.
    /// </summary>
    public class Universe
    {
        /// <summary>
        /// The ID of the universe.
        /// </summary>
        public readonly int ID;

        /// <summary>
        /// The name of the universe.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The UniverseGroup this object belongs to.
        /// </summary>
        public readonly UniverseGroup Group;

        internal Universe(UniverseGroup group, JsonElement element)
        {
            Utils.Traverse(element, out ID, "id");
            Utils.Traverse(element, out Name, "name");

            Group = group;
        }

        /// <summary>
        /// Creates or updates a unit in the map of this universe group. The unit will be overwritten if the unit already
        /// exists (same name) and the colliding unit isn't a non editable unit like a player unit, shot or explosion.
        /// </summary>
        /// <param name="definition">The JSON formatted definition of the unit to create. Please refer to PROCOTOL.md for further information.</param>
        /// <remarks>This method is only accessible if you are an administrator.</remarks>
        public async Task SetUnit(string definition)
        {
            if (string.IsNullOrEmpty(definition))
                throw new GameException(0xB0);

            if (definition.Length > 2048)
                throw new GameException(0xB1);

            using (Query query = Group.connection.Query("unitSet"))
            {
                query.Write("universe", ID);
                query.Write("unit", definition);

                await query.Send().ConfigureAwait(false);

                await query.Wait().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Removes an unit from the universe.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <returns>Nothing, or a GameException.</returns>
        /// <exception cref="GameException">Throws when trying to remove a non editable or non existing unit.</exception>
        public async Task RemoveUnit(string name)
        {
            if (!Utils.CheckName(name))
                throw new GameException(0x02);

            using (Query query = Group.connection.Query("unitRemove"))
            {
                query.Write("universe", ID);
                query.Write("unit", name);

                await query.Send().ConfigureAwait(false);

                await query.Wait().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves the json definition for the map editor of a unit from the universe.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <returns>The map editable json string.</returns>
        /// <exception cref="GameException">Throws when trying to access a non editable unit.</exception>
        public async Task<string> GetUnitMapEditJson(string name)
        {
            if (!Utils.CheckName(name))
                throw new GameException(0x02);

            using (Query query = Group.connection.Query("unitGet"))
            {
                query.Write("universe", ID);
                query.Write("unit", name);

                await query.Send().ConfigureAwait(false);

                return await query.ReceiveString().ConfigureAwait(false);
            }
        }
    }
}