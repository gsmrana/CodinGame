﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

class Cell
{
    public int index;
    public int richness;
    public int[] neighbours;

    public Cell(int index, int richness, int[] neighbours)
    {
        this.index = index;
        this.richness = richness;
        this.neighbours = neighbours;
    }
}

class Tree
{
    public int cellIndex;
    public int size;
    public bool isMine;
    public bool isDormant;

    public Tree(int cellIndex, int size, bool isMine, bool isDormant)
    {
        this.cellIndex = cellIndex;
        this.size = size;
        this.isMine = isMine;
        this.isDormant = isDormant;
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

    public string type;
    public int targetCellIdx;
    public int sourceCellIdx;

    public Action(string type, int sourceCellIdx, int targetCellIdx)
    {
        this.type = type;
        this.targetCellIdx = targetCellIdx;
        this.sourceCellIdx = sourceCellIdx;
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
        if (type == WAIT)
        {
            return Action.WAIT;
        }
        if (type == SEED)
        {
            return string.Format("{0} {1} {2}", SEED, sourceCellIdx, targetCellIdx);
        }
        return string.Format("{0} {1}", type, targetCellIdx);
    }
}

class Game
{
    public int day;
    public int nutrients;
    public bool opponentIsWaiting;
    public int mySun, opponentSun;
    public int myScore, opponentScore;

    public List<Cell> Board { get; set; }
    public List<Tree> Trees { get; set; }
    public List<Action> PossibleActions { get; set; }

    public bool InputDataLogEnable { get; set; }
    public bool ExtraInputLineSkipEnable { get; set; }

    public Game()
    {
        Board = new List<Cell>();
        Trees = new List<Tree>();
        PossibleActions = new List<Action>();
    }

    public Action GetNextAction()
    {
        // TODO: write your algorithm here
        return PossibleActions.First();
    }
}

class Player
{
    static readonly Game game = new Game();

    static string ConsoleReadLine()
    {
        string line = Console.ReadLine();
        if (game.InputDataLogEnable)
            Console.Error.WriteLine(line);
        return line;
    }

    static string SkipExtraLogLines(string line)
    {
        // Standard Error Stream:
        if (line.StartsWith("Standard"))
        {
            game.ExtraInputLineSkipEnable = true;
            line = ConsoleReadLine();
        }
        return line;
    }

    static void IgnoreExtraLines()
    {
        if (game.ExtraInputLineSkipEnable)
        {
            ConsoleReadLine();
            ConsoleReadLine();
        }
    }

    static void Main(string[] args)
    {
        // Enable this to log the game inputs
        //game.InputDataLogEnable = true;

        string[] inputs;
        int numberOfCells = int.Parse(SkipExtraLogLines(ConsoleReadLine())); // 37
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
            IgnoreExtraLines();
            game.day = int.Parse(SkipExtraLogLines(ConsoleReadLine())); // the game lasts 24 days: 0-23
            game.nutrients = int.Parse(ConsoleReadLine()); // the base score you gain from the next COMPLETE action
            inputs = ConsoleReadLine().Split(' ');
            game.mySun = int.Parse(inputs[0]); // your sun points
            game.myScore = int.Parse(inputs[1]); // your current score
            inputs = ConsoleReadLine().Split(' ');
            game.opponentSun = int.Parse(inputs[0]); // opponent's sun points
            game.opponentScore = int.Parse(inputs[1]); // opponent's score
            game.opponentIsWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day

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
            Console.WriteLine(action);
        }
    }
}
