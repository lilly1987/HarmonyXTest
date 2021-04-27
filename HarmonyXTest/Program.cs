using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using MonoMod.Cil;

namespace HarmonyXTest
{
    class Original
    {
        public static int RollDice()
        {
            int s = 0;
            for (int i = 0; i < 40; i++)
            {
                s += i;
            }
            return s;
        }
    }
    class TestClass
    {
        public static string SomeMethod(string a, string b)
        {
            return a + " something " + b;
        }
    }

    class HarmonyPatchTest
    {
        //[HarmonyPatch(typeof(TestClass), "SomeMethod")] // Specify target method with HarmonyPatch attribute
        //[HarmonyPostfix]                              // There are different patch types. Prefix code runs before original code
        static void RollRealDice(ref string __result)
        {
            // https://xkcd.com/221/
            __result += " add test"; // The special __result variable allows you to read or change the return value
            //return false; // Returning false in prefix patches skips running the original code
        }

        //[HarmonyPatch(typeof(Original), "RollDice")] // Specify target method with HarmonyPatch attribute
        //[HarmonyPostfix]                              // There are different patch types. Prefix code runs before original code
        static void RollRealDice(ref int __result)
        {
            // https://xkcd.com/221/
            __result += 4; // The special __result variable allows you to read or change the return value
            //return false; // Returning false in prefix patches skips running the original code
        }

        [HarmonyPatch(typeof(TestClass), "SomeMethod")]
        [HarmonyILManipulator]
        public static void SomeOtherILManipulator(ILContext ctx, MethodBase orig) // parameter names can be anything, all parameters are optional
        {
            ILCursor c = new ILCursor(ctx);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdstr(" something ")
            );

            c.Next.Operand = " SomeOtherILManipulator ";
        } 
                
        [HarmonyPatch(typeof(Original), "RollDice")]
        [HarmonyILManipulator]
        public static void RollRealDiceILManipulator(ILContext ctx, MethodBase orig) // parameter names can be anything, all parameters are optional
        {

            ILCursor c = new ILCursor(ctx);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdcI4(40)
            );
            c.Remove();
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, 10);

        } 

    }

    class Program
    {
      
        static void Main(string[] args)
        {
            Log("start");


            Log(TestClass.SomeMethod("asd","efg"));
            Console.WriteLine($"Random roll: {Original.RollDice()}"); // Prints: "Random roll: <some number between 1 and 6>"
            try
            {
                // Actual patching is just a one-liner!
                Harmony.CreateAndPatchAll(typeof(HarmonyPatchTest));
            }
            catch (Exception e)
            {
                Log("err : "+e.ToString());
            }

            Log(TestClass.SomeMethod("asd","efg"));
            Console.WriteLine($"Random roll: {Original.RollDice()}"); // Will always print "Random roll: 4"


            End();
        }

        static void Log<T>(T o)
        {
            Console.WriteLine(o);
        }

        static void End()
        {
            Console.ReadKey();
        }



    }
}
