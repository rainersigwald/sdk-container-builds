﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Containers.Tasks;

#nullable disable

namespace Test.System.Containers.Tasks
{
    [TestClass]
    public class ParseContainerPropertiesTests
    {
        [TestMethod]
        public void Baseline()
        {
            ParseContainerProperties task = new ParseContainerProperties();
            task.ContainerBaseImage = "https://mcr.microsoft.com/dotnet/runtime:6.0";
            task.ContainerImageName = "dotnet/testimage";
            task.ContainerImageTag = "5.0";

            Assert.IsTrue(task.Execute());
            Assert.AreEqual("https://mcr.microsoft.com", task.ParsedContainerRegistry);
            Assert.AreEqual("dotnet/runtime", task.ParsedContainerImage);
            Assert.AreEqual("6.0", task.ParsedContainerTag);

            Assert.AreEqual("dotnet/testimage", task.NewImageName);
            Assert.AreEqual("5.0", task.NewImageTag);
        }

        [TestMethod]
        public void RegistriesWithNoHttpGetHttp()
        {
            ParseContainerProperties task = new ParseContainerProperties();
            task.ContainerBaseImage = "mcr.microsoft.com/dotnet/runtime:6.0";
            task.ContainerImageName = "dotnet/testimage";
            task.ContainerImageTag = "5.0";

            Assert.IsTrue(task.Execute());
            Assert.AreEqual("https://mcr.microsoft.com", task.ParsedContainerRegistry);
            Assert.AreEqual("dotnet/runtime", task.ParsedContainerImage);
            Assert.AreEqual("6.0", task.ParsedContainerTag);

            Assert.AreEqual("dotnet/testimage", task.NewImageName);
            Assert.AreEqual("5.0", task.NewImageTag);
        }
    }
}