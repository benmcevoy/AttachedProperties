using System;
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
            foreach (var x in Augment.State.Keys)
            {
                Assert.IsTrue(x.Owner.IsAlive);
                break;
            }

            // act
            GC.Collect();
            // when this runs is non-deterministic, so just run it for testing 
            Augment.Compact();

            // assert
            foreach (var (key, value) in Augment.State)
            {
                Assert.IsFalse(key.Owner.IsAlive);
                Assert.IsNull(value);
                break;
            }
        }
    }
}
