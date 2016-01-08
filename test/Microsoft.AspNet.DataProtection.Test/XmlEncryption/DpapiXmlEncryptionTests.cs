// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Xml.Linq;
using Microsoft.AspNet.DataProtection.Test.Shared;
using Microsoft.AspNet.Testing;
using Microsoft.AspNet.Testing.xunit;
using Xunit;

namespace Microsoft.AspNet.DataProtection.XmlEncryption
{
    public class DpapiXmlEncryptionTests
    {
        [ConditionalTheory]
        [ConditionalRunTestOnlyOnWindows]
        [InlineData(true)]
        [InlineData(false)]
        public void Encrypt_CurrentUserOrLocalMachine_Decrypt_RoundTrips(bool protectToLocalMachine)
        {
            // Arrange
            var originalXml = XElement.Parse(@"<mySecret value='265ee4ea-ade2-43b1-b706-09b259e58b6b' />");
            var encryptor = new DpapiXmlEncryptor(protectToLocalMachine);
            var decryptor = new DpapiXmlDecryptor();

            // Act & assert - run through encryptor and make sure we get back an obfuscated element
            var encryptedXmlInfo = encryptor.Encrypt(originalXml);
            Assert.Equal(typeof(DpapiXmlDecryptor), encryptedXmlInfo.DecryptorType);
            Assert.DoesNotContain("265ee4ea-ade2-43b1-b706-09b259e58b6b", encryptedXmlInfo.EncryptedElement.ToString(), StringComparison.OrdinalIgnoreCase);

            // Act & assert - run through decryptor and make sure we get back the original value
            var roundTrippedElement = decryptor.Decrypt(encryptedXmlInfo.EncryptedElement);
            XmlAssert.Equal(originalXml, roundTrippedElement);
        }

#if !DNXCORE50
        [ConditionalFact]
        [ConditionalRunTestOnlyOnWindows]
        public void Encrypt_CurrentUser_Decrypt_ImpersonatedAsAnonymous_Fails()
        {
            // Arrange
            var originalXml = XElement.Parse(@"<mySecret value='265ee4ea-ade2-43b1-b706-09b259e58b6b' />");
            var encryptor = new DpapiXmlEncryptor(protectToLocalMachine: false);
            var decryptor = new DpapiXmlDecryptor();

            // Act & assert - run through encryptor and make sure we get back an obfuscated element
            var encryptedXmlInfo = encryptor.Encrypt(originalXml);
            Assert.Equal(typeof(DpapiXmlDecryptor), encryptedXmlInfo.DecryptorType);
            Assert.DoesNotContain("265ee4ea-ade2-43b1-b706-09b259e58b6b", encryptedXmlInfo.EncryptedElement.ToString(), StringComparison.OrdinalIgnoreCase);

            // Act & assert - run through decryptor (while impersonated as anonymous) and verify failure
            ExceptionAssert2.ThrowsCryptographicException(() =>
                AnonymousImpersonation.Run(() => decryptor.Decrypt(encryptedXmlInfo.EncryptedElement)));
        }
#endif
    }
}
