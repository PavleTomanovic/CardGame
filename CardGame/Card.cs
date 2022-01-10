using System;
namespace CardGame
{
    public abstract class Card
    {
        public int Id { get { return ID; } }
        public int Name { get { return (int)name; } }
        public int Damage { get { return damage; } }
        public int MyType { get { return (int)type; } }
        public Card(TypeOfCardName name, Type type, int damage, int ID)
        {
            this.name = name;
            this.type = type;
            this.damage = damage;
            this.ID = ID;
        }

        public static int IsSpecialCase(Card first, Card second)
        {
            if (first.name == TypeOfCardName.Goblin && second.name == TypeOfCardName.Dragoon)
            {
                return -1;
            }
            if (second.name == TypeOfCardName.Goblin && first.name == TypeOfCardName.Dragoon)
            {
                return 1;
            }


            if (first.name == TypeOfCardName.Ork && second.name == TypeOfCardName.Wizzard)
            {
                return -1;
            }
            if (second.name == TypeOfCardName.Ork && first.name == TypeOfCardName.Wizzard)
            {
                return 1;
            }


            if (first.type == Type.Water && first is Spell && second.name == TypeOfCardName.Knight)
            {
                return 1;
            }
            if (second.type == Type.Water && second is Spell && first.name == TypeOfCardName.Knight)
            {
                return -1;
            }


            if (first.name == TypeOfCardName.Kraken && second is Spell)
            {
                return 1;
            }
            if (second.name == TypeOfCardName.Kraken && first is Spell)
            {
                return -1;
            }


            if (first.name == TypeOfCardName.Elf && first.type == Type.Fire && second.name == TypeOfCardName.Dragoon)
            {
                return 1;
            }
            if (second.name == TypeOfCardName.Elf && second.type == Type.Fire && first.name == TypeOfCardName.Dragoon)
            {
                return -1;
            }

            return 0;
        }

        override
        public string ToString()
        {
            string nameForPrint = this is Spell ? "" : name.ToString();
            return nameForPrint + " " + type.ToString() + " damage: " + damage;
        }

        public abstract int FightWith(Card card);
        public abstract string GetName();

        public int GetDamage()
        {
            return damage;
        }

        public Type GetCardType()
        {
            return type;
        }

        public void SetNewID(int newID)
        {
            ID = newID;
        }

        protected TypeOfCardName name;
        protected int damage;
        protected int attack;
        protected Type type;
        protected int ID;

    }
}
