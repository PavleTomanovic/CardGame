using System;
using System.IO;

namespace CardGame
{
    public class Monster : Card
    {
        public Monster(TypeOfCardName name, Type type, int damage, int ID) : base(name, type, damage, ID)
        {
        }

        override
        public string ToString()
        {
            return "Monster " + base.ToString();
        }

        override
        public string GetName()
        {
            return type.ToString() + " " + name.ToString();
        }

        override
        public int FightWith(Card card)
        {
            int win = 0;

            double myDamage = damage;
            double opDamage = card.GetDamage();

            if (card is Spell)
            {

                switch (type)
                {
                    case Type.Fire:
                        switch (card.GetCardType())
                        {
                            case Type.Normal:
                                myDamage *= 2;
                                opDamage /= 2;
                                break;
                            case Type.Water:
                                myDamage /= 2;
                                opDamage *= 2;
                                break;
                        }
                        break;

                    case Type.Normal:
                        switch (card.GetCardType())
                        {
                            case Type.Water:
                                myDamage *= 2;
                                opDamage /= 2;
                                break;
                            case Type.Fire:
                                myDamage /= 2;
                                opDamage *= 2;
                                break;
                        }
                        break;

                    case Type.Water:
                        switch (card.GetCardType())
                        {
                            case Type.Fire:
                                myDamage *= 2;
                                opDamage /= 2;
                                break;
                            case Type.Normal:
                                myDamage /= 2;
                                opDamage *= 2;
                                break;
                        }
                        break;
                }
            }

            string description = this.name + " is draw with " + card.GetName();

            if (myDamage < opDamage && Card.IsSpecialCase(this, card) != 1)
            {
                description = this.GetName() + " is defeted by " + card.GetName();
                win = -1;
            }
            else if (myDamage > opDamage && Card.IsSpecialCase(this, card) != -1)
            {
                description = this.GetName() + " won against " + card.GetName();
                win = 1;
            }

            StreamWriter sw = new StreamWriter(@"C:\Users\p.tomanovic\Desktop\Fight.txt", true);
            sw.WriteLine(description);
            sw.Close();

            return win;
        }
    }
}
