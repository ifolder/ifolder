/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 *
 ***********************************************************************/
 
using System;
using System.IO;
using System.Xml;
using System.Collections;

/// <summary>
/// Parses test result files and creates a summary report.
/// </summary>
public class Report 
{
	/// <summary>
	/// The test result files suffix.
	/// </summary>
	const string RESULTS_SUFFIX = "*.test.xml";

	/// <summary>
	/// The directory containing the test result files.
	/// </summary>
	string dir;

	/// <summary>
	/// The text writer for the summary report.
	/// </summary>
	TextWriter writer;

	/// <summary>
	/// The total number of test cases.
	/// </summary>
	int totalCases;

	/// <summary>
	/// The total number of test cases that ran.
	/// </summary>
	int totalRuns;

	/// <summary>
	/// The total number of test cases that passed.
	/// </summary>
	int totalPasses;

	/// <summary>
	/// The total time required to run the test cases.
	/// </summary>
	double totalTime;

	/// <summary>
	/// A string list of the number of fails in each test result file.
	/// </summary>
	ArrayList failList;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="dir">The directory with the test result files.</param>
	/// <param name="writer">The writer for the summary report.</param>
	public Report(string dir, TextWriter writer)
	{
		this.dir = dir;
		this.writer = writer;
		this.failList = new ArrayList();
	}

	/// <summary>
	/// Parse the test result files in the directory.
	/// </summary>
	void Parse()
	{
		writer.WriteLine("Parsing test result files...");
		writer.WriteLine();

		if (Directory.Exists(dir))
		{
			String[] files = Directory.GetFiles(dir, RESULTS_SUFFIX);

			foreach(string file in files)
			{
				writer.WriteLine("Parsing: {0}", Path.GetFileName(file));

				ParseTestFile(file);
			}
		}
	}

	/// <summary>
	/// Parse a test result file.
	/// </summary>
	/// <param name="file">The test result file.</param>
	void ParseTestFile(string file)
	{
		int cases = 0;
		int runs = 0;
		int passes = 0;
		int fails = 0;

		// find a component name
		string name = Path.GetFileNameWithoutExtension(file);
		name = Path.GetFileNameWithoutExtension(name);

		// parse the XML
		XmlDocument doc = new XmlDocument();
		doc.Load(file);
		
		foreach (XmlElement node in doc.GetElementsByTagName("test-case"))
		{
			cases++;
	
			if (bool.Parse(node.GetAttribute("executed")))
			{
				runs++;

				if (bool.Parse(node.GetAttribute("success")))
				{
					passes++;
				}
			}

			// not all test-case elementes have a time
			string time = node.GetAttribute("time"); 
			if ((time != null) && (time.Length > 0))
			{
				totalTime += Double.Parse(time);
			}
		}
		
		// fails
		if ((fails = runs - passes) > 0)
		{
			failList.Add(String.Format("{0,4}: {1}", fails, name));
		}

		// sum up
		totalCases += cases;
		totalRuns += runs;
		totalPasses += passes;
	}

	/// <summary>
	/// Write the summary report.
	/// </summary>
	void WriteSummary()
	{
		const string HEADER = "------------ TEST REPORT ------------";
		
		// summary
		writer.WriteLine();
		writer.WriteLine(HEADER);
		writer.WriteLine();
		
		writer.WriteLine("TEST CASE RESULTS");
		writer.WriteLine();
		writer.WriteLine("  Total: {0}", totalCases);
		writer.WriteLine("    Ran: {0}", totalRuns);
		writer.WriteLine(" Passed: {0} [ {1:p1} ]", totalPasses,
			(totalRuns > 0 ? (double)totalPasses/(double)totalRuns : 0));
		writer.WriteLine();
		writer.WriteLine("   Time: {0} seconds", totalTime);
		
		// failed cases
		writer.WriteLine();
		writer.WriteLine("FAILED TEST CASES");
		writer.WriteLine();

		foreach(string fail in failList)
		{
			writer.WriteLine(fail);
		}

		writer.WriteLine();
		writer.WriteLine(HEADER);
		writer.WriteLine();
	}

	/// <summary>
	/// Main method.
	/// </summary>
	/// <param name="args">Command-line arguments.</param>
	/// <returns>The process result.</returns>
	[STAThread]
	static int Main(string[] args)
	{
		// check arguments
		if (args.Length != 1)
		{
			Console.WriteLine("USAGE: Report.exe [directory]");

			return 1;
		}

		// create the report object
		Report report = new Report(args[0], Console.Out);
		
		// parse the test result files
		report.Parse();

		// write the summary report
		report.WriteSummary();

		return 0;
	}
}


