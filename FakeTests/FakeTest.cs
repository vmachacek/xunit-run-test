using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using Xunit;

namespace Profiles.Tests.Smoke
{
    public class FakeTest
    {
        [Fact]
        public void Should_fake_test()
        {
            true.ShouldBe(true);
        }
    }
}
