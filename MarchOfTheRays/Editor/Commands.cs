using System.Collections.Generic;

namespace MarchOfTheRays.Editor
{
    /// <summary>
    /// A command than can be executed and undone
    /// </summary>
    interface ICommand
    {
        /// <summary>
        /// Executes the command
        /// </summary>
        void Execute();

        /// <summary>
        /// Undoes the command
        /// </summary>
        void Undo();
    }

    /// <summary>
    /// A command that inverts the wrapped command
    /// </summary>
    class InverseCommand : ICommand
    {
        ICommand cmd;
        public InverseCommand(ICommand cmd)
        {
            this.cmd = cmd;
        }

        public void Execute()
        {
            cmd.Undo();
        }

        public void Undo()
        {
            cmd.Execute();
        }
    }

    /// <summary>
    /// A group of commands that can be done and undone as if they were one
    /// </summary>
    class AggregateCommand : ICommand
    {
        IList<ICommand> cmds;

        public AggregateCommand(IList<ICommand> cmds)
        {
            this.cmds = cmds;
        }

        public void Execute()
        {
            for(int i = 0; i < cmds.Count; i++)
            {
                cmds[i].Execute();
            }
        }

        public void Undo()
        {
            for (int i = cmds.Count - 1; i >= 0; i--)
            {
                cmds[i].Undo();
            }
        }
    }

    /// <summary>
    /// An history of executed commands
    /// </summary>
    class CommandList
    {
        List<ICommand> commands = new List<ICommand>();
        int pos = -1;

        /// <summary>
        /// Adds a command to the history
        /// </summary>
        /// <param name="cmd">The command to be added</param>
        public void Add(ICommand cmd)
        {
            pos++;
            int count = commands.Count - pos;
            commands.RemoveRange(pos, count);
            commands.Add(cmd);
        }

        /// <summary>
        /// Whether it is possible to undo the last command
        /// </summary>
        public bool CanUndo => pos >= 0;

        /// <summary>
        /// Whether it is possibler to redo the last undone command
        /// </summary>
        public bool CanRedo => pos < commands.Count - 1;

        /// <summary>
        /// Undoes the last executed command
        /// </summary>
        /// <remarks>
        /// If <see cref="CanUndo"/> is false, this method does nothing
        /// </remarks>
        public void Undo()
        {
            if (!CanUndo) return;
            commands[pos].Undo();
            pos--;
        }

        /// <summary>
        /// Redoes the last undone command
        /// </summary>
        /// <remarks>
        /// If <see cref="CanRedo"/> is false, this method does nothing
        /// </remarks>
        public void Redo()
        {
            if (!CanRedo) return;
            pos++;
            commands[pos].Execute();
        }
        
        /// <summary>
        /// Clears the command history
        /// </summary>
        public void Clear()
        {
            pos = -1;
            commands.Clear();
        }
    }
}
