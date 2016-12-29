using System;

namespace RemoteTech.Common.Interfaces
{
    /// <summary>
    ///     Command interface.
    /// </summary>
    public interface ICommand : IComparable<ICommand>, IConfigNode
    {
        /*
         * Command Properties 
         */

        /// <summary>
        ///     The command unique identifier.
        /// </summary>
        Guid CommandId { get; }

        /// <summary>
        ///     A complete command description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Tells whether or not the condition for the command execution is met.
        /// </summary>
        bool IsExecutable { get; }

        /// <summary>
        /// The command priority. From 0 (less privileged) to 255 (highest priority).
        /// </summary>
        byte Priority { get; }

        /// <summary>
        ///     A short command description.
        /// </summary>
        string ShortDescription { get; }

        /// <summary>
        ///     The time at which the command was enqueued.
        /// </summary>
        double TimeStamp { get; set; }

        /// <summary>
        ///     The vessel on which the command applies.
        /// </summary>
        Vessel Vessel { get; }

        /*
         * Command Events
         */

        event EventHandler CommandEnqueued;
        event EventHandler CommandRemoved;
        event EventHandler CommandExecuted;
        event EventHandler CommandAborted;

        /*
         * Command methods
         */

        /// <summary>
        ///     Abort the command.
        /// </summary>
        void Abort();

        /// <summary>
        ///     Execute the command.
        /// </summary>
        /// <returns>true if the command was successfully executed, false otherwise.</returns>
        bool Invoke();
    }
}