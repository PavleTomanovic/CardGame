using System;
using Npgsql;
using System.IO;
using System.Collections.Generic;

namespace CardGame
{
    public class BattleField
    {
        public BattleField()
        {
            this.currentRound = 0;
        }

        public void Fight(bool withBot)
        {
            if (withBot)
            {
                fighter2 = new User();
            }

            User winner = null;
            User loser = null;

            StreamWriter sw = new StreamWriter(@"C:\Users\p.tomanovic\Desktop\Fight.txt", true);
            sw.WriteLine("\n--------------------------------------------------------------------------------------------------\n");
            sw.WriteLine(fighter1.Name + " is fighting with " + fighter2.Name + "\n");
            sw.Close();

            bool botInGame = fighter1.BOT || fighter2.BOT;

            while (currentRound < 20)
            {
                Card card1 = fighter1.GetRandomDeckCard(botInGame);
                Card card2 = fighter2.GetRandomDeckCard(botInGame);
                switch (card1.FightWith(card2))
                {
                    case 1:
                        fighter1.PutCardInDeck(card1);
                        fighter1.PutCardInDeck(card2);
                        if (!botInGame)
                        {
                            fighter1.PutCardInStack(card1);
                            fighter1.PutCardInStack(card2);
                        }
                        break;
                    case -1:
                        fighter2.PutCardInDeck(card1);
                        fighter2.PutCardInDeck(card2);
                        if (!botInGame)
                        {
                            fighter2.PutCardInStack(card1);
                            fighter2.PutCardInStack(card2);
                        }
                        break;
                }

                if (fighter1.IsLoser())
                {
                    winner = fighter2;
                    loser = fighter1;
                    break;
                }

                if (fighter2.IsLoser())
                {
                    winner = fighter1;
                    loser = fighter2;
                    break;
                }

                currentRound++;
            }

            sw = new StreamWriter(@"C:\Users\p.tomanovic\Desktop\Fight.txt", true);

            if (winner != null)
            {
                if (!botInGame)
                {
                    winner.ChangeELO(3);
                    loser.ChangeELO(-5);
                }
                

                sw.WriteLine("\n" + winner.Name + " won!\n");
                sw.WriteLine("\n--------------------------------------------------------------------------------------------------");
            }
            else
            {
                sw.WriteLine("\nFight was draw!\n");
            }

            if (!botInGame)
            {
                fighter1.UpdateStackInDataBase();
                fighter2.UpdateStackInDataBase();
            }

            fighter1.ClearDeck();
            fighter2.ClearDeck();
            fighter1 = null;
            fighter2 = null;
            this.currentRound = 0;
            sw.Close();
        }

        public bool BuyCards()
        {
            if (currentUser.NumberOfCoins < 5) return false;

            currentUser.ChangeCoins(-5);
            Random rnd = new Random();
            List<Card> cards = new List<Card>();
            for (int i = 0; i < 5; i++)
            {
                if (i % 2 == 0)
                {
                    // make monster

                    Card card = new Monster((TypeOfCardName)rnd.Next(7), (Type)rnd.Next(3),(int) (rnd.NextDouble() * 100f), -1);
                    cards.Add(card);
                }
                else
                {
                    // make spell

                    Card card = new Spell((TypeOfCardName)rnd.Next(7), (Type)rnd.Next(3),(int) (rnd.NextDouble() * 100f), -1);
                    cards.Add(card);
                }
            }

            currentUser.AddToStack(cards);
            currentUser.UpdateStackInDataBase();

            return true;
        }

