using System;
using NUnit.Framework;

namespace Demegraunt.Framework.Tests {
    public sealed class BuffTests {
        [Test]
        public void Calculate_WithoutProcessors_ReturnsOriginalValue() {
            Assert.AreEqual(5f, new Buff<float>(5f).Calculate());
        }

        [Test]
        public void Calculate_UsesOriginalValueCallbackEveryTime() {
            var original = 1f;
            var buff = new Buff<float>(() => original);

            original = 3f;

            Assert.AreEqual(3f, buff.Calculate());
        }

        [Test]
        public void Processors_WithSameOrder_AreAppliedInInsertionOrder_AfterRemovals() {
            var buff = new Buff<string>("");
            var a = buff.Add(value => value + "A");
            buff.Add(value => value + "B");
            buff.Add(value => value + "C");

            a.Remove();
            buff.Add(value => value + "D");

            Assert.AreEqual("BCD", buff.Calculate());
        }

        [Test]
        public void Processors_AreSortedByOrder() {
            var buff = new Buff<string>("");
            buff.Add(value => value + "3", 30);
            buff.Add(value => value + "1", 10);
            buff.Add(value => value + "2", 20);
            buff.Add(value => value + "1b", 10);

            Assert.AreEqual("11b23", buff.Calculate());
        }

        [Test]
        public void BuiltInProcessors_ApplyAddersBeforeMultipliers() {
            var buff = new Buff<float>(10f);
            buff.Add(new FloatBuffMultiplier(2f));
            buff.Add(new FloatBuffAdder(5f));

            Assert.AreEqual(30f, buff.Calculate());
        }

        [Test]
        public void ExplicitOrder_OverridesDefaultOrder() {
            var buff = new Buff<float>(10f);
            buff.Add(new FloatBuffMultiplier(2f));
            buff.Add(new FloatBuffAdder(5f), BuffOrder.Final);

            Assert.AreEqual(25f, buff.Calculate());
        }

        [Test]
        public void Add_DuplicateId_Throws() {
            var buff = new Buff<int>(0);
            var id = Guid.NewGuid();
            buff.Add(id, value => value + 1);

            Assert.Throws<ArgumentException>(() => buff.Add(id, value => value + 2));
            Assert.AreEqual(1, buff.Calculate());
        }

        [Test]
        public void Add_Null_Throws() {
            var buff = new Buff<int>(0);

            Assert.Throws<ArgumentNullException>(() => buff.Add(Guid.NewGuid(), (BuffProcessor<int>)null));
            Assert.Throws<ArgumentNullException>(() => buff.Add((Func<int, int>)null));
        }

        [Test]
        public void TryAdd_DuplicateId_ReturnsFalse() {
            var buff = new Buff<int>(0);
            var id = Guid.NewGuid();

            Assert.IsTrue(buff.TryAdd(id, value => value + 1));
            Assert.IsFalse(buff.TryAdd(id, value => value + 2));
            Assert.AreEqual(1, buff.Calculate());
        }

        [Test]
        public void Replace_KeepsPositionAndOrder() {
            var buff = new Buff<string>("");
            var id = Guid.NewGuid();
            buff.Add(value => value + "A");
            buff.Add(id, value => value + "B");
            buff.Add(value => value + "C");

            buff.Replace(id, value => value + "X");

            Assert.AreEqual("AXC", buff.Calculate());
        }

        [Test]
        public void Replace_MissingId_Adds() {
            var buff = new Buff<int>(1);
            var id = Guid.NewGuid();

            buff.Replace(id, value => value * 10);

            Assert.IsTrue(buff.Contains(id));
            Assert.AreEqual(10, buff.Calculate());
        }

        [Test]
        public void Remove_ReturnsWhetherRemoved() {
            var buff = new Buff<int>(1);
            var id = Guid.NewGuid();
            buff.Add(id, value => value + 1);

            Assert.IsTrue(buff.Remove(id));
            Assert.IsFalse(buff.Remove(id));
            Assert.AreEqual(1, buff.Calculate());
        }

        [Test]
        public void Handle_Dispose_RemovesProcessor() {
            var buff = new Buff<int>(1);
            var handle = buff.Add(value => value + 1);
            Assert.IsTrue(handle.IsActive);

            handle.Dispose();
            handle.Dispose();

            Assert.IsFalse(handle.IsActive);
            Assert.AreEqual(0, buff.Count);
            Assert.AreEqual(1, buff.Calculate());
        }

        [Test]
        public void DefaultHandle_IsSafe() {
            var handle = default(BuffHandle);

            Assert.IsFalse(handle.IsActive);
            Assert.IsFalse(handle.Remove());
            handle.Dispose();
        }

        [Test]
        public void RemoveBySource_RemovesOnlyThatSource() {
            var buff = new Buff<int>(0);
            var sword = new object();
            var ring = new object();
            buff.Add(new IntBuffAdder(1), source: sword);
            buff.Add(new IntBuffAdder(10), source: ring);
            buff.Add(new IntBuffAdder(100), source: sword);

            Assert.AreEqual(2, buff.RemoveBySource(sword));
            Assert.AreEqual(10, buff.Calculate());
        }

        [Test]
        public void RemoveProcessor_RemovesAllRegistrations() {
            var buff = new Buff<int>(0);
            var processor = new IntBuffAdder(1);
            buff.Add(processor);
            buff.Add(processor);
            buff.Add(new IntBuffAdder(10));

            Assert.AreEqual(2, buff.RemoveProcessor(processor));
            Assert.AreEqual(10, buff.Calculate());
        }

