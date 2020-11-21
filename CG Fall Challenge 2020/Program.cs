﻿using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    struct Spell
    {
        public int Id;
        public bool Castable;
        public bool Repeatable;
        public List<int> Cost;
    }

    struct Learn
    {
        public int Id;
        public int TomeIndex;
        public int TaxCount;
        public List<int> Cost;
    }

    struct Order
    {
        public int Id;
        public int Price;
        public int Bonus;
        public int BonusTimes;
        public List<int> Cost;
    }

    class GameState
    {
        public int MyScore { get; set; }
        public int OppScore { get; set; }
        public List<Spell> MySpells { get; set; }
        public List<Learn> MyLearns { get; set; }
        public List<Order> OrderList { get; set; }
        public List<int> MyInventory { get; set; }

        public bool InputPrintEnable { get; set; }
        public bool InputLineSkipEnable { get; set; }
        public delegate string ReadLineFunc();

        public GameState()
        {
            MySpells = new List<Spell>();
            MyLearns = new List<Learn>();
            OrderList = new List<Order>();
            MyInventory = new List<int> { 0, 0, 0, 0 };
        }

        public void ClearLastTurnData()
        {
            MySpells.Clear();
            MyLearns.Clear();
            OrderList.Clear();
        }

        public void DebugRawData(string line)
        {
            if (InputPrintEnable)
            {
                Console.Error.WriteLine(line);
            }
        }

        public void ReadInputParams(ReadLineFunc ReadLine)
        {
            string[] inputs;
            var line = ReadLine();
            if (line.StartsWith("Standard")) line = ReadLine();
            gs.DebugRawData(line);

            int actionCount = int.Parse(line); // the number of spells and recipes in play
            for (int i = 0; i < actionCount; i++)
            {
                line = ReadLine();
                gs.DebugRawData(line);
                inputs = line.Split(' ');
                int actionId = int.Parse(inputs[0]); // the unique ID of this spell or recipe
                string actionType = inputs[1]; // in the first league: BREW; later: CAST, OPPONENT_CAST, LEARN, BREW
                int delta0 = int.Parse(inputs[2]); // tier-0 ingredient change
                int delta1 = int.Parse(inputs[3]); // tier-1 ingredient change
                int delta2 = int.Parse(inputs[4]); // tier-2 ingredient change
                int delta3 = int.Parse(inputs[5]); // tier-3 ingredient change
                int price = int.Parse(inputs[6]); // the price in rupees if this is a potion
                int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax; For brews, this is the value of the current urgency bonus
                int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell; For brews, this is how many times you can still gain an urgency bonus
                bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
                bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell

                if (actionType == "BREW")
                    gs.OrderList.Add(new Order
                    {
                        Id = actionId,
                        Price = price,
                        Bonus = tomeIndex,
                        BonusTimes = taxCount,
                        Cost = new List<int> { delta0, delta1, delta2, delta3 }
                    });
                else if (actionType == "CAST")
                    gs.MySpells.Add(new Spell
                    {
                        Id = actionId,
                        Castable = castable,
                        Repeatable = repeatable,
                        Cost = new List<int> { delta0, delta1, delta2, delta3 }
                    });
                else if (actionType == "LEARN")
                    gs.MyLearns.Add(new Learn
                    {
                        Id = actionId,
                        TomeIndex = tomeIndex,
                        TaxCount = taxCount,
                        Cost = new List<int> { delta0, delta1, delta2, delta3 }
                    });
            }
            for (int i = 0; i < 2; i++)
            {
                line = ReadLine();
                gs.DebugRawData(line);
                inputs = line.Split(' ');
                int inv0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory
                int inv1 = int.Parse(inputs[1]);
                int inv2 = int.Parse(inputs[2]);
                int inv3 = int.Parse(inputs[3]);
                int score = int.Parse(inputs[4]); // amount of rupees
                if (i == 0)
                {
                    gs.MyInventory = new List<int> { inv0, inv1, inv2, inv3 };
                    gs.MyScore = score;
                }
                else if (i == 1)
                {
                    gs.OppScore = score;
                }
            }

            // skipp copied input log extra data
            if (InputLineSkipEnable)
            {
                ReadLine();
                ReadLine();
            }
        }

        bool CanExpenseFromInventory(List<int> cost)
        {
            if (MyInventory.Sum() + cost.Sum() < 0)
                return false;

            for (int i = 0; i < MyInventory.Count; i++)
            {
                if (MyInventory[i] + cost[i] < 0)
                    return false;
            }

            return true;
        }

        bool CanAppendToInventory(List<int> cost)
        {
            return MyInventory.Sum() + cost.Sum() <= 10;
        }

        int GetWeightedCost(List<int> cost)
        {
            int sum = 0;
            for (int i = 0; i < cost.Count; i++)
            {
                sum += ((i + 1) * cost[i]);
            }
            return sum;
        }

        Order GetWeightedBestProfitOrder()
        {
            var bestorder = new Order();
            var maxprofit = -9999;
            foreach (var order in OrderList)
            {
                int weightedprofit = order.Price;
                for (int i = 0; i < order.Cost.Count; i++)
                {
                    weightedprofit += ((i + 1) * (MyInventory[i] + order.Cost[i]));
                }

                if (weightedprofit > maxprofit)
                {
                    maxprofit = weightedprofit;
                    bestorder = order;
                }
            }
            return bestorder;
        }

        IEnumerable<string> GetStepsToBrewOrder(Order order)
        {
            var netcost = new int[4];
            for (int i = 0; i < order.Cost.Count; i++)
            {
                netcost[i] += MyInventory[i] + order.Cost[i];
            }

            var steps = new List<string>();
            for (int i = 0; i < netcost.Length; i++)
            {
                if (netcost[i] >= 0)
                    continue;

                //ToDo: need to implement
            }

            return steps;
        }

        bool CanBrewFromInventory(ref int brewid)
        {
            foreach (var order in OrderList)
            {
                if (CanExpenseFromInventory(order.Cost))
                {
                    brewid = order.Id;
                    return true;
                }
            }
            return false;
        }

        bool CanCastFromSpells(ref int spellid)
        {
            int smallest_index = 0;
            int smallest_count = 99;

            for (int i = 0; i < MyInventory.Count; i++)
            {
                if (MyInventory[i] < smallest_count)
                {
                    smallest_count = MyInventory[i];
                    smallest_index = i;
                }
            }

            foreach (var spell in MySpells)
            {
                if (!spell.Castable)
                    continue;

                if (spell.Cost[smallest_index] > 0) // try increse minimum
                {
                    if (CanExpenseFromInventory(spell.Cost) &&
                        CanAppendToInventory(spell.Cost))
                    {
                        spellid = spell.Id;
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanLearnFromTome(ref int learnid)
        {
            foreach (var learn in MyLearns)
            {
                if (CanExpenseFromInventory(learn.Cost))
                {
                    learnid = learn.Id;
                    return true;
                }
            }
            return false;
        }

        bool needrest;
        int learncount;
        int turn_count;

        public string ExecuteGameLogic()
        {
            turn_count++;
            var id = -1;
            string action;

            if (CanBrewFromInventory(ref id))
            {
                action = "BREW " + id;
                learncount = 0;
            }
            else if (learncount < 3 && MyLearns.Count > 0)
            {
                id = MyLearns.FirstOrDefault().Id;
                action = "LEARN " + id;
                learncount++;
            }
            else if (CanCastFromSpells(ref id))
            {
                action = "CAST " + id;
                needrest = true;
            }
            else if (needrest)
            {
                action = "REST";
                needrest = false;
            }
            else
            {
                action = "WAIT";
            }

            return action;
        }
    }

    static GameState gs;

    static void Main(string[] args)
    {
        gs = new GameState();
        //gs.InputPrintEnable = true;
        //gs.InputLineSkipEnable = true;

        while (true)
        {
            gs.ClearLastTurnData();
            gs.ReadInputParams(Console.ReadLine);
            var myaction = gs.ExecuteGameLogic();
            Console.WriteLine(myaction);
        }
    }
}