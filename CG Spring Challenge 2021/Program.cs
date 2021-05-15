using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

class Game
{
    #region Properties

    public int Day { get; set; }
    public int Nutrients { get; set; }
    public int MySun { get; set; }
    public int MyScore { get; set; }
    public int OpponentSun { get; set; }
    public int OpponentScore { get; set; }
    public bool IsOpponentWaiting { get; set; }

    public List<Cell> Board { get; set; }
    public List<Tree> Trees { get; set; }
    public List<Action> PossibleActions { get; set; }

    public int SunDirection { get => Day % 6; }

    public int InputLineNumber { get; set; }
    public int InputTurnCount { get; set; }
    public bool InputFromFile { get; set; }
    public bool InputDataLogEnable { get; set; }
    public bool OutputDebugEnable { get; set; }
    public bool ExtraInputLineSkipEnable { get; set; }

    public Game()
    {
        Board = new List<Cell>();
        Trees = new List<Tree>();
        PossibleActions = new List<Action>();
    }

    #endregion

    #region Game AI Logic

    public Action GetNextAction()
    {
        Action action;

        // Wait a while to increase sun points
        if (Day < 1)
            return new Action(Action.WAIT);

        // 1. Try to complete
        if (Day > 20 ||
            Trees.Count(t => t.IsMine && t.Size == 3) > 3 ||
            !PossibleActions.Any(a => a.Type == Action.GROW || a.Type == Action.SEED))
        {
            action = PossibleActions
                .Where(a => a.Type == Action.COMPLETE)
                .OrderBy(a => a.TargetCellIdx)
                .FirstOrDefault();
            if (action != null)
                return action;
        }

        // 2. Try to grow
        var growactions = PossibleActions
            .Where(a => a.Type == Action.GROW)
            .OrderBy(a => a.TargetCellIdx);

        var gjaction = growactions.Join(Trees,
            ga => ga.TargetCellIdx,
            tr => tr.CellIndex,
            (action, tree) => new
            {
                Gaction = action,
                TreeSize = tree.Size
            })
            .OrderByDescending(gj => gj.TreeSize)
            .FirstOrDefault();

        if (gjaction != null)
            return gjaction.Gaction;

        // 3. Try to seed
        var seedactions = PossibleActions
            .Where(a => a.Type == Action.SEED)
            .OrderBy(a => a.TargetCellIdx);

        foreach (var seedaction in seedactions)
        {
            var noneighbour = true;
            foreach (var tree in Trees.Where(t => t.IsMine))
            {
                var cell = Board.First(c => c.Index == tree.CellIndex);
                if (cell.Neighbours.Contains(seedaction.TargetCellIdx))
                {
                    noneighbour = false;
                    break;
                }
            }

            if (noneighbour)
            {
                return seedaction;
            }
        }

        if (!Trees.Any(t => t.IsMine && t.Size == 1))
        {
            action = seedactions.FirstOrDefault();
            if (action != null)
                return action;
        }

        // 4. Nothing to do, wait for next day
        return new Action(Action.WAIT);
    }

    #endregion
}

class Cell
{
    public int Index { get; set; }
    public int Richness { get; set; }
    public int[] Neighbours { get; set; }

    public Cell(int index, int richness, int[] neighbours)
    {
        Index = index;
        Richness = richness;
        Neighbours = neighbours;
    }
}

class Tree
{
    public int CellIndex { get; set; }
    public int Size { get; set; }
    public bool IsMine { get; set; }
    public bool IsDormant { get; set; }

    public Tree(int cellIndex, int size, bool isMine, bool isDormant)
    {
        CellIndex = cellIndex;
        Size = size;
        IsMine = isMine;
        IsDormant = isDormant;
    }
}

class Action
{
    public const string WAIT = "WAIT";
    public const string SEED = "SEED";
    public const string GROW = "GROW";
    public const string COMPLETE = "COMPLETE";

    public static Action Parse(string action)
    {
        string[] parts = action.Split(" ");
        switch (parts[0])
        {
            case WAIT:
                return new Action(WAIT);
            case SEED:
                return new Action(SEED, int.Parse(parts[1]), int.Parse(parts[2]));
            case GROW:
            case COMPLETE:
            default:
                return new Action(parts[0], int.Parse(parts[1]));
        }
    }

    public string Type { get; set; }
    public int TargetCellIdx { get; set; }
    public int SourceCellIdx { get; set; }

    public Action(string type, int sourceCellIdx, int targetCellIdx)
    {
        this.Type = type;
        this.TargetCellIdx = targetCellIdx;
        this.SourceCellIdx = sourceCellIdx;
    }

