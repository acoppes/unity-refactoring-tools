using Gemserk.RefactorTools.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Gemserk.RefactorTools.Tests
{
    public class AssetDatabaseExtTests
    {
        [Test]
        public void GetFilter_ReturnProperSearchString()
        {
            Assert.AreEqual("t:Texture2D", AssetDatabaseExt.GetSearchFilter<Texture2D>());
        }
    }
}
