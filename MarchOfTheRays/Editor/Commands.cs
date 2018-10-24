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