    public Action(string type, int targetCellIdx)
        : this(type, 0, targetCellIdx)
    {
    }

    public Action(string type)
        : this(type, 0, 0)
    {
    }

    public override string ToString()
    {
        if (Type == WAIT) return Action.WAIT;
        if (Type == SEED) return string.Format("{0} {1} {2}", SEED, SourceCellIdx, TargetCellIdx);
        return string.Format("{0} {1}", Type, TargetCellIdx);
    }
}

class Player
{
    static readonly Game game = new Game();

    static StreamReader InputFileStream;
    static readonly string InputFileName = "InputData.txt";

    static string ConsoleReadLine()
    {
        string line;
        if (game.InputFromFile)
        {
            line = InputFileStream.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                Console.WriteLine("*** End of input file data! ***");
                Console.ReadKey();
            }
        }
        else line = Console.ReadLine();

        if (game.InputDataLogEnable)
            Console.Error.WriteLine(line);

        game.InputLineNumber++;
        return line;
    }

    static string DetectExtraLogLines(string line)
    {
        // Standard Error Stream:
        if (line.StartsWith("Standard"))
        {
            game.ExtraInputLineSkipEnable = true;
            line = ConsoleReadLine();
        }
        return line;
    }

    static void SkipExtraLinesUptoNextTurn()
    {
        if (game.ExtraInputLineSkipEnable)
        {
            int skipcount = 0;
            while (true)
            {
                var line = ConsoleReadLine();
                if (line.StartsWith("Standard"))
                    break;
                if (++skipcount > 12)
                    throw new Exception("Extra line skip count is too high!");
            }
        }
    }

    static void Main(string[] args)
    {
        //game.InputFromFile = true;       
        game.InputDataLogEnable = true;
        game.OutputDebugEnable = true;

        if (game.InputFromFile)
        {
            if (File.Exists(InputFileName))
                InputFileStream = new StreamReader(InputFileName);
            else throw new Exception(InputFileName + " - Input file not found! ");
        }

        string[] inputs;
        int numberOfCells = int.Parse(DetectExtraLogLines(ConsoleReadLine())); // 37
        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = ConsoleReadLine().Split(' ');
            int index = int.Parse(inputs[0]); // 0 is the center cell, the next cells spiral outwards
            int richness = int.Parse(inputs[1]); // 0 if the cell is unusable, 1-3 for usable cells
            int neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
            int neigh1 = int.Parse(inputs[3]);
            int neigh2 = int.Parse(inputs[4]);
            int neigh3 = int.Parse(inputs[5]);
            int neigh4 = int.Parse(inputs[6]);
            int neigh5 = int.Parse(inputs[7]);
            int[] neighs = { neigh0, neigh1, neigh2, neigh3, neigh4, neigh5 };
            game.Board.Add(new Cell(index, richness, neighs));
        }

        // game loop
        while (true)
        {
            SkipExtraLinesUptoNextTurn();
            game.Day = int.Parse(ConsoleReadLine()); // the game lasts 24 days: 0-23
            game.Nutrients = int.Parse(ConsoleReadLine()); // the base score you gain from the next COMPLETE action
            inputs = ConsoleReadLine().Split(' ');
            game.MySun = int.Parse(inputs[0]); // your sun points
            game.MyScore = int.Parse(inputs[1]); // your current score
            inputs = ConsoleReadLine().Split(' ');
            game.OpponentSun = int.Parse(inputs[0]); // opponent's sun points
            game.OpponentScore = int.Parse(inputs[1]); // opponent's score
            game.IsOpponentWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day

            game.Trees.Clear();
            int numberOfTrees = int.Parse(ConsoleReadLine()); // the current amount of trees
            for (int i = 0; i < numberOfTrees; i++)
            {
                inputs = ConsoleReadLine().Split(' ');
                int cellIndex = int.Parse(inputs[0]); // location of this tree
                int size = int.Parse(inputs[1]); // size of this tree: 0-3
                bool isMine = inputs[2] != "0"; // 1 if this is your tree
                bool isDormant = inputs[3] != "0"; // 1 if this tree is dormant
                game.Trees.Add(new Tree(cellIndex, size, isMine, isDormant));
            }

            game.PossibleActions.Clear();
            int numberOfPossibleMoves = int.Parse(ConsoleReadLine());
            for (int i = 0; i < numberOfPossibleMoves; i++)
            {
                string possibleMove = ConsoleReadLine();
                game.PossibleActions.Add(Action.Parse(possibleMove));
            }

            Action action = game.GetNextAction();
            Console.WriteLine("{0}{1}", action, game.OutputDebugEnable ? " " + action : "");
            game.InputTurnCount++;
        }
    }
}
