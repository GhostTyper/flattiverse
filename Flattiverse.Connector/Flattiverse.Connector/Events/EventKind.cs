﻿namespace Flattiverse.Connector.Events
{
    /// <summary>
    /// Specifies the various event kinds for a better switch() experience.
    /// </summary>
    public enum EventKind
    {
        /// <summary>
        /// A Fallback event for debugging purposes, if the event sent from the server is unknown to the connector.
        /// </summary>
        RAW,
        /// <summary>
        /// This event indicates some critical out-of-game failure like a problem with the
        /// data-transport, etc.. Consider upgrading the connector if this happens and it
        /// is not due to a lost connection.
        /// </summary>
        Failure,
        /// <summary>
        /// This event notifies about the meta informations a UniverseGroup has, like Name, Description, Teams, Rules...
        /// You actually don't need to parse this event because it's also parsed by the connector and the results are
        /// presented in fields on the UniverseGroup.
        /// </summary>
        UniverseGroupInfo,
        /// <summary>
        /// This event contains all information abaout a player.
        /// </summary>
        PlayerFullUpdate,
        /// <summary>
        /// This event contains only mutable information about a player.
        /// </summary>
        PlayerPartialUpdate,
        /// <summary>
        /// This event informs of the disconnect of a player from the universeGroup.
        /// </summary>
        PlayerRemoved
    }
}