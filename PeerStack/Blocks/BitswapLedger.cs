﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Blocks
{
    /// <summary>
    ///   Statistics on the bitswap blocks exchanged with another <see cref="Peer"/>.
    /// </summary>
     public record class BitswapLedger
    {
        /// <summary>
        /// The <see cref="Peer"/> that pertains to this ledger.
        /// </summary>
        /// <value>
        /// The peer that is being monitored.
        /// </value>
        public required Peer Peer { get; set; }

        /// <summary>
        ///   The number of blocks exchanged with the <see cref="Peer"/>.
        /// </summary>
        /// <value>
        ///   The number of blocks sent by the peer or sent by us to the peer.
        /// </value>
        public ulong BlocksExchanged { get; set; }

        /// <summary>
        ///   The number of bytes sent by the <see cref="Peer"/> to us.
        /// </summary>
        /// <value>
        ///   The number of bytes.
        /// </value>
        public ulong DataReceived { get; set; }

        /// <summary>
        ///   The number of bytes sent by us to the <see cref="Peer"/>
        /// </summary>
        /// <value>
        ///   The number of bytes.
        /// </value>
        public ulong DataSent { get; set; }

        /// <summary>
        ///   The calculated debt to the peer.
        /// </summary>
        /// <value>
        ///   <see cref="DataSent"/> divided by <see cref="DataReceived"/>.
        ///   A value less than 1 indicates that we are in debt to the
        ///   <see cref="Peer"/>.
        /// </value>
        public float DebtRatio => (float)DataSent / (float)(DataReceived + 1); // +1 is to prevent division by zero

        /// <summary>
        /// Determines if we owe the <see cref="Peer"/> some blocks.
        /// </summary>
        /// <value>
        /// <b>true</b> if we owe data to the peer; otherwise, <b>false</b>.
        /// </value>
        public bool IsInDebt => DebtRatio < 1;
    }
}
