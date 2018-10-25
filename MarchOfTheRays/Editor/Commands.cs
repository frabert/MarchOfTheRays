using System.Collections.Generic;

namespace MarchOfTheRays.Editor
{
    interface ICommand
    {
        void Execute();
        void Undo();
    }

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

    class CommandList
    {
        List<ICommand> commands = new List<ICommand>();
        int pos = -1;

        public void Add(ICommand cmd)
        {
            pos++;
            int count = commands.Count - pos;
            commands.RemoveRange(pos, count);
            commands.Add(cmd);
        }

        public bool CanUndo => pos >= 0;
        public bool CanRedo => pos < commands.Count - 1;

        public void Undo()
        {
            if (!CanUndo) return;
            commands[pos].Undo();
            pos--;
        }

        public void Redo()
        {
            if (!CanRedo) return;
            pos++;
            commands[pos].Execute();
        }

        public void Clear()
        {
            pos = -1;
            commands.Clear();
        }
    }
}