        public bool SignUpForFight()
        {
            if (currentUser.NumberOfCardsInStack() == 0)
            {
                Console.WriteLine(@"Your stack is empty, buy some cards");
                return false;
            }
            else if (currentUser.NumberOfCardsInStack() < 4)
            {
                Console.WriteLine(@"You dont have enough cards, please buy more");
                return false;
            }

            Console.WriteLine(@"Your deck is empty, make your deck first");
            currentUser.DefineDeck();

            if (this.fighter1 == null)
            {
                this.fighter1 = currentUser;
                Console.WriteLine(@"No users ready to fight with you, do you want to fight with bot? [Y\N]");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "y":
                    case "Y":
                        Fight(true);
                        return true;
                }
            }
            else
            {
                fighter2 = currentUser;
                Console.WriteLine("Fight starting!");
                Fight(false);
                return true;
            }

            Console.WriteLine("Waiting for other player!");
            return false;
        }

        public bool TradeCards()
        {
            if (currentUser.NumberOfCardsInStack() == 0)
            {
                Console.WriteLine(@"Your stack is empty, buy some cards");
                return false;
            }
            NpgsqlConnection conn = new NpgsqlConnection(@"Server=localhost;Port=5432;User Id=postgres;Password=PtAoVm!994;Database=cardgame;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT name, damage, cardtype, element, id FROM card where user_id = -1;", conn);
            NpgsqlDataReader reader = cmd.ExecuteReader();

            List<Card> tradeCards = new List<Card>();
            while (reader.Read())
            {
                string name = reader[0].ToString();
                string damage = reader[1].ToString();
                string cardtype = reader[2].ToString();
                string element = reader[3].ToString();
                string cardId = reader[4].ToString();

                Card card = null;

                if (cardtype == "0")
                {
                    // Monster
                    card = new Monster((TypeOfCardName)int.Parse(name), (Type)int.Parse(element), int.Parse(damage), int.Parse(cardId));
                }
                else
                {
                    // Spell
                    card = new Spell((TypeOfCardName)int.Parse(name), (Type)int.Parse(element), int.Parse(damage), int.Parse(cardId));
                }

                tradeCards.Add(card);
            }

            conn.Close();
            Tuple<Card, Card> pair = currentUser.TradeCard(tradeCards);

            conn.Open();
            NpgsqlCommand secondCmd = new NpgsqlCommand("UPDATE card SET user_id = @user_id where id = " + pair.Item1.Id.ToString() + ";", conn);
            secondCmd.Parameters.AddWithValue("user_id", currentUser.Id);
            secondCmd.ExecuteReader();
            conn.Close();

            conn.Open();
            NpgsqlCommand thirdCmd = new NpgsqlCommand("UPDATE card SET user_id = @user_id where id = " + pair.Item2.Id.ToString() + ";", conn);
            thirdCmd.Parameters.AddWithValue("user_id", -1);
            thirdCmd.ExecuteReader();
            conn.Close();

            currentUser.UpdateStackInDataBase();
            return true;
        }
        public void ChangeProfile()
        {
            currentUser.ProfileManagement();
        }

        public void PrintStack()
        {
            currentUser.PrintStack();
        }

        public void PrintScores()
        {
            NpgsqlConnection conn = new NpgsqlConnection(@"Server=localhost;Port=5432;User Id=postgres;Password=PtAoVm!994;Database=cardgame;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT elo, username FROM player ORDER BY elo DESC;", conn);

            int i = 1;
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string elo = reader[0].ToString();
                string username = reader[1].ToString();

                Console.WriteLine(i + ". " + username + " ------> " + elo);
                i++;
            }

