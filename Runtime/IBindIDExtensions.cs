using MontanGames.Data.Core;

namespace MontanaGames
{
    public static class DataTypeExtensions
    {
        public static string ToEntityID(this IBindID self)
        {
            return string.Concat(self.Type, self.ID);
        }
        public static string ToEntityID(this IBindID self, object id)
        {
            return string.Concat(self.Type, id);
        }
        public static string ToEntityID(this int self, IBindID type) => type.ToEntityID(self);
    }
}
