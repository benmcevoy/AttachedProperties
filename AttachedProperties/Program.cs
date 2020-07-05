using System;
using System.Text;

namespace AttachedProperties
{
    class Program
    {
        static void Main(string[] args)
        {

            #region test1

            var x = new MyObject1 {Name = "I only have one property"};
            var y = new MyObject1 {Name = "I am another instance"};

            var am = new AttachedPropertyManager2();

            am.Set(x, "MyAttachedProp", "Well hello there!");

            var result1 = am.Get(x, "MyAttachedProp");
            var result2 = am.Get(y, "MyAttachedProp");

            Console.WriteLine($"result1 should be \"Well hello there!\" : '{result1}'");
            Console.WriteLine($"result2 should be null or something : '{result2}'");

            #endregion

            x.GetVirtualPropertyThing();

            var x3 = new AttachedPropertyManager3();

            NewMethod(x3);

            GC.Collect();

            x3.ToString();
         
        }

        private static void NewMethod(AttachedPropertyManager3 x3)
        {
            //x3.Set(x, ()=>x.Name , "");
            {
                var garbage = new MyObject1 {Name = "GC Collect me"};

                x3.Set(garbage, "Test", new StringBuilder().Append("some stuff in here"));

                Console.WriteLine(x3.Get(garbage, "Test"));
            }
        }
    }

    public class MyObject1
    {
        public string Name { get; set; }
    }

    public static class MyObjectEx
    {
        static readonly AttachedPropertyManager2 AM = new AttachedPropertyManager2();

        public static string GetVirtualPropertyThing(this MyObject1 owner) => (string)AM.Get(owner, nameof(GetVirtualPropertyThing));
    }
}
