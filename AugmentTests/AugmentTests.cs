using System;
using System.Linq;
using NUnit.Framework;

namespace AugmentTests
{
    public class AugmentTests
    {
        [Test]
        public void CanSetAndGet()
        {
            // arrange
            var sut = new object();

            //  act
            Augment.Instance.Set(sut, "MyMember", "this is my value");
            var result = Augment.Instance.Get<string>(sut, "MyMember");

            // assert
            Assert.AreEqual("this is my value", result);
        }

        [Test]
        public void SettingToADifferentTypeIsNotAllowed()
        {
            // arrange
            var sut = new object();
            Augment.Instance.Set(sut, "MyMember", "this is my value");

            // act & assert
            Assert.Throws<InvalidOperationException>(() => sut._Set("MyMember", 555));
        }

        [Test]
        public void MutatingAValueIsOk()
        {
            // arrange
            var sut = new object();
            sut._Set("MyMember", "this is my value");

            // act
            sut._Set("MyMember", "this is my NEW value");
            var result = sut._Get<string>("MyMember");

            // assert
            Assert.AreEqual("this is my NEW value", result);
        }

        [Test]
        public void TryGetDoesNotThrow()
        {
            // arrange
            var sut = new object();

            // act
            var result = sut._TryGet("ThisDoesNotExistsButThatsOk", out string x);

            // assert
            Assert.False(result);
        }

        [Test]
        public void TryGetCanGet()
        {
            // arrange
            var sut = new object();

            // act
            Augment.Instance.Set(sut, "MyMember", "this is my value");
            var result = Augment.Instance.TryGet(sut, "MyMember", out string x);

            // assert
            Assert.True(result);
            Assert.AreEqual("this is my value", x);
        }

        [Test]
        public void WhenPropertyNotAttachedHasIsFalse()
        {
            // arrange
            var sut = new object();

            //  act
            var result = Augment.Instance.Has(sut, "MyMember");

            // assert
            Assert.False(result);
        }

        [Test]
        public void WhenPropertyAttachedHasIsTrue()
        {
            // arrange
            var sut = new object();
            Augment.Instance.Set(sut, "MyMember", "this is my value");

            //  act
            var result = sut._Has("MyMember");

            // assert
            Assert.True(result);
        }

        [Test]
        public void GarbageCollectionCleansUp()
        {
            // arrange
            CanSetAndGet();

            // internals have been made visible to us
            foreach (var x in Augment.Instances.Keys)
            {
                Assert.IsTrue(x.Owner.IsAlive);
                break;
            }

            // act
            GC.Collect();
            // when this runs is non-deterministic, so just run it for testing 
            Augment.Compact();

            // assert
            foreach (var (key, value) in Augment.Instances)
            {
                Assert.IsFalse(key.Owner.IsAlive);
                Assert.IsNull(value);
                break;
            }
        }

        [Test]
        public void SetPrototypeAndGetInstance()
        {
            // arrange
            var aCompletelyDifferentObject = new object();
            var sut = new object();

            //  act
            aCompletelyDifferentObject._SetPrototype("MyBaseMember", "this is my value");

            var result = Augment.Instance.Get<string>(sut, "MyBaseMember");

            // assert
            Assert.AreEqual("this is my value", result);
        }

        [Test]
        public void ShadowPrototypeAndGetInstance()
        {
            // arrange
            var sut = new object();

            //  act
            sut._SetPrototype("MyBaseMember", "this is my value");
            sut._Set("MyBaseMember", "this is the new value");

            var result = Augment.Instance.Get<string>(sut, "MyBaseMember");

            // assert
            Assert.AreEqual("this is the new value", result);
        }


        [Test]
        public void InstanceHasPrototypeMember()
        {
            // arrange
            var aCompletelyDifferentObject = new object();

            aCompletelyDifferentObject._SetPrototype("MyBaseMember", "this is my value");

            var sut = new object();

            //  act
            var result = sut._Has("MyBaseMember");
            
            // assert
            Assert.True(result);
        }

        [Test]
        public void EntriesContainsInstanceAndPrototypeMembers()
        {
            // arrange
            var aCompletelyDifferentObject = new object();

            aCompletelyDifferentObject._SetPrototype("MyBaseMember", "this is my value");

            var sut = new object();

            sut._Set("MyInstanceMember", 56f);

            // act
            var result = sut._Entries().ToArray();

            // assert
            Assert.Contains("MyBaseMember", result);
            Assert.Contains("MyInstanceMember", result);
        }
    }
}