            conn.Close();
        }

        public bool LogIn()
        {

            Console.WriteLine("Enter username: ");
            string username = Console.ReadLine();
            Console.WriteLine("Enter password: ");
            string password = Console.ReadLine();
            string encryptPassword = Crypto.EncryptPassword(password);

            NpgsqlConnection conn = new NpgsqlConnection(@"Server=localhost;Port=5432;User Id=postgres;Password=PtAoVm!994;Database=cardgame;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("SELECT id, username, password, coins, elo FROM player where username = @username and password = @password;", conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", encryptPassword);

            var result = cmd.ExecuteScalar();
            if (result != null)
            { 
                NpgsqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                
                string newId = reader[0].ToString();
                string newUsername = reader[1].ToString();
                string newPassword = reader[2].ToString();
                string newCoins = reader[3].ToString();
                string newElo = reader[4].ToString();

                conn.Close();
                conn.Open();

                NpgsqlCommand secondCmd = new NpgsqlCommand("SELECT id, name, damage, cardtype, element, indeck FROM card WHERE user_id = @user_id;", conn);
                secondCmd.Parameters.AddWithValue("user_id", int.Parse(newId));

                List<Card> stack = new List<Card>();
                List<Card> deck = new List<Card>();

                NpgsqlDataReader secondReader = secondCmd.ExecuteReader();
                while (secondReader.Read())
                {
                    string cardId = secondReader[0].ToString();
                    string name = secondReader[1].ToString();
                    string damage = secondReader[2].ToString();
                    string cardtype = secondReader[3].ToString();
                    string element = secondReader[4].ToString();
                    string indeck = secondReader[5].ToString();

                    Card card = null;

                    if (cardtype == "0")
                    {
                        // Monster
                        card = new Monster((TypeOfCardName)int.Parse(name), (Type)int.Parse(element), int.Parse(damage), int.Parse(cardId));
                    }
                    else
                    {
                        // Spell
                        card = new Spell((TypeOfCardName)int.Parse(name), (Type)int.Parse(element), int.Parse(damage), int.Parse(cardId));
                    }

                    stack.Add(card);
                    if (bool.Parse(indeck) == true)
                    {
                        deck.Add(card);
                    }
                }
                
                conn.Close();

                Console.WriteLine("Hello " + newUsername + "!");
                Console.WriteLine("You have " + newCoins + " coins to buy new cards.");
                Console.WriteLine("Your score is " + newElo + ". Try to beat every player. To become the best player in the game!");
                User user = new User(stack, deck, int.Parse(newCoins),  newUsername, encryptPassword, int.Parse(newElo), int.Parse(newId));
                currentUser = user;

                return true;
            }

                
            Console.WriteLine("Credentials are not right!");
            return false;
        }

        public bool Register()
        {
            Console.WriteLine("Enter username: ");
            string username = Console.ReadLine();
            Console.WriteLine("Enter password: ");
            string password = Console.ReadLine();
            string encryptPassword = Crypto.EncryptPassword(password);

            NpgsqlConnection conn = new NpgsqlConnection(@"Server=localhost;Port=5432;User Id=postgres;Password=PtAoVm!994;Database=cardgame;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("SELECT username FROM player where username = @username;", conn);
            cmd.Parameters.AddWithValue("username", username);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                Console.WriteLine("User already exists! Please choose another username!");
                return false;
            }
            else
            {
                NpgsqlCommand secondCmd = new NpgsqlCommand("INSERT INTO player (username,password,coins,elo) VALUES (@username, @password, 20, 100);", conn);
                secondCmd.Parameters.AddWithValue("username", username);
                secondCmd.Parameters.AddWithValue("password", encryptPassword);
                secondCmd.ExecuteReader();
                conn.Close();

                conn.Open();
                NpgsqlCommand thirdCmd = new NpgsqlCommand("SELECT id FROM player where username = @username;", conn);
                thirdCmd.Parameters.AddWithValue("username", username);
                NpgsqlDataReader secondReader = thirdCmd.ExecuteReader();
                secondReader.Read();
                string userId = secondReader[0].ToString();
                conn.Close();

                User user = new User(username, encryptPassword, int.Parse(userId));
                Console.WriteLine("Hello " + username + "!");
                Console.WriteLine("You have 20 coins to buy new cards.");
                Console.WriteLine("Your score is 100. Try to beat every player. To become the best player in the game!");
                currentUser = user;
                return true;
            }
            
        }

        public void LogOut()
        {
            currentUser = null;
        }

        private User fighter1 = null;
        private User fighter2 = null;
        private User currentUser;
        private int currentRound;
    }
}
