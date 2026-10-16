using NUnit.Framework;
using UnityEngine;

namespace Demegraunt.Framework.Tests {
    public sealed class BuffFieldTests {
        [Test]
        public void Calculate_UsesOriginalValue() {
            var field = new FloatBuffField(2f);
            field.Buff.Add(new FloatBuffMultiplier(3f));

            Assert.AreEqual(6f, field.Calculate());
        }

        [Test]
        public void SetOriginalValue_InvalidatesCacheAndRaisesChanged() {
            var field = new IntBuffField(1);
            field.Buff.CacheEnabled = true;
            field.Buff.Add(new IntBuffAdder(1));
            var raised = false;
            field.Buff.Changed += () => raised = true;
            Assert.AreEqual(2, field.Calculate());

            field.SetOriginalValue(10);

            Assert.IsTrue(raised);
            Assert.AreEqual(10, field.OriginalValue);
            Assert.AreEqual(11, field.Calculate());
        }

        [Test]
        public void GenericField_CanBeUsedDirectly() {
            var field = new BuffField<int>(4);
            field.Buff.Add(new IntBuffMultiplier(2));

            Assert.AreEqual(8, field.Calculate());
        }

        [Test]
        public void SerializedData_KeepsFormatOfPreviousVersions() {
            // Data saved by previous versions of the package must still be loaded.
            var field = JsonUtility.FromJson<FloatBuffField>("{\"<OriginalValue>k__BackingField\":7.5}");

            Assert.AreEqual(7.5f, field.OriginalValue);
            StringAssert.Contains("<OriginalValue>k__BackingField", JsonUtility.ToJson(field));
        }
    }
}
