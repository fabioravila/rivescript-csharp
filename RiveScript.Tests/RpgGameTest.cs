using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class RpgGameTest
    {
        [TestMethod]
        public void RPG_Full_Test()
        {
            var rs = TestHelper.getStreamed(new[] {"/*",
                                                        "    Topic Inheritence (simple roleplaying game)",
                                                        "    -------------------------------------------",
                                                        "    Human says:     enter the dungeon",
                                                        "    Expected reply: (drops you into a mini game. Skim the code below to figure",
                                                        "                    it out)",
                                                        "*/",

                                                        "+ enter the dungeon",
                                                        "- {topic=room1}You've entered the dungeon. {@look}",

                                                        "> topic global",
                                                        "	+ help{weight=100}",
                                                        "	- Game Help (todo)",

                                                        "	+ inventory{weight=100}",
                                                        "	- Your Inventory (todo)",

                                                        "	+ (north|n|south|s|east|e|west|w)",
                                                        "	- You can't go in that direction.",

                                                        "	+ quit{weight=100}",
                                                        "	- {topic=random}Quitter!",

                                                        "	+ _ *",
                                                        "	- You don't need to use the word \"<star>\" in this game.",

                                                        "	+ *",
                                                        "	- I don't understand what you're saying. Try \"help\" or \"quit\".",
                                                        "< topic",

                                                        "> topic dungeon inherits global",
                                                        "	+ hint",
                                                        "	- What do you need a hint on?\n",
                                                        "	^ * How to play\n",
                                                        "	^ * About this game",

                                                        "	+ how to play",
                                                        "	% what do you need a hint *",
                                                        "	- The commands are \"help\", \"inventory\", and \"quit\". Just read and type.",

                                                        "	+ about this game",
                                                        "	% what do you need a hint *",
                                                        "	- This is just a sample RPG game to demonstrate topic inheritence.",
                                                        "< topic",

                                                        "> topic room1 inherits dungeon",
                                                        "	+ look",
                                                        "	- You're in a room with a large number \"1\" on the floor.\\s",
                                                        "	^ Exits are north and east.",

                                                        "	+ (north|n){weight=5}",
                                                        "	- {topic=room2}{@look}",

                                                        "	+ (east|e){weight=5}",
                                                        "	- {topic=room3}{@look}",
                                                        "< topic",

                                                        "> topic room2 inherits dungeon",
                                                        "	+ look",
                                                        "	- This room has the number \"2\" here. There's a flask here that's trapped",
                                                        "	^ \\sin some kind of mechanism that only opens while the button is held",
                                                        "	^ \\sdown (so, hold down the button then quickly grab the flask).\n\n",
                                                        "	^ The only exit is to the south.",

                                                        "	+ [push|press|hold] button [*]",
                                                        "	- You press down on the button and the mechanism holding the flask is\\s",
                                                        "	^ unlocked.",

                                                        "	+ [take|pick up|grab] [ye] flask [*]",
                                                        "	% * mechanism holding the flask is unlocked",
                                                        "	- You try to take ye flask but fail (you can't take ye flask, give up).",

                                                        "	+ [take|pick up|grab] [ye] flask [*]",
                                                        "	- You can't get ye flask while the mechanism is holding onto it.",

                                                        "	+ (south|s){weight=5}",
                                                        "	- {topic=room1}{@look}",
                                                        "< topic",

                                                        "> topic room3 inherits dungeon",
                                                        "	+ look",
                                                        "	- There's nothing here but the number \"3\". Only exit is to the west.",

                                                        "	+ (west|w){weight=5}",
                                                        "	- {topic=room1}{@look}",
                                                        "< topic"});



            //room1
            rs.reply("enter the dungeon")
              .AssertAreEqual("You've entered the dungeon. You're in a room with a large number \"1\" on the floor. Exits are north and east.");

            rs.reply("help").AssertAreEqual("Game Help (todo)");
            rs.reply("inventory").AssertAreEqual("Your Inventory (todo)");
            rs.reply("s").AssertAreEqual("You can't go in that direction.");
            rs.reply("w").AssertAreEqual("You can't go in that direction.");

            //room3
            rs.reply("e")
              .AssertAreEqual("There's nothing here but the number \"3\". Only exit is to the west.");
            rs.reply("help")
              .AssertAreEqual("Game Help (todo)");
            rs.reply("n")
              .AssertAreEqual("You can't go in that direction.");
            rs.reply("s")
              .AssertAreEqual("You can't go in that direction.");
            rs.reply("e")
              .AssertAreEqual("You can't go in that direction.");

            //room2
            rs.reply("w")
              .AssertAreEqual("You're in a room with a large number \"1\" on the floor. Exits are north and east.");
            rs.reply("s")
              .AssertAreEqual("You can't go in that direction.");
            rs.reply("w")
              .AssertAreEqual("You can't go in that direction.");

           //room3
           rs.reply("n")
             .AssertAreEqual("This room has the number \"2\" here. There's a flask here that's trapped in some kind of mechanism that only opens while the button is held down (so, hold down the button then quickly grab the flask).\n\nThe only exit is to the south.");

            rs.reply("n")
              .AssertAreEqual("You can't go in that direction.");

            rs.reply("e")
              .AssertAreEqual("You can't go in that direction.");

            rs.reply("w")
              .AssertAreEqual("You can't go in that direction.");

            rs.reply("help")
              .AssertAreEqual("Game Help (todo)");

            rs.reply("grab flask")
              .AssertAreEqual("You can't get ye flask while the mechanism is holding onto it.");

            rs.reply("press button")
              .AssertAreEqual("You press down on the button and the mechanism holding the flask is unlocked.");

            rs.reply("hold button")
              .AssertAreEqual("You press down on the button and the mechanism holding the flask is unlocked.");

            rs.reply("push button")
              .AssertAreEqual("You press down on the button and the mechanism holding the flask is unlocked.");

            rs.reply("take flask")
              .AssertAreEqual("You try to take ye flask but fail (you can't take ye flask, give up).");

            rs.reply("get out")
               .AssertAreEqual("You don't need to use the word \"get\" in this game.");


            rs.reply("something")
              .AssertAreEqual("I don't understand what you're saying. Try \"help\" or \"quit\".");

            rs.reply("quit")
              .AssertAreEqual("Quitter!");
        }
    }
}