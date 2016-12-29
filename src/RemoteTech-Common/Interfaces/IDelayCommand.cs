using System;

namespace RemoteTech.Common.Interfaces
{
    /// <summary>
    ///     A command whose execution can be delayed in time.
    /// </summary>
    public interface IDelayCommand : ICommand, IComparable<IDelayCommand>
    {
        /// <summary>
        ///     Base delay for the command.
        /// </summary>
        double Delay { get; }

        /// <summary>
        ///     Extra delay added to the current command <see cref="Delay" />.
        /// </summary>
        double ExtraDelay { get; set; }

        /// <summary>
        ///     Total delay of the command (<see cref="Delay" /> + <see cref="ExtraDelay" />).
        /// </summary>
        double TotalDelay { get; }

        /// <summary>
        ///     Time at which the command is planned to be executed.
        /// </summary>
        /// <remarks><see cref="PlannedExecutionTime" /> = <see cref="ICommand.TimeStamp" /> + <see cref="TotalDelay" /></remarks>
        double PlannedExecutionTime { get; }
    }
}