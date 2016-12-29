using System;
using RemoteTech.Common.Interfaces;
using RemoteTech.Common.Utils;

namespace RemoteTech.Common.Commands
{
    public abstract class AbstractDelayCommand : IDelayCommand
    {
        /// <summary>
        /// Indicates whether or not the command is aborted.
        /// </summary>
        protected bool Aborted;

        public virtual int CompareTo(ICommand other)
        {
            return Priority.CompareTo(other.Priority);
        }

        public virtual int CompareTo(IDelayCommand other)
        {
            return PlannedExecutionTime.CompareTo(other.PlannedExecutionTime);
        }

        protected AbstractDelayCommand()
        {
            CommandId = new Guid();
        }

        /// <summary>
        ///     The command unique identifier.
        /// </summary>
        public virtual Guid CommandId { get; }

        /// <summary>
        /// Base delay for the command.
        /// </summary>
        public virtual double Delay
        {
            get
            {
                if (Vessel == null || Vessel.Connection == null)
                    return 0;

                return Vessel.Connection.SignalDelay;
            }
        }

        /// <summary>
        ///     A complete command description.
        /// </summary>
        public virtual string Description
        {
            get
            {
                if (!(Delay > 0) && !(ExtraDelay > 0))
                    return string.Empty;

                var delayStr = TimeUtils.FormatDuration(Delay);
                if (ExtraDelay > 0)
                    delayStr = $"{delayStr} + {TimeUtils.FormatDuration(ExtraDelay)}";

                return $"Signal delay: {delayStr}";
            }
        }

        /// <summary>
        ///     Extra delay added to the current command <see cref="TimeStamp" />.
        /// </summary>
        public virtual double ExtraDelay { get; set; }

        /// <summary>
        /// Tells whether or not the condition for the command execution is met.
        /// </summary>
        public virtual bool IsExecutable => (PlannedExecutionTime - TimeUtils.GameTime) <= 0;

        /// <summary>
        ///     Time at which the command is planned to be executed.
        /// </summary>
        public double PlannedExecutionTime => TimeStamp + TotalDelay;

        /// <summary>
        /// The command priority. From 0 (less privileged) to 255 (highest priority).
        /// </summary>
        public virtual byte Priority => 0;

        /// <summary>
        ///     A short command description.
        /// </summary>
        public abstract string ShortDescription { get; }

        /// <summary>
        ///     The time at which the command was enqueued.
        /// </summary>
        public virtual double TimeStamp { get; set; }

        /// <summary>
        ///     Total delay of the command (<see cref="IDelayCommand.Delay" /> + <see cref="IDelayCommand.ExtraDelay" />).
        /// </summary>
        public virtual double TotalDelay => Delay + ExtraDelay;

        /// <summary>
        ///     The vessel on which the command applies.
        /// </summary>
        public abstract Vessel Vessel { get;  }



        public virtual event EventHandler CommandEnqueued;
        public virtual event EventHandler CommandRemoved;
        public virtual event EventHandler CommandExecuted;
        public virtual event EventHandler CommandAborted;


        /// <summary>
        ///     Abort the command.
        /// </summary>
        public virtual void Abort()
        {
            Aborted = true;
        }

        /// <summary>
        ///     Execute the command.
        /// </summary>
        /// <returns>true if the command was successfully executed, false otherwise.</returns>
        public abstract bool Invoke();

        public abstract void Load(ConfigNode node);

        public abstract void Save(ConfigNode node);
    }
}