        [Test]
        public void Clear_RemovesEverything() {
            var buff = new Buff<int>(3);
            buff.Add(new IntBuffAdder(1));
            buff.Add(new IntBuffMultiplier(2));

            buff.Clear();

            Assert.AreEqual(0, buff.Count);
            Assert.AreEqual(3, buff.Calculate());
        }

        [Test]
        public void TryGetProcessor_ReturnsRegisteredInstance() {
            var buff = new Buff<int>(0);
            var id = Guid.NewGuid();
            var processor = new IntBuffAdder(1);
            buff.Add(id, processor);

            Assert.IsTrue(buff.TryGetProcessor(id, out var found));
            Assert.AreSame(processor, found);
            Assert.IsFalse(buff.TryGetProcessor(Guid.NewGuid(), out found));
            Assert.IsNull(found);
        }

        [Test]
        public void Changed_IsRaisedOnEveryModification() {
            var buff = new Buff<int>(0);
            var count = 0;
            buff.Changed += () => count++;
            var processor = new IntBuffAdder(1);

            var handle = buff.Add(processor);            // 1
            buff.Replace(handle.Id, new IntBuffAdder(2)); // 2
            processor.NotifyChanged();                    // removed by replace: no event
            buff.SetOriginalValue(5);                     // 3
            handle.Remove();                              // 4
            buff.Clear();                                 // empty: no event

            Assert.AreEqual(4, count);
        }

        [Test]
        public void ProcessorChange_RaisesBuffChanged() {
            var buff = new Buff<int>(1);
            var processor = new BuffProcessor<int>(value => value + 1);
            buff.Add(processor);
            var raised = false;
            buff.Changed += () => raised = true;

            processor.ProcessCallback = value => value + 2;

            Assert.IsTrue(raised);
            Assert.AreEqual(3, buff.Calculate());
        }

        [Test]
        public void Cache_ReturnsCachedValueUntilChanged() {
            var external = 1;
            var buff = new Buff<int>(0) { CacheEnabled = true };
            var processor = new BuffProcessor<int>(value => value + external);
            buff.Add(processor);

            Assert.AreEqual(1, buff.Calculate());
            external = 2;
            Assert.AreEqual(1, buff.Calculate(), "Cached value is expected.");

            processor.NotifyChanged();
            Assert.AreEqual(2, buff.Calculate());

            buff.SetOriginalValue(10);
            Assert.AreEqual(12, buff.Calculate());

            buff.Add(new IntBuffMultiplier(2));
            Assert.AreEqual(24, buff.Calculate());
        }

        [Test]
        public void Cache_Disabled_RecalculatesEveryTime() {
            var external = 1;
            var buff = new Buff<int>(0);
            buff.Add(value => value + external);

            Assert.AreEqual(1, buff.Calculate());
            external = 2;
            Assert.AreEqual(2, buff.Calculate());
        }

        [Test]
        public void ModificationDuringCalculation_IsAppliedOnNextCalculation() {
            var buff = new Buff<int>(0) { CacheEnabled = true };
            var added = false;
            buff.Add(value => {
                if (!added) {
                    added = true;
                    buff.Add(v => v + 100);
                }
                return value + 1;
            });

            Assert.AreEqual(1, buff.Calculate());
            Assert.AreEqual(101, buff.Calculate(), "Value calculated during modification must not be cached.");
        }

        [Test]
        public void RemovalDuringCalculation_DoesNotThrow() {
            var buff = new Buff<int>(0);
            BuffHandle self = default;
            self = buff.Add(value => {
                self.Remove();
                return value + 1;
            });
            buff.Add(value => value + 10);

            Assert.AreEqual(11, buff.Calculate());
            Assert.AreEqual(10, buff.Calculate());
        }

        [Test]
        public void RecursiveCalculation_Throws() {
            Buff<int> buff = null;
            buff = new Buff<int>(() => buff.Calculate());

            Assert.Throws<InvalidOperationException>(() => buff.Calculate());
            buff.SetOriginalValue(1);
            Assert.AreEqual(1, buff.Calculate(), "Buff must stay usable after the exception.");
        }

        [Test]
        public void ChainedBuffs_Work() {
            var walk = new Buff<float>(2f);
            var run = new Buff<float>(() => walk.Calculate() * 3f);
            walk.Add(new FloatBuffMultiplier(0.5f));

            Assert.AreEqual(3f, run.Calculate());
        }

        [Test]
        public void IBuff_CalculateObject_ReturnsBoxedResult() {
            IBuff buff = new Buff<int>(2);
            ((Buff<int>)buff).Add(new IntBuffAdder(3));

            Assert.AreEqual(typeof(int), buff.ValueType);
            Assert.AreEqual(5, buff.CalculateObject());
        }

        [TestCase(2.5f, BuffRounding.Nearest, 5)]
        [TestCase(1.5f, BuffRounding.Nearest, 3)]
        [TestCase(1.3f, BuffRounding.Floor, 2)]
        [TestCase(1.3f, BuffRounding.Ceiling, 3)]
        [TestCase(-1.3f, BuffRounding.Truncate, -2)]
        [TestCase(-1.3f, BuffRounding.Floor, -3)]
        public void IntBuffMultiplier_FractionalMultiplier_Rounds(float multiplier, BuffRounding rounding, int expected) {
            var buff = new Buff<int>(2);
            buff.Add(new IntBuffMultiplier(multiplier, rounding));

            Assert.AreEqual(expected, buff.Calculate());
        }

        [Test]
        public void IntBuffMultiplier_IntMultiplier_IsExact() {
            var buff = new Buff<int>(7);
            buff.Add(new IntBuffMultiplier(3));

            Assert.AreEqual(21, buff.Calculate());
        }
    }
}
