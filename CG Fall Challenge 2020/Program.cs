using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    struct Spell
    {
        public int Id;
        public bool IsCastable;
        public bool IsRepeatable;
        public List<int> Cost;

        public int WeightedValue
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < Cost.Count; i++)
                {
                    sum += ((i + 1) * Cost[i]);
                }
                return sum;
            }
        }
    }

    struct Learn
    {
        public int Id;
        public int TomeIndex;
        public int TaxCount;
        public List<int> Cost;

        public int WeightedValue
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < Cost.Count; i++)
                {
                    sum += ((i + 1) * Cost[i]);
                }
                return sum;
            }
        }
    }

    struct Order
    {
        public int Id;
        public int Price;
        public int Bonus;
        public int BonusTimes;
        public List<int> Cost;

        public int WeightedProfit(List<int> inventory)
        {
            int wprofit = Price + Bonus;
            for (int i = 0; i < Cost.Count; i++)
            {
                wprofit += ((i + 1) * (inventory[i] + Cost[i]));
            }
            return wprofit;
        }
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
            if (line.StartsWith("Standard"))
            {
                InputLineSkipEnable = true;
                line = ReadLine();
            }
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
                        IsCastable = castable,
                        IsRepeatable = repeatable,
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

        static bool CanCastToInventory(List<int> cost, List<int> inventory)
        {
            var totalcount = inventory.Sum() + cost.Sum();

            if (totalcount <= 0)
                return false;

            if (totalcount > 10)
                return false;

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] + cost[i] < 0)
                    return false;
            }

            return true;
        }

        static int GetWeightedCost(List<int> cost)
        {
            int sum = 0;
            for (int i = 0; i < cost.Count; i++)
            {
                sum += ((i + 1) * cost[i]);
            }
            return sum;
        }

        static Order GetWeightedBestProfitOrder(List<Order> orders, List<int> inventory)
        {
            var bestorder = new Order();
            var maxprofit = -9999;
            foreach (var order in orders)
            {
                int weightedprofit = order.Price;
                for (int i = 0; i < order.Cost.Count; i++)
                {
                    weightedprofit += ((i + 1) * (inventory[i] + order.Cost[i]));
                }

                if (weightedprofit > maxprofit)
                {
                    maxprofit = weightedprofit;
                    bestorder = order;
                }
            }
            return bestorder;
        }

        public static void RotateRight(IList sequence, int count)
        {
            object tmp = sequence[count - 1];
            sequence.RemoveAt(count - 1);
            sequence.Insert(0, tmp);
        }

        public static IEnumerable<IList> Permutate(IList sequence, int count)
        {
            if (count == 1) yield return sequence;
            else
            {
                for (int i = 0; i < count; i++)
                {
                    foreach (var perm in Permutate(sequence, count - 1))
                        yield return perm;
                    RotateRight(sequence, count);
                }
            }
        }

        static IEnumerable<string> GetShortestPathBrewSteps(List<Order> myorders, List<Spell> myspells, List<int> myinventory)
        {
            var steps = new List<string>();
            var spells = new List<Spell>(myspells);
            var inv = new List<int>(myinventory);

            foreach (var permu in Permutate(spells, spells.Count))
            {
                foreach (Spell spell in permu)
                {
                    if (CanCastToInventory(spell.Cost, inv))
                    {
                        //cast the spell
                        for (int i = 0; i < inv.Count; i++)
                            inv[i] += spell.Cost[i];
                        steps.Add("CAST " + spell.Id);
                    }

                    var id = -1;
                    if (CanBrewFromInventory(ref id, myorders, inv))
                    {
                        //push the step
                        steps.Add("BREW " + id);
                    }
                }
            }

            return steps;
        }

        static bool CanBrewFromInventory(ref int brewid, List<Order> orders, List<int> inventory)
        {
            foreach (var order in orders)
            {
                if (CanCastToInventory(order.Cost, inventory))
                {
                    brewid = order.Id;
                    return true;
                }
            }
            return false;
        }

        static bool CanIncreaseTheSmallestItem(ref int spellid, List<Spell> spells, List<int> inventory)
        {
            int smallest_index = 0;
            int smallest_count = 99;

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] < smallest_count)
                {
                    smallest_count = inventory[i];
                    smallest_index = i;
                }
            }

            foreach (var spell in spells)
            {
                if (!spell.IsCastable)
                    continue;

                if (spell.Cost[smallest_index] > 0) // try increse minimum
                {
                    if (CanCastToInventory(spell.Cost, inventory))
                    {
                        spellid = spell.Id;
                        return true;
                    }
                }
            }

            return false;
        }

        public string ExecuteGameLogic()
        {
            var id = -1;
            string action;

            MySpells = MySpells.OrderByDescending(s => s.WeightedValue).ToList();
            OrderList = OrderList.OrderByDescending(s => s.WeightedProfit(MyInventory)).ToList();

            if (CanBrewFromInventory(ref id, OrderList, MyInventory))
            {
                action = "BREW " + id;
            }
            else if (MySpells.Count < 12 && MyLearns.Count > 0)
            {
                id = MyLearns.FirstOrDefault().Id;
                action = "LEARN " + id;
            }
            else if (CanIncreaseTheSmallestItem(ref id, MySpells, MyInventory))
            {
                action = "CAST " + id;
            }
            else
            {
                action = "REST";
            }

            return action;
        }
    }

    static GameState gs;

    static void Main(string[] args)
    {
        gs = new GameState();
        //gs.InputPrintEnable = true;

        while (true)
        {
            gs.ClearLastTurnData();
            gs.ReadInputParams(Console.ReadLine);
            var myaction = gs.ExecuteGameLogic();
            Console.WriteLine(myaction);
        }
    }
}