namespace Restless.Vigil
{
    public static class IncompatibilityChecker
    {
        public static bool AreCompatible(AllyData a, AllyData b)
        {
            if (a == null || b == null) return true;
            if (a == b) return false;

            foreach (var blocked in a.incompatibleWith)
                if (blocked == b) return false;

            foreach (var blocked in b.incompatibleWith)
                if (blocked == a) return false;

            return true;
        }
    }
}
