using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AnimalAiUnitTests.utilities
{
    public class ExpectedException
    {
        public static T AssertThrows<T>(Action action) where T : Exception
        {
            try
            {
                action.Invoke();
            }
            catch (T ex)
            {
                if (ex.GetType() != typeof(T)) throw;
                return ex;
            }

            Assert.Fail("Failed to throw exception!");
            return null;
        }
    }
}