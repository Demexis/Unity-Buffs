using System;

namespace Demegraunt.Framework {
    /// <summary>
    /// Handle of a processor registered in a buff. Returned by <see cref="Buff{T}.Add(BuffProcessor{T}, int?, object)"/>.<br/><br/>
    /// Disposing the handle removes the processor from the buff. Disposing it more than once is safe.
    /// </summary>
    public readonly struct BuffHandle : IDisposable, IEquatable<BuffHandle> {
        /// <summary>
        /// The buff in which the processor is registered. Null for a default handle.
        /// </summary>
        public IBuff Buff { get; }

        /// <summary>
        /// The processor ID.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Whether the processor is still registered in the buff.
        /// </summary>
        public bool IsActive => Buff != null && Buff.Contains(Id);

        public BuffHandle(IBuff buff, Guid id) {
            Buff = buff;
            Id = id;
        }

        /// <summary>
        /// Remove the processor from the buff.
        /// </summary>
        /// <returns>true if the processor was found and removed; otherwise, false.</returns>
        public bool Remove() {
            return Buff != null && Buff.Remove(Id);
        }

        /// <summary>
        /// Same as <see cref="Remove"/>.
        /// </summary>
        public void Dispose() {
            Remove();
        }

        public bool Equals(BuffHandle other) {
            return ReferenceEquals(Buff, other.Buff) && Id.Equals(other.Id);
        }

        public override bool Equals(object obj) {
            return obj is BuffHandle other && Equals(other);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public static bool operator ==(BuffHandle left, BuffHandle right) {
            return left.Equals(right);
        }

        public static bool operator !=(BuffHandle left, BuffHandle right) {
            return !left.Equals(right);
        }
    }
}
