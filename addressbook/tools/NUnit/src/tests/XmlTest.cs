#region Copyright (c) 2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;	
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using NUnit.Core;
using NUnit.Framework;

namespace NUnit.Tests.Core
{
	/// <summary>
	/// Summary description for XmlTest.
	/// </summary>
	/// 
	[TestFixture]
	public class XmlTest : FixtureBase
	{
		public class SchemaValidator
		{
			private XmlValidatingReader myXmlValidatingReader;
			private bool success;

			public SchemaValidator(string xmlFile, string schemaFile)
			{
				XmlSchemaCollection myXmlSchemaCollection = new XmlSchemaCollection();
				myXmlSchemaCollection.Add(XmlSchema.Read(new XmlTextReader(schemaFile), null));

				// Validate the XML file with the schema
				XmlTextReader myXmlTextReader = new XmlTextReader (xmlFile);
				myXmlValidatingReader = new XmlValidatingReader(myXmlTextReader);
				myXmlValidatingReader.Schemas.Add(myXmlSchemaCollection);
				myXmlValidatingReader.ValidationType = ValidationType.Schema;
			}

			public bool Validate()
			{
				success = true;

				try
				{
					// Set the validation event handler
					myXmlValidatingReader.ValidationEventHandler += new ValidationEventHandler (this.ValidationEventHandle);

					// Read XML data
					while (myXmlValidatingReader.Read()){}
				}
				catch (Exception e)
				{
					throw new NunitException(e.Message, e);
				}
				finally
				{
					myXmlValidatingReader.Close();
				}

				return success;
			}

			public void ValidationEventHandle (object sender, ValidationEventArgs args)
			{
				success = false;
				Console.WriteLine("\tValidation error: " + args.Message);

				if (args.Severity == XmlSeverityType.Warning)
				{
					Console.WriteLine("No schema found to enforce validation.");
				} 
				else
					if (args.Severity == XmlSeverityType.Error)
				{
					Console.WriteLine("validation error occurred when validating the instance document.");
				} 

				if (args.Exception != null) // XSD schema validation error
				{
					Console.WriteLine(args.Exception.SourceUri + "," +  args.Exception.LinePosition + "," +  args.Exception.LineNumber);
				}
			}
		}

		private void runSchemaValidatorTest(string reportFileName, CultureInfo testCulture)
		{
			// Preserve current culture
			CultureInfo previousCulture = Thread.CurrentThread.CurrentCulture;

			// Enable test culture
			Thread.CurrentThread.CurrentCulture = testCulture;

			try
			{
				string testsDll = "mock-assembly.dll";
				TestSuiteBuilder builder = new TestSuiteBuilder();
				TestSuite suite = builder.Build(testsDll);

				TestResult result = suite.Run(NullListener.NULL);

                XmlResultVisitor visitor = new XmlResultVisitor(reportFileName, result);
				result.Accept(visitor);
				visitor.Write();

				SchemaValidator validator = new SchemaValidator(reportFileName, schemaFile);
				Assert.IsTrue(validator.Validate(), "validate failed");
			}
			finally
			{
				// Restore previous culture
				Thread.CurrentThread.CurrentCulture = previousCulture;
			}
		}

		private void runSchemaValidatorTest(TextWriter writer, CultureInfo testCulture)
		{
			// Preserve current culture
			CultureInfo previousCulture = Thread.CurrentThread.CurrentCulture;

			// Enable test culture
			Thread.CurrentThread.CurrentCulture = testCulture;

			try
			{
				string testsDll = "mock-assembly.dll";
				TestSuiteBuilder builder = new TestSuiteBuilder();
				TestSuite suite = builder.Build(testsDll);

				TestResult result = suite.Run(NullListener.NULL);

				XmlResultVisitor visitor = new XmlResultVisitor(writer, result);
				result.Accept(visitor);
				visitor.Write();

				//SchemaValidator validator = new SchemaValidator(reportFileName, schemaFile);
				//Assert.True(validator.Validate(), "validate failed");
			}
			finally
			{
				// Restore previous culture
				Thread.CurrentThread.CurrentCulture = previousCulture;
			}
		}

		private string tempFile;
		private string schemaFile;

		[SetUp]
		public void CreateTempFileName()
		{
			tempFile = "temp" + Guid.NewGuid().ToString() + ".xml";
			schemaFile = SourcePath + @"\framework\results.xsd";
		}

		[TearDown]
		public void RemoveTempFile()
		{
			FileInfo info = new FileInfo(tempFile);
			if(info.Exists) info.Delete();
		}

		[Test]
		public void TestSchemaValidatorInvariantCulture()
		{
			runSchemaValidatorTest(tempFile, CultureInfo.InvariantCulture);
		}

		[Test]
		public void TestSchemaValidatorUnitedStatesCulture()
		{
			CultureInfo unitedStatesCulture = new CultureInfo("en-US", false);
			runSchemaValidatorTest(tempFile,unitedStatesCulture);
		}

		[Test]
		public void TestStream()
		{
			CultureInfo unitedStatesCulture = new CultureInfo("en-US", false);
			runSchemaValidatorTest(tempFile,unitedStatesCulture);
			StringBuilder builder = new StringBuilder();
			StringWriter writer = new StringWriter(builder);
			runSchemaValidatorTest(writer,unitedStatesCulture);
			string second = builder.ToString();
			StreamReader reader = new StreamReader(tempFile);
			string first = reader.ReadToEnd();
			reader.Close();
			Assert.AreEqual(removeTimeAttributes(first), removeTimeAttributes(second));
		}

		[Test]
		public void TestSchemaValidatorFrenchCulture()
		{
			CultureInfo frenchCulture = new CultureInfo("fr-FR", false);
			runSchemaValidatorTest(tempFile, frenchCulture);
		}

		[Test]
		public void removeTime() 
		{
			string input = "foo time=\"123.745774xxx\" bar time=\"0\"";
			string output = removeTimeAttributes(input);
			Assert.AreEqual("foo  bar ", output);
		}

		private string removeTimeAttributes(string text) 
		{
			int index = 0;
			while ((index = text.IndexOf("time=\"")) != -1) 
			{
				int endQuote = text.IndexOf("\"", index + 7);
				text = text.Remove(index, endQuote - index + 1);
			}

			return text;
		}
	}
}
