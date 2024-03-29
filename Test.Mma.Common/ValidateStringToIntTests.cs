﻿using Mma.Common.Exceptions;
using Mma.Common.Interfaces;
using Mma.Common.IServices;
using Mma.Common.Validators;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Mma.Common {
    [TestFixture]
    internal class Validate_string_to_int_tests {
        private Mock<ILoggingService> _mockLoggingService;
        private IParsingValidator _parsingValidator;

        [SetUp]
        public void SetUp() {
            _mockLoggingService = new Mock<ILoggingService>();
            _parsingValidator = new ParsingValidator(_mockLoggingService.Object);
        }

        [Test]
        public void Validate_string_to_int_with_valid_int_string_does_not_throw_exception() {
            string validIntString = "123";
            string parameterName = "testParameter";

            Assert.DoesNotThrow(() => _parsingValidator.ValidateStringToInt(validIntString, parameterName));
        }

        [Test]
        public void Validate_string_to_int_with_invalid_int_string_throws_parsing_exception() {
            string invalidIntString = "abc";
            string parameterName = "testParameter";

            var ex = Assert.Throws<ParsingException>(() => _parsingValidator.ValidateStringToInt(invalidIntString, parameterName));
            Assert.AreEqual($"Failed to parse '{parameterName}' as an integer. Value: {invalidIntString}", ex.Message);
        }
    }
}
